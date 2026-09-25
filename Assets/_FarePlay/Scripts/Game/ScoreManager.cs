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
                // TODO: passengers x pointsPerPassenger - violations x redLightPenalty - collisions x collisionPenalty
                return 0;
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

        void HandlePassengerBoarded() { /* TODO: PassengersOnBoard++ */ }
        void HandleRedLight()         { /* TODO: RedLightViolations++ */ }
        void HandleCollision()        { /* TODO: Collisions++ */ }

        /// <summary>Called by GameManager when the run ends.</summary>
        public RunResult BuildResult(bool won, float secondsRemaining, string reason)
        {
            // TODO: fill in every field of RunResult.
            //  - Passengers only count as delivered when the bus reaches the finish (won == true).
            //  - TimeBonus = Mathf.FloorToInt(secondsRemaining) x timeBonusPerSecond, only when won.
            //  - Total = passenger points + time bonus - penalties, clamped at 0. When failed, Total = 0.
            //  - RouteName from GameManager.Instance.ActiveRoute.DisplayName.
            return new RunResult { Won = won, Reason = reason, SecondsRemaining = secondsRemaining };
        }
    }
}
