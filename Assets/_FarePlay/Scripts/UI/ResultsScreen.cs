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
        [Tooltip("Optional: 'Your best runs' list (saved in this browser). Put it beside the breakdown.")]
        [SerializeField] TMP_Text bestScoresText;

        const string Good  = "#7CFC9A";
        const string Bad   = "#FF7B7B";
        const string Muted = "#9AA0A6";
        const string Gold  = "#FFD54A";

        void Awake()
        {
            if (panel != null) panel.SetActive(false);
        }

        void OnEnable()  { GameEvents.RunEnded += Show; }
        void OnDisable() { GameEvents.RunEnded -= Show; }

        void Show(RunResult result)
        {
            if (panel != null) panel.SetActive(true);

            // Save completed runs to this browser's best scores, then show the list.
            int rank = result.Won ? BestScores.Record(result.RouteName, result) : -1;
            ShowBestScores(result.RouteName, rank, result.Won);

            if (titleText != null)
                titleText.text = result.Won
                    ? $"<color={Good}>Route complete!</color>"
                    : $"<color={Bad}>{result.Reason}</color>";

            if (breakdownText == null) return;

            GameTuning tuning = GameManager.Instance != null ? GameManager.Instance.Tuning : null;
            int perPassenger = tuning != null ? tuning.pointsPerPassenger : 10000;
            int perRed       = tuning != null ? tuning.redLightPenalty : 5000;
            int perHit       = tuning != null ? tuning.collisionPenalty : 5000;

            string text =
                Row("Passengers", $"{result.PassengersDelivered} × {perPassenger:N0}", result.PassengerPoints) +
                Row("Time bonus", $"{result.SecondsRemaining:0.000} s left", result.TimeBonus) +
                Row("Red lights", $"{result.RedLightViolations} × -{perRed:N0}", result.RedLightPoints) +
                Row("Collisions", $"{result.Collisions} × -{perHit:N0}", result.CollisionPoints) +
                $"<color={Muted}>________________________________________</color>\n" +
                $"<size=130%><b>TOTAL<pos=72%>{result.Total:N0}</b></size>";

            if (!result.Won)
                text += $"\n<size=80%><color={Muted}>Failed runs score 0</color></size>";

            BusBoost boost = BusBoost.Current;
            if (boost != null)
                text += $"\n\n<size=85%><color={Muted}>Perfect stops</color>  {boost.PerfectStops}" +
                        $"<pos=50%><color={Muted}>Best combo</color>  x{boost.BestCombo}</size>";

            breakdownText.text = text;
        }

        void ShowBestScores(string routeName, int rank, bool won)
        {
            if (bestScoresText == null) return;

            var entries = BestScores.Load(routeName);
            string text = rank == 0 ? $"<color={Gold}><b>NEW PERSONAL BEST!</b></color>\n" : "";
            text += $"<b>YOUR BEST RUNS</b>\n<size=70%><color={Muted}>saved in this browser</color></size>\n\n";

            if (entries.Count == 0)
                text += $"<color={Muted}>Finish a run to set a score</color>";

            for (int i = 0; i < entries.Count; i++)
            {
                BestScores.Entry e = entries[i];
                string line = $"{i + 1}.  {e.score:N0}<pos=50%><size=85%>{e.secondsLeft:0.000} s  ·  {e.passengers} pax</size>";
                text += (i == rank ? $"<color={Gold}>{line}  <size=70%>NEW</size></color>" : line) + "\n";
            }

            if (won && rank < 0)
                text += $"\n<size=75%><color={Muted}>Not in your top {BestScores.MaxEntries} this time</color></size>";

            bestScoresText.text = text;
        }

        /// <summary>One line: label, how it was worked out, and the coloured points.</summary>
        static string Row(string label, string detail, int points) =>
            $"{label}<pos=38%><color={Muted}>{detail}</color><pos=72%>{Points(points)}\n";

        static string Points(int value) =>
            value > 0 ? $"<color={Good}>+{value:N0}</color>"
          : value < 0 ? $"<color={Bad}>{value:N0}</color>"
          : $"<color={Muted}>0</color>";
    }
}
