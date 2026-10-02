using System.Collections.Generic;
using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE A. The twist: brakes are your engine. Put on the bus.
    ///   - Parking in a bay grades the stop (PERFECT / GOOD / SLOPPY) and charges boost.
    ///     PERFECT stops in a row build a combo for bigger charges.
    ///   - Regenerative braking: braking hard also charges boost, more when the bus is heavier.
    ///   - Hold Shift to boost (faster top speed and acceleration) while the meter lasts.
    /// Each stop is graded once per run, so you can't farm boost by re-parking.
    /// </summary>
    [RequireComponent(typeof(BusController))]
    public class BusBoost : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;

        public static BusBoost Current { get; private set; }

        /// <summary>Stored boost, in seconds of boosting.</summary>
        public float Seconds { get; private set; }
        public float Fill01 => tuning != null && tuning.boostMaxSeconds > 0f ? Seconds / tuning.boostMaxSeconds : 0f;
        public bool Boosting => bus != null && bus.Boosting;
        public int Combo { get; private set; }
        public int BestCombo { get; private set; }
        public int PerfectStops { get; private set; }

        BusController bus;
        Collider busCollider;
        StopZone[] stopZones;
        readonly HashSet<StopZone> graded = new HashSet<StopZone>();

        void Awake()
        {
            Current = this;
            bus = GetComponent<BusController>();
            busCollider = GetComponent<Collider>();
        }

        void Start()
        {
            if (tuning == null && GameManager.Instance != null) tuning = GameManager.Instance.Tuning;
            stopZones = FindObjectsByType<StopZone>(FindObjectsSortMode.None);
        }

        void Update()
        {
            if (tuning == null) return;

            // Sandbox testing: no GameManager means we're always "driving".
            bool driving = GameManager.Instance == null || GameManager.Instance.State == GameState.Driving;
            if (!driving)
            {
                bus.Boosting = false;
                return;
            }

            GradeNewStops();

            if (bus.IsBraking)
            {
                float heavier = 1f + tuning.regenBonusPerPassenger * bus.PassengersAboard;
                Seconds += tuning.regenBoostPerBrakeSecond * heavier * Time.deltaTime;
            }

            bool wantsBoost = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bus.Boosting = wantsBoost && Seconds > 0f && bus.HasFuel;
            if (bus.Boosting) Seconds -= Time.deltaTime;

            Seconds = Mathf.Clamp(Seconds, 0f, tuning.boostMaxSeconds);
        }

        void GradeNewStops()
        {
            foreach (StopZone zone in stopZones)
            {
                if (zone == null || graded.Contains(zone) || !zone.BusStoppedInside) continue;
                graded.Add(zone);
                Grade(zone);
            }
        }

        void Grade(StopZone zone)
        {
            // How far the bus is from the bay's centre line, and how crooked it is.
            Transform bay = zone.transform;
            Vector3 offset = busCollider.bounds.center - zone.GetComponent<Collider>().bounds.center;
            float sideways = Mathf.Abs(Vector3.Dot(offset, bay.right));
            float angle = Vector3.Angle(Flat(bus.transform.forward), Flat(bay.forward));
            angle = Mathf.Min(angle, 180f - angle);   // facing either way along the bay is fine

            float seconds;
            string label;
            if (sideways <= tuning.perfectMaxOffset && angle <= tuning.perfectMaxAngle)
            {
                Combo++;
                PerfectStops++;
                BestCombo = Mathf.Max(BestCombo, Combo);
                seconds = tuning.perfectStopBoostSeconds * (1f + tuning.comboBonusPerStep * (Combo - 1));
                label = Combo > 1 ? $"<color=#7CFC9A>PERFECT STOP! x{Combo}</color>" : "<color=#7CFC9A>PERFECT STOP!</color>";
            }
            else if (sideways <= tuning.goodMaxOffset && angle <= tuning.goodMaxAngle)
            {
                Combo = 0;
                seconds = tuning.goodStopBoostSeconds;
                label = "<color=#FFE066>GOOD STOP</color>";
            }
            else
            {
                Combo = 0;
                seconds = tuning.sloppyStopBoostSeconds;
                label = "<color=#FF9F6B>SLOPPY STOP</color>";
            }

            Seconds = Mathf.Min(tuning.boostMaxSeconds, Seconds + seconds);
            GameEvents.RaisePopup($"{label}\n<size=55%>+{seconds:0.0} s boost  (hold Shift)</size>");
            Debug.Log($"{zone.name}: {label} sideways {sideways:0.00} m, angle {angle:0.0} deg, +{seconds:0.0} s boost");
        }

        static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v;
        }
    }
}
