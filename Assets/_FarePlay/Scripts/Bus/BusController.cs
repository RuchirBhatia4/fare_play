using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE A. Arcade bus driving on a Rigidbody. Start from your Unit 1 PlayerController,
    /// but drive the Rigidbody in FixedUpdate so obstacles block the bus, collisions fire,
    /// and speed can be measured (bus stops need to know when you've stopped).
    /// Issue: "Bus driving: throttle, brake/reverse, steering"
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BusController : MonoBehaviour
    {
        public static BusController Current { get; private set; }

        [Header("Driving")]
        [SerializeField] float maxSpeed = 15f;              // m/s
        [SerializeField] float acceleration = 6f;           // m/s per second
        [SerializeField] float brakeDeceleration = 12f;
        [SerializeField] float coastDeceleration = 3f;      // no key pressed
        [SerializeField] float maxReverseSpeed = 4f;
        [SerializeField] float turnDegreesPerSecond = 60f;

        /// <summary>Signed speed in m/s (positive = forward). StopZone reads this.</summary>
        public float CurrentSpeed { get; private set; }

        /// <summary>True while the player holds the throttle. The fuel system reads this (stretch).</summary>
        public bool IsThrottling { get; private set; }

        /// <summary>Off during the overview and after the run ends.</summary>
        public bool InputEnabled { get; set; }

        /// <summary>Stretch: the fuel system sets this to false when the tank is empty.</summary>
        public bool HasFuel { get; set; } = true;

        Rigidbody rb;

        void Awake()
        {
            Current = this;
            rb = GetComponent<Rigidbody>();
        }

        void OnEnable()  { GameEvents.StateChanged += HandleStateChanged; }
        void OnDisable() { GameEvents.StateChanged -= HandleStateChanged; }

        void HandleStateChanged(GameState state)
        {
            InputEnabled = state == GameState.Ready || state == GameState.Driving;
        }

        void FixedUpdate()
        {
            // TODO: read input like Unit 1: Input.GetAxis("Vertical") and Input.GetAxis("Horizontal").
            //       Treat both as 0 when !InputEnabled. Ignore throttle when !HasFuel.
            // TODO: move CurrentSpeed toward its target with Mathf.MoveTowards:
            //       throttle -> maxSpeed, brake while moving forward -> 0 (brakeDeceleration),
            //       brake while stopped -> -maxReverseSpeed, no key -> 0 (coastDeceleration).
            // TODO: steer only while moving, scaled by speed, so the bus can't spin in place:
            //       float turn = steer * turnDegreesPerSecond * (CurrentSpeed / maxSpeed) * Time.fixedDeltaTime;
            //       rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
            // TODO: apply speed but keep gravity (Unity 6 name shown; Unity 2022 uses rb.velocity):
            //       Vector3 v = transform.forward * CurrentSpeed; v.y = rb.linearVelocity.y; rb.linearVelocity = v;
            // Tip: on the Rigidbody, freeze rotation X and Z so the bus can't tip over.
        }

        /// <summary>BusCollisionPenalty calls this so a crash actually slows the bus down.</summary>
        public void OnHitObstacle()
        {
            // TODO: CurrentSpeed *= 0.3f; (tune it)
        }
    }
}
