using System.Collections.Generic;
using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE B. A bus stop: while the bus is stopped properly in the bay, passengers board one at a time.
    /// Drive away early and the rest stay waiting.
    /// Issue: "Bus stops: stop properly in the bay to board passengers"
    /// </summary>
    [RequireComponent(typeof(StopZone))]
    public class BusStop : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [Tooltip("Passenger models waiting at the stop (colored capsules are fine).")]
        [SerializeField] List<GameObject> waitingPassengers = new List<GameObject>();

        public int WaitingCount => waitingPassengers.Count;

        StopZone zone;
        float boardTimer;

        void Awake()
        {
            zone = GetComponent<StopZone>();
        }

        void Update()
        {
            // TODO: while zone.BusStoppedInside && GameManager.Instance.State == GameState.Driving
            //       && WaitingCount > 0 (&& capacity not reached, stretch):
            //         boardTimer += Time.deltaTime;
            //         every tuning.secondsPerPassenger: hide one passenger (SetActive(false)),
            //         remove it from the list, GameEvents.RaisePassengerBoarded().
            // TODO: otherwise reset boardTimer to 0.
        }
    }
}
