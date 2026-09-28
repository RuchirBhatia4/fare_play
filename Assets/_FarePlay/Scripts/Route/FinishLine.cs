using UnityEngine;
 
namespace FarePlay
{
    /// <summary>
    /// A trigger across the finish. Both routes share it, so it doesn't need to be inside a Route.
    /// Crossing it before a start line does nothing. Adding this component sizes its Box Collider automatically.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class FinishLine : MonoBehaviour
    {
        void Reset()
        {
            StartLine.ConfigureTrigger(GetComponent<BoxCollider>());
        }
 
        void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }
 
        void OnTriggerEnter(Collider other)
        {
            if (GameManager.Instance == null) return;
            if (other.GetComponentInParent<BusController>() == null) return;
            GameManager.Instance.FinishRun();
        }
 
        void OnDrawGizmos()
        {
            StartLine.DrawLineGizmo(transform, GetComponent<BoxCollider>(), new Color(1f, 0.3f, 0.3f, 0.35f));
        }
    }
}