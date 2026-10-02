using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        /// <summary>True while the player is typing their name. GameManager holds off on P-to-play-again.</summary>
        public static bool EnteringName { get; private set; }

        string routeName;
        int newRank = -1;
        string typedName = "";

        void Awake()
        {
            if (panel == null) return;
            panel.SetActive(false);

            // Wire the panel's button to play again, and label it with the key.
            Button button = panel.GetComponentInChildren<Button>(true);
            if (button != null)
            {
                button.onClick.AddListener(() => { if (GameManager.Instance != null) GameManager.Instance.Restart(); });
                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.text = "Press P to play again";
            }
        }

        void OnEnable()  { GameEvents.RunEnded += Show; }
        void OnDisable() { GameEvents.RunEnded -= Show; EnteringName = false; }

        void Update()
        {
            if (!EnteringName) return;

            bool changed = false;
            foreach (char c in Input.inputString)
            {
                if (c == '\b')
                {
                    if (typedName.Length > 0) typedName = typedName.Substring(0, typedName.Length - 1);
                }
                else if (c == '\n' || c == '\r')
                {
                    EnteringName = false;
                    BestScores.SetName(routeName, newRank, typedName);
                }
                else if ((char.IsLetterOrDigit(c) || c == ' ') && typedName.Length < BestScores.MaxNameLength)
                {
                    typedName += char.ToUpperInvariant(c);
                }
                changed = true;
            }

            // Redraw while typing (the cursor blinks).
            if (changed || EnteringName) ShowBestScores(routeName, newRank, true);
        }

        void Show(RunResult result)
        {
            if (panel != null) panel.SetActive(true);

            // Save completed runs to this browser's best scores, then ask for a name if it made the list.
            routeName = result.RouteName;
            newRank = result.Won ? BestScores.Record(routeName, result) : -1;
            typedName = BestScores.LastName.ToUpperInvariant();
            EnteringName = newRank >= 0 && bestScoresText != null;
            ShowBestScores(routeName, newRank, result.Won);

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
                $"<size=130%><b>TOTAL<pos=72%>{result.Total:N2}</b></size>";

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
                bool typingHere = i == rank && EnteringName;
                string cursor = Mathf.Repeat(Time.unscaledTime, 1f) < 0.5f ? "_" : " ";
                string name = typingHere ? typedName + cursor : e.name;
                string line = $"{i + 1}. {name}<pos=46%>{e.score:N2}<pos=76%><size=80%>{e.secondsLeft:0.000}s</size>";
                text += (i == rank ? $"<color={Gold}>{line}</color>" : line) + "\n";
            }

            if (EnteringName)
                text += $"\n<color={Gold}><size=80%>Type your name, press Enter to save</size></color>";
            else if (won && rank < 0)
                text += $"\n<size=75%><color={Muted}>Not in your top {BestScores.MaxEntries} this time</color></size>";

            bestScoresText.text = text;
        }

        /// <summary>One line: label, how it was worked out, and the coloured points.</summary>
        static string Row(string label, string detail, float points) =>
            $"{label}<pos=38%><color={Muted}>{detail}</color><pos=72%>{Points(points)}\n";

        /// <summary>Whole numbers stay whole (+300); the time bonus keeps its decimals (+98.76).</summary>
        static string Points(float value)
        {
            string number = Mathf.Approximately(value, Mathf.Round(value)) ? value.ToString("N0") : value.ToString("N2");
            return value > 0 ? $"<color={Good}>+{number}</color>"
                 : value < 0 ? $"<color={Bad}>{number}</color>"
                 : $"<color={Muted}>0</color>";
        }
    }
}
