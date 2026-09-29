using TMPro;
using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE B. The end-of-run panel with the score breakdown and a Restart button.
    /// Issue: "Results screen: win/fail, score breakdown, restart"
    /// </summary>
    public class ResultsScreen : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text breakdownText;

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
                titleText.text = result.Won ? "Route complete!" : result.Reason;

            if (breakdownText == null) return;

            GameTuning tuning = GameManager.Instance != null ? GameManager.Instance.Tuning : null;
            int perPassenger = tuning != null ? tuning.pointsPerPassenger : 100;
            int perSecond = tuning != null ? tuning.timeBonusPerSecond : 10;
            int seconds = Mathf.FloorToInt(result.SecondsRemaining);

            breakdownText.text =
                $"Passengers  {result.PassengersDelivered} x {perPassenger}      {Signed(result.PassengerPoints)}\n" +
                $"Red lights  {result.RedLightViolations}            {Signed(result.RedLightPoints)}\n" +
                $"Collisions  {result.Collisions}            {Signed(result.CollisionPoints)}\n" +
                $"Time bonus  {seconds} s x {perSecond}    {Signed(result.TimeBonus)}\n" +
                $"TOTAL                    {result.Total}";
        }

        static string Signed(int value) => value > 0 ? $"+{value}" : value.ToString();
    }
}
