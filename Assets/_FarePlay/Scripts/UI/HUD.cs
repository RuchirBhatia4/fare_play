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

        [Header("Boost and load (the twist)")]
        [Tooltip("Filled Image, like the fuel bar.")]
        [SerializeField] Image boostFill;
        [Tooltip("Small text next to the boost bar: 'BOOST' / 'BOOST READY' / 'BOOSTING'.")]
        [SerializeField] TMP_Text boostText;
        [Tooltip("Shows how heavy the bus is: 'Load 6  (heavier)'.")]
        [SerializeField] TMP_Text loadText;
        [Tooltip("How long a popup ('PERFECT STOP!') stays in the middle of the screen.")]
        [SerializeField] float popupSeconds = 1.8f;
 
        [Header("Look")]
        [SerializeField] float lowTimeWarningSeconds = 15f;
        [SerializeField] Color normalTimeColor = Color.white;
        [SerializeField] Color lowTimeColor = new Color(1f, 0.3f, 0.3f);
 
        string popup;
        float popupUntil;

        void Awake()
        {
            if (timer == null) timer = GetComponentInParent<RaceTimer>();
            if (score == null) score = GetComponentInParent<ScoreManager>();
        }

        void OnEnable()  { GameEvents.Popup += HandlePopup; }
        void OnDisable() { GameEvents.Popup -= HandlePopup; }

        void HandlePopup(string message)
        {
            popup = message;
            popupUntil = Time.time + popupSeconds;
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
 
            if (scoreText != null && score != null)      scoreText.text = $"Score {score.LiveScore:N0}";
            if (passengersText != null && score != null) passengersText.text = $"Passengers {score.PassengersOnBoard}";
            if (routeText != null)                       routeText.text = gm.ActiveRoute != null ? gm.ActiveRoute.DisplayName : "";
            if (messageText != null)
            {
                bool showPopup = gm.State == GameState.Driving && Time.time < popupUntil;
                messageText.text = showPopup ? popup : MessageFor(gm);
            }

            UpdateSignal(gm);
            UpdateFuel();
            UpdateBoostAndLoad();
        }

        void UpdateBoostAndLoad()
        {
            BusBoost boost = BusBoost.Current;
            float fill = boost != null ? boost.Fill01 : 0f;
            bool boosting = boost != null && boost.Boosting;

            if (boostFill != null)
            {
                boostFill.fillAmount = fill;
                Color idle = new Color(0.25f, 0.6f, 1f);
                Color hot = new Color(0.55f, 0.95f, 1f);
                boostFill.color = boosting ? hot : fill >= 0.99f ? Color.Lerp(idle, hot, Mathf.PingPong(Time.time * 3f, 1f)) : idle;
            }

            if (boostText != null)
                boostText.text = boosting ? "BOOSTING"
                               : boost != null && boost.Regenerating ? "REGEN +"
                               : fill >= 0.99f ? "BOOST READY"
                               : fill > 0f ? "BOOST (Shift)" : "BOOST";

            if (loadText != null)
            {
                BusController bus = BusController.Current;
                int load = bus != null ? bus.PassengersAboard : 0;
                GameTuning tuning = GameManager.Instance != null ? GameManager.Instance.Tuning : null;
                int handlingLoss = tuning != null ? Mathf.RoundToInt(Mathf.Min(0.8f, tuning.weightAccelerationPerPassenger * load) * 100f) : 0;
                loadText.text = load == 0 ? "Load: empty"
                              : $"Load: {load}  <size=80%><color=#FFB36B>handling -{handlingLoss}%</color></size>";
            }
        }

        void UpdateFuel()
        {
            if (fuelFill == null) return;
            BusFuel fuel = BusFuel.Current;
            float amount = fuel != null ? fuel.Fuel01 : 1f;
            fuelFill.fillAmount = amount;
            fuelFill.color = amount > 0.5f ? Color.green : amount > 0.2f ? Color.yellow : Color.red;
        }
 
        static string MessageFor(GameManager gm)
        {
            switch (gm.State)
            {
                case GameState.Overview:
                    return $"Driving starts in {Mathf.CeilToInt(gm.OverviewSecondsLeft)}s\n" +
                           "<size=55%>Space: skip  |  Arrows: drive  |  Shift: boost  |  C: top view</size>";
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