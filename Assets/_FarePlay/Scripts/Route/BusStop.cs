using System.Collections.Generic;
using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE B. A bus stop: while the bus is stopped properly in the bay, passengers board one at a time.
    /// Drive away early and the rest stay waiting. Optional patience: if the bus hasn't arrived
    /// leaveAfterSeconds after the run starts, the passengers still waiting give up and leave
    /// (they flash red first, and never leave while the bus is parked in the bay).
    /// Issue: "Bus stops: stop properly in the bay to board passengers"
    /// </summary>
    [RequireComponent(typeof(StopZone))]
    public class BusStop : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [Tooltip("Passenger models waiting at the stop (colored capsules are fine).")]
        [SerializeField] List<GameObject> waitingPassengers = new List<GameObject>();

        [Header("Patience")]
        [Tooltip("Seconds after the run starts before waiting passengers leave. 0 = they wait forever.")]
        [SerializeField] float leaveAfterSeconds = 0f;
        [Tooltip("They flash this many seconds before leaving.")]
        [SerializeField] float warningSeconds = 5f;
        [SerializeField] Color warningColor = Color.red;

        public int WaitingCount => waitingPassengers.Count;

        StopZone zone;
        float boardTimer;
        float runStartTime = -1f;
        readonly Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

        void Awake()
        {
            zone = GetComponent<StopZone>();
            foreach (GameObject p in waitingPassengers)
            {
                Renderer r = p != null ? p.GetComponent<Renderer>() : null;
                if (r != null) originalColors[r] = r.material.color;
            }
        }

        void OnEnable()  { GameEvents.RunStarted += HandleRunStarted; }
        void OnDisable() { GameEvents.RunStarted -= HandleRunStarted; }

        void HandleRunStarted(Route route) { runStartTime = Time.time; }

        void Update()
        {
            UpdatePatience();

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

        void UpdatePatience()
        {
            if (leaveAfterSeconds <= 0f || runStartTime < 0f || WaitingCount == 0) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Driving) return;
            bool busHere = zone.BusStoppedInside;   // never walk off while the bus is right there

            float secondsLeft = leaveAfterSeconds - (Time.time - runStartTime);
            if (secondsLeft <= 0f && !busHere)
            {
                foreach (GameObject p in waitingPassengers)
                    if (p != null) p.SetActive(false);
                Debug.Log($"{name}: {WaitingCount} passengers got tired of waiting and left");
                waitingPassengers.Clear();
                return;
            }

            bool flashOn = !busHere && secondsLeft <= warningSeconds && Mathf.Repeat(Time.time, 0.5f) < 0.25f;
            foreach (var pair in originalColors)
                if (pair.Key != null) pair.Key.material.color = flashOn ? warningColor : pair.Value;
        }
    }
}
