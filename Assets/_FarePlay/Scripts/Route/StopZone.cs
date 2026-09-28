using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE B. A reusable "is the bus parked properly inside me?" check. BusStop uses it now,
    /// and a FuelStation can reuse it later (stretch).
    /// Put it on a BoxCollider (Is Trigger) laid over a painted bay on the road.
    /// Keep bays lined up with the world axes so the bounds check below is accurate.
    /// Issue: "Bus stops: stop properly in the bay to board passengers"
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class StopZone : MonoBehaviour
    {
        public enum ZoneStatus { Empty, PartlyInside, StoppedInside }

        [SerializeField] GameTuning tuning;
        [Tooltip("The painted rectangle on the road. Tinted to show the player how they're doing.")]
        [SerializeField] Renderer bayMarking;

        public ZoneStatus Status { get; private set; } = ZoneStatus.Empty;
        public bool BusStoppedInside => Status == ZoneStatus.StoppedInside;

        BoxCollider zone;

        void Awake()
        {
            zone = GetComponent<BoxCollider>();
            zone.isTrigger = true;
        }

        void Update()
        {
            BusController bus = BusController.Current;
            if (bus == null)
            {
                Status = ZoneStatus.Empty;
                ApplyBayTint();
                return;
            }

            Bounds busBounds = bus.GetComponent<Collider>().bounds;
            bool overlaps = zone.bounds.Intersects(busBounds);
            bool fullyInside = IsFullyInsideOnPlane(zone.bounds, busBounds);

            if (!overlaps)
                Status = ZoneStatus.Empty;
            else if (fullyInside && Mathf.Abs(bus.CurrentSpeed) < tuning.stoppedSpeedThreshold)
                Status = ZoneStatus.StoppedInside;
            else
                Status = ZoneStatus.PartlyInside;

            ApplyBayTint();
        }

        void ApplyBayTint()
        {
            if (bayMarking == null) return;

            Color color = Status == ZoneStatus.StoppedInside ? Color.green
                        : Status == ZoneStatus.PartlyInside  ? Color.yellow
                        : Color.white;
            bayMarking.material.color = color;
        }

        static bool IsFullyInsideOnPlane(Bounds zoneBounds, Bounds busBounds)
        {
            return busBounds.min.x >= zoneBounds.min.x
                && busBounds.max.x <= zoneBounds.max.x
                && busBounds.min.z >= zoneBounds.min.z
                && busBounds.max.z <= zoneBounds.max.z;
        }
    }
}
