using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE A. A trigger across the road at a light's stop line. Crossing it on red costs points,
    /// once per light. Yellow is fine. Running the red is allowed; it just costs points.
    /// Put it as a CHILD of the TrafficLight (inside a Route) and it finds both by itself.
    /// Adding this component sizes its Box Collider automatically (14 m wide = one road).
    /// Issue: "Traffic lights: cycle, stop line and red-light violations"
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class TrafficStopLine : MonoBehaviour
    {
        [Tooltip("Leave empty if this object is a child of the TrafficLight.")]
        [SerializeField] TrafficLight trafficLight;
        [Tooltip("Leave empty if the light is inside a Route object (otherwise the active route is used).")]
        [SerializeField] Route route;

        bool penalized;
        bool passed;

        void Reset()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(14f, 4f, 1f);
            box.center = new Vector3(0f, 2f, 0f);
        }

        void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            if (trafficLight == null) trafficLight = GetComponentInParent<TrafficLight>();
            if (route == null) route = GetComponentInParent<Route>();
            if (trafficLight == null)
                Debug.LogWarning($"[Fare Play] {name}: no TrafficLight found. Make this stop line a child of a TrafficLight.", this);
        }

        void OnTriggerEnter(Collider other)
        {
            if (trafficLight == null) return;
            if (other.GetComponentInParent<BusController>() == null) return;

            // Sandbox testing: no GameManager means we're always "driving".
            bool driving = GameManager.Instance == null || GameManager.Instance.State == GameState.Driving;
            if (!driving) return;

            if (trafficLight.State == SignalState.Red && !penalized)
            {
                penalized = true;
                GameEvents.RaiseRedLightViolation();
                Debug.Log($"Red light violation at {trafficLight.name}");
            }

            if (!passed)
            {
                passed = true;
                // Not inside a Route? Fall back to the route being driven.
                Route r = route != null ? route : GameManager.Instance != null ? GameManager.Instance.ActiveRoute : null;
                if (r != null) r.MarkLightPassed(trafficLight);
            }
        }

        void OnDrawGizmos()
        {
            Color color = new Color(1f, 0.85f, 0.2f, 0.35f);
            if (Application.isPlaying && trafficLight != null)
            {
                color = trafficLight.State == SignalState.Red    ? new Color(1f, 0.2f, 0.2f, 0.35f)
                      : trafficLight.State == SignalState.Yellow ? new Color(1f, 0.85f, 0.2f, 0.35f)
                      : new Color(0.2f, 1f, 0.3f, 0.35f);
            }
            StartLine.DrawLineGizmo(transform, GetComponent<BoxCollider>(), color);
        }
    }
}
