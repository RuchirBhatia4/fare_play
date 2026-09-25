using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE B writes it, LANE A places it. A trigger across the finish. Both routes share it.
    /// Issue: "Start lines, finish line and the 1:30 route timer"
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FinishLine : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            // TODO: if other belongs to the bus, call GameManager.Instance.FinishRun().
        }
    }
}
