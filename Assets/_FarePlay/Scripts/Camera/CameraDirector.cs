using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE A. Put on the Main Camera. One camera, two poses: a top-down overview of the routes
    /// during GameState.Overview, then a chase view behind the bus.
    /// Issue: "Overview camera for 30 s, then switch to the chase view"
    /// </summary>
    public class CameraDirector : MonoBehaviour
    {
        [Tooltip("Empty GameObject placed high above the map, rotated to look down at both routes.")]
        [SerializeField] Transform overviewPose;
        [SerializeField] Transform bus;

        [Header("Chase view")]
        [Tooltip("Behind and above the bus, in the bus's local space.")]
        [SerializeField] Vector3 chaseOffset = new Vector3(0f, 4f, -9f);
        [SerializeField] float lookAhead = 6f;
        [SerializeField] float followSharpness = 5f;

        [Header("Switch")]
        [SerializeField] float blendSeconds = 1.5f;

        bool chasing;
        float blendStartTime;

        void OnEnable()  { GameEvents.StateChanged += HandleStateChanged; }
        void OnDisable() { GameEvents.StateChanged -= HandleStateChanged; }

        void HandleStateChanged(GameState state)
        {
            bool shouldChase = state != GameState.Overview;
            if (shouldChase && !chasing) blendStartTime = Time.time;
            chasing = shouldChase;
        }

        void LateUpdate()
        {
            // TODO (overview): copy overviewPose.position and overviewPose.rotation.
            // TODO (chase): target position = bus.TransformPoint(chaseOffset);
            //       look at bus.position + bus.forward * lookAhead.
            //       Smooth it: transform.position = Vector3.Lerp(transform.position, target,
            //                  1f - Mathf.Exp(-followSharpness * Time.deltaTime));
            // TODO (switch): for the first blendSeconds after blendStartTime, lerp position AND rotation
            //       from the overview pose to the chase pose, so it swoops down instead of cutting.
        }
    }
}
