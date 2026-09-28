using UnityEngine;
 
namespace FarePlay
{
    /// <summary>
    /// A trigger across a route's entrance. The first start line the bus crosses starts the run
    /// (and the timer) for that route. Put it as a CHILD of the Route object and it finds the route
    /// by itself. Adding this component sizes its Box Collider automatically (12 m wide, 4 m tall).
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class StartLine : MonoBehaviour
    {
        [Tooltip("Leave empty if this object is a child of the Route.")]
        [SerializeField] Route route;
 
        void Reset()
        {
            ConfigureTrigger(GetComponent<BoxCollider>());
        }
 
        void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            if (route == null) route = GetComponentInParent<Route>();
            if (route == null)
                Debug.LogWarning($"[Fare Play] {name}: no Route found. Make this start line a child of a Route object.", this);
        }
 
        void OnTriggerEnter(Collider other)
        {
            if (route == null || GameManager.Instance == null) return;
            if (other.GetComponentInParent<BusController>() == null) return;
            GameManager.Instance.StartRun(route);
        }
 
        void OnDrawGizmos()
        {
            DrawLineGizmo(transform, GetComponent<BoxCollider>(), new Color(0.2f, 1f, 0.3f, 0.35f));
        }
 
        /// <summary>Shared by StartLine and FinishLine: a wide, tall, trigger-only box.</summary>
        internal static void ConfigureTrigger(BoxCollider box)
        {
            box.isTrigger = true;
            box.size = new Vector3(12f, 4f, 1f);
            box.center = new Vector3(0f, 2f, 0f);
        }
 
        /// <summary>Shared by StartLine and FinishLine: a see-through box in the Scene view.</summary>
        internal static void DrawLineGizmo(Transform t, BoxCollider box, Color color)
        {
            if (box == null) return;
            Gizmos.matrix = t.localToWorldMatrix;
            Gizmos.color = color;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(color.r, color.g, color.b, 1f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}