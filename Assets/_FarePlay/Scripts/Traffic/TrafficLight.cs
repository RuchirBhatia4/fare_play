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
        [SerializeField] float greenSeconds = 5f;
        [SerializeField] float yellowSeconds = 2f;
        [SerializeField] float redSeconds = 10f;
        [Tooltip("Seconds already into the cycle at start. Different offsets keep lights on the same route out of sync.")]
        [SerializeField] float startOffsetSeconds = 0f;

        [Header("Lamps")]
        [SerializeField] Renderer greenLamp;
        [SerializeField] Renderer yellowLamp;
        [SerializeField] Renderer redLamp;
        [Tooltip("How bright an unlit lamp is, compared to a lit one.")]
        [SerializeField, Range(0f, 1f)] float dimBrightness = 0.15f;

        public SignalState State { get; private set; } = SignalState.Green;
        public float SecondsUntilChange { get; private set; }

        /// <summary>Raised whenever the light changes color.</summary>
        public event Action<SignalState> Changed;

        float CycleSeconds => greenSeconds + yellowSeconds + redSeconds;

        void Start()
        {
            // Work out where in the cycle the offset puts us, so any offset is valid.
            float t = CycleSeconds > 0f ? Mathf.Repeat(startOffsetSeconds, CycleSeconds) : 0f;
            if (t < greenSeconds)
            {
                SecondsUntilChange = greenSeconds - t;
                SetState(SignalState.Green);
            }
            else if (t < greenSeconds + yellowSeconds)
            {
                SecondsUntilChange = greenSeconds + yellowSeconds - t;
                SetState(SignalState.Yellow);
            }
            else
            {
                SecondsUntilChange = CycleSeconds - t;
                SetState(SignalState.Red);
            }
        }

        void Update()
        {
            SecondsUntilChange -= Time.deltaTime;
            if (SecondsUntilChange > 0f) return;

            // Carry the leftover time over, so the cycle doesn't drift.
            switch (State)
            {
                case SignalState.Green:
                    SecondsUntilChange += yellowSeconds;
                    SetState(SignalState.Yellow);
                    break;
                case SignalState.Yellow:
                    SecondsUntilChange += redSeconds;
                    SetState(SignalState.Red);
                    break;
                default:
                    SecondsUntilChange += greenSeconds;
                    SetState(SignalState.Green);
                    break;
            }
        }

        void SetState(SignalState newState)
        {
            State = newState;
            SetLamp(greenLamp,  Color.green,                 newState == SignalState.Green);
            SetLamp(yellowLamp, new Color(1f, 0.75f, 0f),    newState == SignalState.Yellow);
            SetLamp(redLamp,    Color.red,                   newState == SignalState.Red);
            Changed?.Invoke(newState);
        }

        void SetLamp(Renderer lamp, Color color, bool lit)
        {
            if (lamp == null) return;
            Color c = lit ? color : color * dimBrightness;
            c.a = 1f;
            lamp.material.color = c;
        }
    }
}
