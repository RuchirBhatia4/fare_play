using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE A. Put on the bus. Holding the throttle burns fuel; with an empty tank the bus can only
    /// coast, brake and reverse. Stopping properly in a bus stop bay refuels, so skipping stops is risky.
    /// Running dry and rolling to a stop outside a bay ends the run: "Game over: out of fuel".
    /// The HUD's fuel bar reads Fuel01.
    /// Issue: "Fuel: the throttle drains a fuel bar"
    /// </summary>
    [RequireComponent(typeof(BusController))]
    public class BusFuel : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [Tooltip("With an empty tank, the run ends once the bus has been stopped (outside a bay) this long.")]
        [SerializeField] float outOfFuelGraceSeconds = 1f;

        public static BusFuel Current { get; private set; }

        public float Fuel { get; private set; }
        public float Fuel01 => tuning != null && tuning.fuelTankSize > 0f ? Fuel / tuning.fuelTankSize : 1f;

        BusController bus;
        StopZone[] stopZones;
        float stoppedEmptyTimer;

        void Awake()
        {
            Current = this;
            bus = GetComponent<BusController>();
        }

        void Start()
        {
            Fuel = tuning != null ? tuning.fuelTankSize : 0f;
            stopZones = FindObjectsByType<StopZone>(FindObjectsSortMode.None);
        }

        void Update()
        {
            if (tuning == null) return;

            // Sandbox testing: no GameManager means we're always "driving".
            bool driving = GameManager.Instance == null || GameManager.Instance.State == GameState.Driving;
            if (!driving) return;

            if (bus.IsThrottling)
                Fuel -= tuning.fuelPerSecondAtFullThrottle * Time.deltaTime;

            if (IsParkedInABay())
                Fuel += tuning.refuelPerSecond * Time.deltaTime;

            Fuel = Mathf.Clamp(Fuel, 0f, tuning.fuelTankSize);
            bus.HasFuel = Fuel > 0f;

            CheckOutOfFuel();
        }

        void CheckOutOfFuel()
        {
            bool stranded = !bus.HasFuel
                && Mathf.Abs(bus.CurrentSpeed) < tuning.stoppedSpeedThreshold
                && !IsParkedInABay();

            stoppedEmptyTimer = stranded ? stoppedEmptyTimer + Time.deltaTime : 0f;
            if (stoppedEmptyTimer < outOfFuelGraceSeconds) return;

            stoppedEmptyTimer = 0f;
            if (GameManager.Instance != null) GameManager.Instance.FailRun("Game over: out of fuel");
            else Debug.Log("Out of fuel (sandbox: no GameManager, so the run doesn't end)");
        }

        bool IsParkedInABay()
        {
            foreach (StopZone zone in stopZones)
                if (zone != null && zone.BusStoppedInside) return true;
            return false;
        }
    }
}
