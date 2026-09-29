using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE B. Counts passengers and penalties during the run and builds the final breakdown.
    /// Lives on the GameSystems prefab.
    /// Issue: "Scoring: passengers, penalties and time bonus"
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;

        public int PassengersOnBoard { get; private set; }
        public int RedLightViolations { get; private set; }
        public int Collisions { get; private set; }

        /// <summary>Score shown on the HUD during the run (no time bonus yet).</summary>
        public int LiveScore
        {
            get
            {
                if (tuning == null) return 0;
                return PassengersOnBoard * tuning.pointsPerPassenger
                     - RedLightViolations * tuning.redLightPenalty
                     - Collisions * tuning.collisionPenalty;
            }
        }

        void OnEnable()
        {
            GameEvents.PassengerBoarded += HandlePassengerBoarded;
            GameEvents.RedLightViolation += HandleRedLight;
            GameEvents.CollisionPenalty += HandleCollision;
        }

        void OnDisable()
        {
            GameEvents.PassengerBoarded -= HandlePassengerBoarded;
            GameEvents.RedLightViolation -= HandleRedLight;
            GameEvents.CollisionPenalty -= HandleCollision;
        }

        void HandlePassengerBoarded() { PassengersOnBoard++; }
        void HandleRedLight()         { RedLightViolations++; }
        void HandleCollision()        { Collisions++; }

        /// <summary>Called by GameManager when the run ends.</summary>
        public RunResult BuildResult(bool won, float secondsRemaining, string reason)
        {
            int passengersDelivered = won ? PassengersOnBoard : 0;
            int passengerPoints = won && tuning != null
                ? passengersDelivered * tuning.pointsPerPassenger
                : 0;
            int redLightPoints = tuning != null
                ? -RedLightViolations * tuning.redLightPenalty
                : 0;
            int collisionPoints = tuning != null
                ? -Collisions * tuning.collisionPenalty
                : 0;
            int timeBonus = won && tuning != null
                ? Mathf.FloorToInt(secondsRemaining) * tuning.timeBonusPerSecond
                : 0;

            int total = 0;
            if (won)
                total = Mathf.Max(0, passengerPoints + timeBonus + redLightPoints + collisionPoints);

            string routeName = "";
            if (GameManager.Instance != null && GameManager.Instance.ActiveRoute != null)
                routeName = GameManager.Instance.ActiveRoute.DisplayName;

            return new RunResult
            {
                Won = won,
                Reason = reason,
                RouteName = routeName,
                PassengersDelivered = passengersDelivered,
                PassengerPoints = passengerPoints,
                RedLightViolations = RedLightViolations,
                RedLightPoints = redLightPoints,
                Collisions = Collisions,
                CollisionPoints = collisionPoints,
                SecondsRemaining = secondsRemaining,
                TimeBonus = timeBonus,
                Total = total,
            };
        }
    }
}
