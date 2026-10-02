using UnityEngine;
using UnityEngine.SceneManagement;
 
namespace FarePlay
{
    /// <summary>
    /// LANE B. Owns the game state and the life of one run. Lives on the GameSystems prefab,
    /// next to RaceTimer and ScoreManager.
    ///
    ///   Overview --(30 s or Space)--> Ready --(start line)--> Driving --(finish line)--> Won
    ///                                                                 --(timer hits 0)--> Failed
    ///   Won / Failed --(R)--> restart the scene
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
 
        [SerializeField] GameTuning tuning;
        [Tooltip("Leave empty to use the RaceTimer on this same object.")]
        [SerializeField] RaceTimer timer;
        [Tooltip("Leave empty to use the ScoreManager on this same object.")]
        [SerializeField] ScoreManager score;
 
        [Header("Keys")]
        [SerializeField] KeyCode skipOverviewKey = KeyCode.Space;
        [SerializeField] KeyCode restartKey = KeyCode.R;
 
        public GameTuning Tuning => tuning;
        public GameState State { get; private set; } = GameState.Overview;
        public Route ActiveRoute { get; private set; }
        public float OverviewSecondsLeft { get; private set; }
        public RunResult LastResult { get; private set; }
 
        void Awake()
        {
            Instance = this;
            if (timer == null) timer = GetComponent<RaceTimer>();
            if (score == null) score = GetComponent<ScoreManager>();
        }
 
        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
 
        void Start()
        {
            OverviewSecondsLeft = tuning != null ? tuning.overviewSeconds : 30f;
            SetState(GameState.Overview);
        }
 
        void Update()
        {
            switch (State)
            {
                case GameState.Overview:
                    OverviewSecondsLeft -= Time.deltaTime;
                    if (OverviewSecondsLeft <= 0f || Input.GetKeyDown(skipOverviewKey))
                    {
                        OverviewSecondsLeft = 0f;
                        SetState(GameState.Ready);
                    }
                    break;
 
                case GameState.Won:
                case GameState.Failed:
                    if (Input.GetKeyDown(restartKey)) Restart();
                    break;
            }
        }
 
        /// <summary>Called by StartLine. Only the first start line crossed counts.</summary>
        public void StartRun(Route route)
        {
            if (State != GameState.Ready || route == null) return;
 
            ActiveRoute = route;
            SetState(GameState.Driving);
            GameEvents.RaiseRunStarted(route);
            Debug.Log($"[Fare Play] Run started on {route.DisplayName} ({route.TimeLimitSeconds:0} s)");
        }
 
        /// <summary>Called by FinishLine.</summary>
        public void FinishRun()
        {
            if (State != GameState.Driving) return;
            EndRun(true, timer != null ? timer.SecondsLeft : 0f, "Route complete!");
        }
 
        /// <summary>Called by RaceTimer when time runs out.</summary>
        public void FailRun(string reason)
        {
            if (State != GameState.Driving) return;
            EndRun(false, 0f, reason);
        }
 
        void EndRun(bool won, float secondsLeft, string reason)
        {
            RunResult result = score != null
                ? score.BuildResult(won, secondsLeft, reason)
                : new RunResult { Won = won, Reason = reason, SecondsRemaining = secondsLeft };
 
            LastResult = result;
            SetState(won ? GameState.Won : GameState.Failed);
            GameEvents.RaiseRunEnded(result);
            Debug.Log($"[Fare Play] {reason}  Time left: {secondsLeft:0.000} s  Total: {result.Total}");
        }
 
        /// <summary>Reloads the current scene. Also hooked to the results screen's Restart button later.</summary>
        public void Restart()
        {
            Scene scene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
            if (scene.buildIndex < 0)
            {
                // Sandbox scenes aren't in the build's scene list, so reload them the editor way.
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                    scene.path, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            SceneManager.LoadScene(scene.buildIndex);
        }
 
        void SetState(GameState newState)
        {
            State = newState;
            GameEvents.RaiseStateChanged(newState);
        }
    }
}