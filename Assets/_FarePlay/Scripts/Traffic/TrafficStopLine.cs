using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE A. A trigger across the road at a light's stop line. Crossing it on red costs points,
    /// once per light. Yellow is fine. Running the red is allowed; it just costs points.
    /// Issue: "Traffic lights: cycle, stop line and red-light violations"
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TrafficStopLine : MonoBehaviour
    {
        [SerializeField] TrafficLight trafficLight;
        [SerializeField] Route route;

        bool penalized;

        void OnTriggerEnter(Collider other)
        {
            // TODO: only react to the bus, and only while GameManager.Instance.State == GameState.Driving.
            // TODO: if trafficLight.State == SignalState.Red && !penalized:
            //           penalized = true; GameEvents.RaiseRedLightViolation();
            // TODO: route.MarkLightPassed(trafficLight) so the HUD moves on to the next signal.
        }
    }
}
