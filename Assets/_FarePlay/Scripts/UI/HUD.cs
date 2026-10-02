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
        [Tooltip("The word BOOST on the boost bar.")]
        [SerializeField] TMP_Text boostText;
        [Tooltip("Shows how heavy the bus is: 'Load 6  (heavier)'.")]
        [SerializeField] TMP_Text loadText;
        [Tooltip("How long a popup ('PERFECT STOP!') stays in the middle of the screen.")]
        [SerializeField] float popupSeconds = 1.8f;

        [Header("Overview: rules and controls (made automatically if left empty)")]
        [SerializeField] GameObject rulesPanel;
        [SerializeField] GameObject controlsPanel;
 
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

            // The boost label sits inside a small bar: one line only, shrinking to fit if needed.
            if (boostText != null)
            {
                boostText.textWrappingMode = TextWrappingModes.NoWrap;
                boostText.enableAutoSizing = true;
                boostText.fontSizeMin = 8f;
                boostText.fontSizeMax = Mathf.Max(boostText.fontSize, 8f);
            }

            // Shown on the left and right of the top-down overview, where the screen is empty.
            if (rulesPanel == null)    rulesPanel    = CreateSidePanel("RulesPanel", left: true, 560f, RulesText);
            if (controlsPanel == null) controlsPanel = CreateSidePanel("ControlsPanel", left: false, 470f, ControlsText);
        }

        const string RulesText =
            "<b><size=120%>HOW TO PLAY</size></b>\n" +
            "<color=#FFD54A>Reach the FINISH gate before the timer runs out.</color>\n\n" +
            "- <b>Pick up passengers:</b> stop fully inside each bus stop bay.  <color=#7CFC9A>+100</color> each\n" +
            "- <b>Brakes are your engine:</b> a straight, centred stop is a <color=#7CFC9A>PERFECT STOP</color> and charges boost. Perfect stops in a row build a combo. Hard braking charges boost too.\n" +
            "- <b>Every passenger makes the bus heavier:</b> slower to speed up, longer to stop, wider turns.\n" +
            "- <b>Boost</b> to fight the weight, but it burns fuel 3x faster.\n" +
            "- <b>Red light:</b> <color=#FF7B7B>-50</color>, unless you boost through it.\n" +
            "- <b>Hits</b> (cars, walls, obstacles): <color=#FF7B7B>-50</color>\n" +
            "- <b>Fuel</b> refills only while parked in a bay. Run dry and stop: game over.\n" +
            "- Late to a stop? Passengers give up and leave.\n" +
            "- <b>Time bonus:</b> <color=#7CFC9A>+10</color> per second left (to the millisecond).";

        const string ControlsText =
            "<b><size=120%>CONTROLS</size></b>\n\n" +
            "<b>Up</b><pos=42%>Accelerate\n" +
            "<b>Down</b><pos=42%>Brake, then reverse\n" +
            "<b>Left / Right</b><pos=42%>Steer\n" +
            "<b>Shift</b><pos=42%>Boost\n" +
            "<b>C</b><pos=42%>Top view (line up with bays)\n" +
            "<b>Space</b><pos=42%>Start now\n" +
            "<b>R</b><pos=42%>Restart\n" +
            "<b>P</b><pos=42%>Play again (results)";

        /// <summary>A dark rounded-off box with text, docked to the left or right edge (1920x1080 layout).</summary>
        GameObject CreateSidePanel(string panelName, bool left, float width, string content)
        {
            var panel = new GameObject(panelName, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            var rect = (RectTransform)panel.transform;
            float x = left ? 0f : 1f;
            rect.anchorMin = rect.anchorMax = new Vector2(x, 0.5f);
            rect.pivot = new Vector2(x, 0.5f);
            rect.anchoredPosition = new Vector2(left ? 30f : -30f, -20f);
            rect.sizeDelta = new Vector2(width, 680f);
            var background = panel.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.6f);
            background.raycastTarget = false;

            var textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(panel.transform, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 20f);
            textRect.offsetMax = new Vector2(-24f, -20f);
            var text = textObject.AddComponent<TextMeshProUGUI>();
            if (messageText != null) text.font = messageText.font;
            text.enableAutoSizing = true;   // shrink to fit on smaller screens
            text.fontSizeMin = 14f;
            text.fontSizeMax = 24f;
            text.lineSpacing = 6f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;
            text.text = content;

            panel.SetActive(false);
            return panel;
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

            bool overview = gm.State == GameState.Overview;
            if (rulesPanel != null)    rulesPanel.SetActive(overview);
            if (controlsPanel != null) controlsPanel.SetActive(overview);
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
                boostText.text = "BOOST";

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
                           "<size=55%>Study the route.  Press Space to start now</size>";
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