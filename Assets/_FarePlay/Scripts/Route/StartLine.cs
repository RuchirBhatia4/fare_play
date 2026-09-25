using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE B writes it, LANE A places it. A trigger across a route's entrance.
    /// The first start line the bus crosses starts the run (and the timer) for that route.
    /// Setup: BoxCollider with Is Trigger ticked, wide enough that the bus can't sneak around it.
    /// Issue: "Start lines, finish line and the 1:30 route timer"
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class StartLine : MonoBehaviour
    {
        [SerializeField] Route route;

        void OnTriggerEnter(Collider other)
        {
            // TODO: if other.GetComponentInParent<BusController>() != null,
            //       call GameManager.Instance.StartRun(route).
        }
    }
}
