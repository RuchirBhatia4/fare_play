using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE A. Moving traffic: drives back and forth between where it starts and start + travel,
    /// turning to face the way it's going. Tag it (and its children) "Obstacle" so hitting it
    /// counts as a collision. Put it on a vehicle model whose front is +Z (the Course Library cars).
    /// Uses a kinematic Rigidbody so the bus collides with it properly.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class TrafficCar : MonoBehaviour
    {
        [Tooltip("Where the other end of the trip is, relative to the start, in world axes (e.g. 0,0,50 = 50 m north).")]
        [SerializeField] Vector3 travel = new Vector3(0f, 0f, 50f);
        [Tooltip("m/s. The bus tops out at 12.")]
        [SerializeField] float speed = 6f;
        [Tooltip("Seconds it waits at each end before turning around.")]
        [SerializeField] float pauseAtEnds = 1f;

        Rigidbody rb;
        Vector3 pointA;
        Vector3 pointB;
        bool headingToB = true;
        float pauseTimer;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            pointA = transform.position;
            pointB = pointA + travel;
            FaceTowards(pointB);
        }

        void FixedUpdate()
        {
            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.fixedDeltaTime;
                return;
            }

            Vector3 target = headingToB ? pointB : pointA;
            Vector3 next = Vector3.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
            rb.MovePosition(next);

            if ((next - target).sqrMagnitude < 0.0001f)
            {
                headingToB = !headingToB;
                pauseTimer = pauseAtEnds;
                FaceTowards(headingToB ? pointB : pointA);
            }
        }

        void FaceTowards(Vector3 target)
        {
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f) rb.MoveRotation(Quaternion.LookRotation(dir));
        }

        void OnDrawGizmosSelected()
        {
            Vector3 a = Application.isPlaying ? pointA : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(a, a + travel);
            Gizmos.DrawWireSphere(a + travel, 1f);
        }
    }
}
