using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE B. Counts down the active route's time limit (default 1:30) once the bus
    /// crosses a start line. Lives on the GameSystems prefab.
    /// Issue: "Start lines, finish line and the 1:30 route timer"
    /// </summary>
    public class RaceTimer : MonoBehaviour
    {
        public float SecondsLeft { get; private set; }
        public bool Running { get; private set; }

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
            // TODO: SecondsLeft = route.TimeLimitSeconds; Running = true;
        }

        void HandleStateChanged(GameState state)
        {
            // TODO: stop running when the state becomes Won or Failed.
        }

        void Update()
        {
            // TODO: if Running, subtract Time.deltaTime. At <= 0: clamp to 0, stop running,
            //       and call GameManager.Instance.FailRun("Out of time").
        }

        /// <summary>"1:05"-style text for the HUD.</summary>
        public string Formatted()
        {
            int total = Mathf.CeilToInt(Mathf.Max(0f, SecondsLeft));
            return $"{total / 60}:{total % 60:00}";
        }
    }
}
