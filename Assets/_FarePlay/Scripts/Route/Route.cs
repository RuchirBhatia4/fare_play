using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE A (placed on each route's root object). Describes one route: its name, time limit,
    /// and its traffic lights in the order the bus meets them. The HUD uses NextLight to show
    /// the upcoming signal the whole time.
    /// </summary>
    public class Route : MonoBehaviour
    {
        [SerializeField] string displayName = "Route 1";
        [SerializeField] float timeLimitSeconds = 90f;
        [Tooltip("Traffic lights on this route, in the order the bus reaches them.")]
        [SerializeField] TrafficLight[] lightsInOrder = new TrafficLight[0];

        int nextLightIndex;

        public string DisplayName => displayName;
        public float TimeLimitSeconds => timeLimitSeconds;

        /// <summary>The next light the bus will reach, or null when none are left.</summary>
        public TrafficLight NextLight =>
            nextLightIndex < lightsInOrder.Length ? lightsInOrder[nextLightIndex] : null;

        /// <summary>Called by TrafficStopLine when the bus crosses a light's stop line.</summary>
        public void MarkLightPassed(TrafficLight passed)
        {
            for (int i = nextLightIndex; i < lightsInOrder.Length; i++)
            {
                if (lightsInOrder[i] == passed)
                {
                    nextLightIndex = i + 1;
                    return;
                }
            }
        }
    }
}
