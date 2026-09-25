using System;

namespace FarePlay
{
    /// <summary>
    /// The contract between Lane A (Driving & World) and Lane B (Rules & UI).
    /// Systems raise events here and listen here, so neither lane needs a direct
    /// reference to the other lane's scripts.
    ///
    /// Rule: subscribe in OnEnable, unsubscribe in OnDisable. Otherwise a restarted
    /// scene keeps calling destroyed objects.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>GameManager changed state. Raised by GameManager.</summary>
        public static event Action<GameState> StateChanged;

        /// <summary>The bus crossed a route's start line and the timer starts. Raised by GameManager.</summary>
        public static event Action<Route> RunStarted;

        /// <summary>One passenger boarded. Raised by BusStop.</summary>
        public static event Action PassengerBoarded;

        /// <summary>The bus crossed a stop line on red. Raised by TrafficStopLine.</summary>
        public static event Action RedLightViolation;

        /// <summary>The bus hit an obstacle (after the cooldown). Raised by BusCollisionPenalty.</summary>
        public static event Action CollisionPenalty;

        /// <summary>The run is over, won or failed, with the full breakdown. Raised by GameManager.</summary>
        public static event Action<RunResult> RunEnded;

        public static void RaiseStateChanged(GameState state) => StateChanged?.Invoke(state);
        public static void RaiseRunStarted(Route route) => RunStarted?.Invoke(route);
        public static void RaisePassengerBoarded() => PassengerBoarded?.Invoke();
        public static void RaiseRedLightViolation() => RedLightViolation?.Invoke();
        public static void RaiseCollisionPenalty() => CollisionPenalty?.Invoke();
        public static void RaiseRunEnded(RunResult result) => RunEnded?.Invoke(result);
    }
}
