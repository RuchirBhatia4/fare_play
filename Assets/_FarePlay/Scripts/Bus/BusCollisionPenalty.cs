using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE A. Put on the bus. Raises a collision penalty when the bus hits anything tagged "Obstacle".
    /// Setup: add the tag "Obstacle" (Tags & Layers) and tag every prop and wall along the routes.
    /// Don't tag the road or the ground.
    /// Issue: "Collision penalty when the bus hits something"
    /// </summary>
    [RequireComponent(typeof(BusController))]
    public class BusCollisionPenalty : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;

        BusController bus;
        float lastPenaltyTime = -999f;

        void Awake()
        {
            bus = GetComponent<BusController>();
        }

        void OnCollisionEnter(Collision collision)
        {
            // TODO: if collision.gameObject.CompareTag("Obstacle")
            //       and GameManager.Instance.State == GameState.Driving
            //       and Time.time - lastPenaltyTime >= tuning.collisionCooldownSeconds:
            //           lastPenaltyTime = Time.time;
            //           bus.OnHitObstacle();
            //           GameEvents.RaiseCollisionPenalty();
        }
    }
}
