using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// Every balancing number in one asset. Create it once:
    /// Project window > right-click > Create > Fare Play > Game Tuning,
    /// save it as Assets/_FarePlay/Settings/GameTuning.asset, and drag it into every
    /// script that has a "Tuning" slot. During the balance pass you only edit this asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Fare Play/Game Tuning", fileName = "GameTuning")]
    public class GameTuning : ScriptableObject
    {
        [Header("Score")]
        public int pointsPerPassenger = 100;
        [Tooltip("Added at the finish: exact seconds left x this, with decimals (9.876 s x 10 = +98.76).")]
        public int timeBonusPerSecond = 10;
        public int redLightPenalty = 50;
        public int collisionPenalty = 50;
        [Tooltip("A long scrape along a wall should count once, not every frame.")]
        public float collisionCooldownSeconds = 1f;

        [Header("Flow")]
        [Tooltip("How long the top-down overview shows before switching to the chase camera.")]
        public float overviewSeconds = 30f;

        [Header("Bus stops")]
        [Tooltip("The bus counts as stopped below this speed (m/s).")]
        public float stoppedSpeedThreshold = 0.5f;
        [Tooltip("One passenger boards every this many seconds.")]
        public float secondsPerPassenger = 1.5f;
        [Tooltip("0 = unlimited. (Stretch goal: capacity limit.)")]
        public int busCapacity = 0;

        [Header("Weight: each passenger aboard makes the bus heavier")]
        [Tooltip("Acceleration lost per passenger (0.07 = -7%).")]
        public float weightAccelerationPerPassenger = 0.07f;
        public float weightBrakingPerPassenger = 0.07f;
        public float weightSteeringPerPassenger = 0.05f;
        public float weightTopSpeedPerPassenger = 0.03f;
        [Tooltip("Momentum: a heavier bus slows down less when you let go of the keys (0.07 = coasts 7% further per passenger).")]
        public float weightCoastingPerPassenger = 0.07f;
        [Tooltip("Extra fuel burn per passenger (0.05 = +5%).")]
        public float weightFuelPerPassenger = 0.05f;

        [Header("Boost: brakes are your engine")]
        [Tooltip("Most boost the bus can store, in seconds of boosting.")]
        public float boostMaxSeconds = 6f;
        public float perfectStopBoostSeconds = 3f;
        public float goodStopBoostSeconds = 1.8f;
        public float sloppyStopBoostSeconds = 0.75f;
        [Tooltip("Each extra PERFECT in a row adds this much: x2 = 1.5x, x3 = 2x.")]
        public float comboBonusPerStep = 0.5f;
        [Tooltip("PERFECT: bus centre within this many metres of the bay's centre line...")]
        public float perfectMaxOffset = 0.35f;
        [Tooltip("...and within this many degrees of straight.")]
        public float perfectMaxAngle = 5f;
        public float goodMaxOffset = 0.6f;
        public float goodMaxAngle = 10f;
        [Tooltip("Regenerative braking: boost seconds gained per m/s of speed you brake away (empty bus). A full stop from 12 m/s = 0.72 s.")]
        public float regenBoostPerSpeedLost = 0.06f;
        [Tooltip("Heavier bus = more energy recovered: +10% regen per passenger.")]
        public float regenBonusPerPassenger = 0.1f;
        public float boostTopSpeedMultiplier = 1.5f;
        public float boostAccelerationMultiplier = 2f;
        [Tooltip("Boosting burns fuel this many times faster.")]
        public float boostFuelMultiplier = 3f;

        [Header("Fuel (stretch goal)")]
        public float fuelTankSize = 100f;
        public float fuelPerSecondAtFullThrottle = 4f;
        public float refuelPerSecond = 25f;
    }
}
