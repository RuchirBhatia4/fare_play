using System;
using UnityEngine;

namespace FarePlay
{
    public enum SignalState { Green, Yellow, Red }

    /// <summary>
    /// LANE A. Cycles green -> yellow -> red. The HUD reads State and SecondsUntilChange for the
    /// next light on the active route, so players can plan a bus stop around a red.
    /// Issue: "Traffic lights: cycle, stop line and red-light violations"
    /// </summary>
    public class TrafficLight : MonoBehaviour
    {
        [SerializeField] float greenSeconds = 8f;
        [SerializeField] float yellowSeconds = 2f;
        [SerializeField] float redSeconds = 8f;
        [Tooltip("Different offsets keep lights on the same route out of sync.")]
        [SerializeField] float startOffsetSeconds = 0f;

        [Header("Lamps")]
        [SerializeField] Renderer greenLamp;
        [SerializeField] Renderer yellowLamp;
        [SerializeField] Renderer redLamp;

        public SignalState State { get; private set; } = SignalState.Green;
        public float SecondsUntilChange { get; private set; }

        /// <summary>Raised whenever the light changes color.</summary>
        public event Action<SignalState> Changed;

        void Start()
        {
            // TODO: SecondsUntilChange = greenSeconds - startOffsetSeconds (keep it above 0); SetState(Green).
        }

        void Update()
        {
            // TODO: SecondsUntilChange -= Time.deltaTime. At <= 0, go Green -> Yellow -> Red -> Green,
            //       reset SecondsUntilChange to the new phase's duration, and call SetState.
        }

        void SetState(SignalState newState)
        {
            State = newState;
            // TODO: light up the active lamp and dim the others (swap materials or toggle emission).
            Changed?.Invoke(newState);
        }
    }
}
