using TMPro;
using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE B. The end-of-run panel with the score breakdown and a Restart button.
    /// The breakdown is laid out in columns with TextMeshPro &lt;pos&gt; tags, so set the
    /// Breakdown text's alignment to LEFT (top-left) in the Inspector.
    /// Issue: "Results screen: win/fail, score breakdown, restart"
    /// </summary>
    public class ResultsScreen : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text breakdownText;

        const string Good  = "#7CFC9A";
        const string Bad   = "#FF7B7B";
        const string Muted = "#9AA0A6";

        void Awake()
        {
            if (panel != null) panel.SetActive(false);
        }

        void OnEnable()  { GameEvents.RunEnded += Show; }
        void OnDisable() { GameEvents.RunEnded -= Show; }

        void Show(RunResult result)
        {
            if (panel != null) panel.SetActive(true);

            if (titleText != null)
                titleText.text = result.Won
                    ? $"<color={Good}>Route complete!</color>"
                    : $"<color={Bad}>{result.Reason}</color>";

            if (breakdownText == null) return;

            GameTuning tuning = GameManager.Instance != null ? GameManager.Instance.Tuning : null;
            int perPassenger = tuning != null ? tuning.pointsPerPassenger : 100;
            int perSecond    = tuning != null ? tuning.timeBonusPerSecond : 10;
            int perRed       = tuning != null ? tuning.redLightPenalty : 50;
            int perHit       = tuning != null ? tuning.collisionPenalty : 25;
            int seconds      = Mathf.FloorToInt(result.SecondsRemaining);

            string text =
                Row("Passengers", $"{result.PassengersDelivered} × {perPassenger}", result.PassengerPoints) +
                Row("Time bonus", $"{seconds} s × {perSecond}", result.TimeBonus) +
                Row("Red lights", $"{result.RedLightViolations} × -{perRed}", result.RedLightPoints) +
                Row("Collisions", $"{result.Collisions} × -{perHit}", result.CollisionPoints) +
                $"<color={Muted}>________________________________________</color>\n" +
                $"<size=130%><b>TOTAL<pos=72%>{result.Total}</b></size>";

            if (!result.Won)
                text += $"\n<size=80%><color={Muted}>Failed runs score 0</color></size>";

            BusBoost boost = BusBoost.Current;
            if (boost != null)
                text += $"\n\n<size=85%><color={Muted}>Perfect stops</color>  {boost.PerfectStops}" +
                        $"<pos=50%><color={Muted}>Best combo</color>  x{boost.BestCombo}</size>";

            breakdownText.text = text;
        }

        /// <summary>One line: label, how it was worked out, and the coloured points.</summary>
        static string Row(string label, string detail, int points) =>
            $"{label}<pos=38%><color={Muted}>{detail}</color><pos=72%>{Points(points)}\n";

        static string Points(int value) =>
            value > 0 ? $"<color={Good}>+{value}</color>"
          : value < 0 ? $"<color={Bad}>{value}</color>"
          : $"<color={Muted}>0</color>";
    }
}
