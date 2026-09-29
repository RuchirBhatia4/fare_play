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
            if (!collision.gameObject.CompareTag("Obstacle")) return;

            // Sandbox testing: no GameManager means we're always "driving".
            bool driving = GameManager.Instance == null || GameManager.Instance.State == GameState.Driving;
            if (!driving) return;

            if (Time.time - lastPenaltyTime < tuning.collisionCooldownSeconds) return;

            lastPenaltyTime = Time.time;
            bus.OnHitObstacle();
            GameEvents.RaiseCollisionPenalty();
            Debug.Log($"Collision penalty: hit {collision.gameObject.name}");
        }
    }
}
