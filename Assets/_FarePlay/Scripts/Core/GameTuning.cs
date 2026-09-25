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
        [Tooltip("Added at the finish: seconds left on the clock x this.")]
        public int timeBonusPerSecond = 10;
        public int redLightPenalty = 50;
        public int collisionPenalty = 25;
        [Tooltip("A long scrape along a wall should count once, not every frame.")]
        public float collisionCooldownSeconds = 1f;

        [Header("Flow")]
        [Tooltip("How long the top-down overview shows before switching to the chase camera.")]
        public float overviewSeconds = 30f;

        [Header("Bus stops")]
        [Tooltip("The bus counts as stopped below this speed (m/s).")]
        public float stoppedSpeedThreshold = 0.5f;
        [Tooltip("One passenger boards every this many seconds.")]
        public float secondsPerPassenger = 0.75f;
        [Tooltip("0 = unlimited. (Stretch goal: capacity limit.)")]
        public int busCapacity = 0;

        [Header("Fuel (stretch goal)")]
        public float fuelTankSize = 100f;
        public float fuelPerSecondAtFullThrottle = 4f;
        public float refuelPerSecond = 25f;
    }
}
