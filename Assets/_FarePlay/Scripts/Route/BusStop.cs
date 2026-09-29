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
            bool canBoard = zone.BusStoppedInside
                && WaitingCount > 0
                && (GameManager.Instance == null || GameManager.Instance.State == GameState.Driving);

            if (!canBoard)
            {
                boardTimer = 0f;
                return;
            }

            boardTimer += Time.deltaTime;
            if (boardTimer < tuning.secondsPerPassenger) return;

            boardTimer = 0f;

            GameObject passenger = waitingPassengers[waitingPassengers.Count - 1];
            waitingPassengers.RemoveAt(waitingPassengers.Count - 1);
            if (passenger != null) passenger.SetActive(false);

            GameEvents.RaisePassengerBoarded();
        }
    }
}
