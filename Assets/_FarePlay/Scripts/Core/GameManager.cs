using UnityEngine;
using UnityEngine.SceneManagement;

namespace FarePlay
{
    /// <summary>
    /// LANE B. Owns the game state and the life of one run. Lives on the GameSystems prefab
    /// next to RaceTimer and ScoreManager.
    /// Issue: "Game flow: Overview -> Ready -> Driving -> Won / Failed"
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] GameTuning tuning;
        [SerializeField] RaceTimer timer;
        [SerializeField] ScoreManager score;

        public GameTuning Tuning => tuning;
        public GameState State { get; private set; } = GameState.Overview;
        public Route ActiveRoute { get; private set; }
        public float OverviewSecondsLeft { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            OverviewSecondsLeft = tuning != null ? tuning.overviewSeconds : 30f;
            SetState(GameState.Overview);
        }

        void Update()
        {
            // TODO: while State == Overview, count OverviewSecondsLeft down with Time.deltaTime.
            // TODO: when it reaches 0, or the player presses Space, SetState(GameState.Ready).
        }

        /// <summary>Called by StartLine. Only the first start line crossed counts.</summary>
        public void StartRun(Route route)
        {
            // TODO: ignore unless State == GameState.Ready (so the second route's line does nothing).
            // TODO: ActiveRoute = route; SetState(GameState.Driving); GameEvents.RaiseRunStarted(route);
        }

        /// <summary>Called by FinishLine.</summary>
        public void FinishRun()
        {
            // TODO: ignore unless State == GameState.Driving.
            // TODO: var result = score.BuildResult(true, timer.SecondsLeft, "Route complete!");
            //       SetState(GameState.Won); GameEvents.RaiseRunEnded(result);
        }

        /// <summary>Called by RaceTimer when time runs out.</summary>
        public void FailRun(string reason)
        {
            // TODO: same as FinishRun, but BuildResult(false, 0f, reason) and SetState(GameState.Failed).
        }

        /// <summary>Hook this to the results screen's Restart button.</summary>
        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void SetState(GameState newState)
        {
            State = newState;
            GameEvents.RaiseStateChanged(newState);
        }
    }
}
