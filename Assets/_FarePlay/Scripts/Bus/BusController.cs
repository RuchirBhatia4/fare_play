using UnityEngine;
 
namespace FarePlay
{
    /// <summary>
    /// LANE A. Arcade bus driving on a Rigidbody.
    ///   Up    = accelerate (or brake, if you're rolling backwards)
    ///   Down  = brake, then reverse once stopped
    ///   Left/Right = steer (only while moving, reversed when backing up, like a real vehicle)
    ///
    /// We set the Rigidbody's velocity ourselves every physics step instead of using forces:
    /// the bus responds instantly and predictably, but walls and obstacles still block it,
    /// and collisions still fire (BusCollisionPenalty needs that).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BusController : MonoBehaviour
    {
        public static BusController Current { get; private set; }

        [Header("Speed (m/s)")]
        [SerializeField] float maxSpeed = 12f;
        [SerializeField] float acceleration = 5f;
        [SerializeField] float brakeDeceleration = 14f;
        [Tooltip("How fast the bus slows down when no key is pressed.")]
        [SerializeField] float coastDeceleration = 3f;
        [SerializeField] float maxReverseSpeed = 4f;

        [Header("Steering")]
        [SerializeField] float turnDegreesPerSecond = 55f;
        [Tooltip("Below this speed, steering is weaker, so the bus can't spin on the spot.")]
        [SerializeField] float fullSteerSpeed = 5f;

        [Header("Collisions")]
        [Tooltip("If the bus suddenly loses more speed than this in one physics step, it hit something: match the real speed.")]
        [SerializeField] float blockedSpeedLoss = 0.5f;

        /// <summary>Signed speed in m/s (positive = forward). StopZone reads this.</summary>
        public float CurrentSpeed { get; private set; }

        /// <summary>True while the player holds the throttle. The fuel system reads this (stretch).</summary>
        public bool IsThrottling { get; private set; }

        /// <summary>Off during the overview and after the run ends.</summary>
        public bool InputEnabled { get; set; }

        /// <summary>Stretch: the fuel system sets this to false when the tank is empty.</summary>
        public bool HasFuel { get; set; } = true;

        /// <summary>Passengers on the bus. Each one makes it heavier (see GameTuning > Weight).</summary>
        public int PassengersAboard { get; private set; }

        /// <summary>BusBoost sets this while the player is boosting: faster top speed and acceleration.</summary>
        public bool Boosting { get; set; }

        /// <summary>True while the bus is braking hard (regenerative braking charges the boost).</summary>
        public bool IsBraking { get; private set; }

        Rigidbody rb;
        float throttleInput;
        float steerInput;

        void Awake()
        {
            Current = this;
            rb = GetComponent<Rigidbody>();

            // Settings that make a velocity-driven vehicle behave. Set here so nobody forgets them.
            rb.interpolation = RigidbodyInterpolation.Interpolate;                 // smooth camera, no jitter
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;  // no driving through thin walls
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // can't tip over
        }

        void OnEnable()
        {
            GameEvents.StateChanged += HandleStateChanged;
            GameEvents.PassengerBoarded += HandlePassengerBoarded;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= HandleStateChanged;
            GameEvents.PassengerBoarded -= HandlePassengerBoarded;
        }

        void HandlePassengerBoarded() { PassengersAboard++; }

        /// <summary>1 for an empty bus, lower for each passenger (never below 0.2).</summary>
        float Load(float lossPerPassenger) => Mathf.Max(0.2f, 1f - lossPerPassenger * PassengersAboard);

        void Start()
        {
            // Sandbox testing: there's no GameManager in this scene, so let the bus drive anyway.
            if (GameManager.Instance == null) InputEnabled = true;
        }

        void HandleStateChanged(GameState state)
        {
            InputEnabled = state == GameState.Ready || state == GameState.Driving;
        }

        void Update()
        {
            // Read the keyboard every frame; apply it in FixedUpdate.
            throttleInput = InputEnabled ? Input.GetAxisRaw("Vertical") : 0f;
            steerInput    = InputEnabled ? Input.GetAxis("Horizontal")  : 0f;
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            Vector3 forward = rb.rotation * Vector3.forward;

            // Weight and boost scale the driving numbers. No GameManager (sandbox) = no weight.
            GameTuning tuning = GameManager.Instance != null ? GameManager.Instance.Tuning : null;
            float topSpeed = maxSpeed, accel = acceleration, brake = brakeDeceleration, turn = turnDegreesPerSecond, coast = coastDeceleration;
            if (tuning != null)
            {
                topSpeed *= Load(tuning.weightTopSpeedPerPassenger);
                accel    *= Load(tuning.weightAccelerationPerPassenger);
                brake    *= Load(tuning.weightBrakingPerPassenger);
                turn     *= Load(tuning.weightSteeringPerPassenger);
                coast    *= Load(tuning.weightCoastingPerPassenger);   // momentum: heavy buses roll on
                if (Boosting)
                {
                    topSpeed *= tuning.boostTopSpeedMultiplier;
                    accel    *= tuning.boostAccelerationMultiplier;
                }
            }

            // 1. Did we hit something? If the bus lost a lot of speed since last step, a collision
            //    stopped it, so adopt the real speed (otherwise it would keep "pushing" into walls).
            float actualSpeed = Vector3.Dot(rb.linearVelocity, forward);
            if (Mathf.Abs(CurrentSpeed) - Mathf.Abs(actualSpeed) > blockedSpeedLoss)
            {
                CurrentSpeed = actualSpeed;
            }

            // 2. Throttle / brake / reverse. With an empty tank you can still brake.
            float throttle = HasFuel ? throttleInput : Mathf.Min(throttleInput, 0f);
            IsThrottling = throttle > 0.01f;

            float target;
            float rate;
            bool braking = false;
            if (throttle > 0.01f)
            {
                if (CurrentSpeed < -0.1f) { target = 0f; rate = brake; braking = true; }       // rolling back: brake first
                else                      { target = topSpeed * throttle; rate = accel; }
            }
            else if (throttle < -0.01f)
            {
                if (CurrentSpeed > 0.1f)  { target = 0f; rate = brake; braking = true; }       // brake...
                else                      { target = maxReverseSpeed * throttle; rate = accel; } // ...then reverse
            }
            else
            {
                target = 0f; rate = coast;                                        // no key: roll to a stop
            }
            IsBraking = braking && Mathf.Abs(CurrentSpeed) > 1f;
            CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, target, rate * dt);

            // 3. Steering. steerFactor goes 0 -> 1 as you speed up, and turns negative when reversing,
            //    so backing up steers like a real vehicle.
            float steerFactor = Mathf.Clamp(CurrentSpeed / fullSteerSpeed, -1f, 1f);
            float turnRadiansPerSecond = steerInput * turn * steerFactor * Mathf.Deg2Rad;
            rb.angularVelocity = new Vector3(0f, turnRadiansPerSecond, 0f);

            // 4. Move. Keep the vertical velocity so gravity still works.
            Vector3 velocity = forward * CurrentSpeed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;
        }

        /// <summary>BusCollisionPenalty calls this so a crash noticeably slows the bus.</summary>
        public void OnHitObstacle()
        {
            CurrentSpeed *= 0.3f;
        }
    }
}
