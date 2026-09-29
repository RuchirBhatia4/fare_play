using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
namespace FarePlay
{
    /// <summary>
    /// LANE B. Put on the Canvas inside the GameSystems prefab. Every slot is optional:
    /// leave empty the ones you haven't built yet, and the HUD simply skips them.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [Tooltip("Leave empty to find them on the GameSystems object above.")]
        [SerializeField] RaceTimer timer;
        [SerializeField] ScoreManager score;
 
        [Header("Texts (leave empty the ones you haven't made yet)")]
        [SerializeField] TMP_Text timerText;
        [Tooltip("Big centre text: overview countdown, 'Route complete!', 'Out of time'.")]
        [SerializeField] TMP_Text messageText;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] TMP_Text passengersText;
        [SerializeField] TMP_Text routeText;
 
        [Header("Signal indicator")]
        [Tooltip("A circle Image tinted green / yellow / red.")]
        [SerializeField] Image signalLamp;
        [SerializeField] TMP_Text signalText;
 
        [Header("Fuel (stretch)")]
        [SerializeField] Image fuelFill;
 
        [Header("Look")]
        [SerializeField] float lowTimeWarningSeconds = 15f;
        [SerializeField] Color normalTimeColor = Color.white;
        [SerializeField] Color lowTimeColor = new Color(1f, 0.3f, 0.3f);
 
        void Awake()
        {
            if (timer == null) timer = GetComponentInParent<RaceTimer>();
            if (score == null) score = GetComponentInParent<ScoreManager>();
        }
 
        void Update()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null) return;
 
            if (timerText != null && timer != null)
            {
                timerText.text = timer.Formatted();
                bool low = timer.Running && timer.SecondsLeft <= lowTimeWarningSeconds;
                timerText.color = low ? lowTimeColor : normalTimeColor;
            }
 
            if (scoreText != null && score != null)      scoreText.text = $"Score {score.LiveScore}";
            if (passengersText != null && score != null) passengersText.text = $"Passengers {score.PassengersOnBoard}";
            if (routeText != null)                       routeText.text = gm.ActiveRoute != null ? gm.ActiveRoute.DisplayName : "";
            if (messageText != null)                     messageText.text = MessageFor(gm);
 
            UpdateSignal(gm);
            // Fuel bar (stretch): fuelFill.fillAmount = fuel / tank size.
        }
 
        static string MessageFor(GameManager gm)
        {
            switch (gm.State)
            {
                case GameState.Overview:
                    return $"Driving starts in {Mathf.CeilToInt(gm.OverviewSecondsLeft)}s\n" +
                           "<size=55%>Press Space to skip  |  Arrow keys to drive</size>";
                case GameState.Ready:
                    return "<size=55%>Drive through a start line to begin</size>";
                case GameState.Won:
                case GameState.Failed:
                    return "";
                default:
                    return "";
            }
        }
 
        void UpdateSignal(GameManager gm)
        {
            if (signalLamp == null && signalText == null) return;
 
            TrafficLight next = gm.ActiveRoute != null ? gm.ActiveRoute.NextLight : null;
            if (next == null)
            {
                if (signalLamp != null) signalLamp.color = new Color(1f, 1f, 1f, 0.25f);
                if (signalText != null) signalText.text = "No signals ahead";
                return;
            }
 
            Color c = next.State == SignalState.Green  ? Color.green
                    : next.State == SignalState.Yellow ? Color.yellow
                    : Color.red;
            if (signalLamp != null) signalLamp.color = c;
            if (signalText != null)
                signalText.text = $"{next.State.ToString().ToUpper()} {Mathf.CeilToInt(next.SecondsUntilChange)}s";
        }
    }
}