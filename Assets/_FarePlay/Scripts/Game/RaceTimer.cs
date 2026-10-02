using UnityEngine;
 
namespace FarePlay
{
    /// <summary>
    /// LANE B. Counts down the active route's time limit once the bus crosses a start line,
    /// and fails the run at 0:00. Lives on the GameSystems prefab.
    /// </summary>
    public class RaceTimer : MonoBehaviour
    {
        [Tooltip("What the HUD shows before the run starts.")]
        [SerializeField] float secondsShownBeforeStart = 90f;
 
        public float SecondsLeft { get; private set; }
        public bool Running { get; private set; }
 
        void Awake()
        {
            SecondsLeft = secondsShownBeforeStart;
        }
 
        void OnEnable()
        {
            GameEvents.RunStarted += HandleRunStarted;
            GameEvents.StateChanged += HandleStateChanged;
        }
 
        void OnDisable()
        {
            GameEvents.RunStarted -= HandleRunStarted;
            GameEvents.StateChanged -= HandleStateChanged;
        }
 
        void HandleRunStarted(Route route)
        {
            SecondsLeft = route.TimeLimitSeconds;
            Running = true;
        }
 
        void HandleStateChanged(GameState state)
        {
            if (state == GameState.Won || state == GameState.Failed) Running = false;
        }
 
        void Update()
        {
            if (!Running) return;
 
            SecondsLeft -= Time.deltaTime;
            if (SecondsLeft <= 0f)
            {
                SecondsLeft = 0f;
                Running = false;
                if (GameManager.Instance != null) GameManager.Instance.FailRun("Out of time");
            }
        }
 
        /// <summary>"1:05"-style text for the HUD.</summary>
        public string Formatted()
        {
            // To the thousandth: "1:29.874". Fixed-width digits so the timer doesn't wobble;
            // the milliseconds are drawn smaller, racing-game style.
            int ms = Mathf.FloorToInt(Mathf.Max(0f, SecondsLeft) * 1000f);
            int minutes = ms / 60000;
            int seconds = ms / 1000 % 60;
            int thousandths = ms % 1000;
            return $"<mspace=0.6em>{minutes}:{seconds:00}</mspace><size=60%><mspace=0.6em>.{thousandths:000}</mspace></size>";
        }
    }
}
