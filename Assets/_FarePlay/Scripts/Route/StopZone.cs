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
            // TODO: get BusController.Current and the bounds of its collider.
            //   Not overlapping the zone at all -> Empty.
            //   Fully inside = zone.bounds.Contains(busBounds.min) && zone.bounds.Contains(busBounds.max).
            //   Fully inside AND Mathf.Abs(bus.CurrentSpeed) < tuning.stoppedSpeedThreshold -> StoppedInside,
            //   otherwise -> PartlyInside.
            // TODO: tint bayMarking.material.color: white = Empty,
            //   yellow = PartlyInside (show "Pull fully into the bay"), green = StoppedInside.
        }
    }
}
