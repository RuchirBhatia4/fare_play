using UnityEngine;
namespace FarePlay
{
    /// <summary>
    /// LANE A. Put on the Main Camera. One camera, two poses:
    ///   - Overview: sits at overviewPose (a top-down view of the routes) during GameState.Overview.
    ///   - Chase: behind and above the bus, looking a little ahead of it.
    /// When the game leaves Overview, the camera swoops from one to the other over blendSeconds.
    ///
    /// In a sandbox scene with no GameManager (or with no overviewPose set), it starts in chase mode.
    /// </summary>
    public class CameraDirector : MonoBehaviour
    {
        [Tooltip("Empty GameObject placed high above the map, rotated to look down at the routes. Optional in sandboxes.")]
        [SerializeField] Transform overviewPose;
        [Tooltip("Leave empty to find the bus automatically.")]
        [SerializeField] Transform bus;

        [Header("Chase view")]
        [Tooltip("Camera position relative to the bus: x = right, y = up, z = forward (negative = behind).")]
        [SerializeField] Vector3 chaseOffset = new Vector3(0f, 5f, -12f);
        [Tooltip("How far in front of the bus the camera looks.")]
        [SerializeField] float lookAhead = 8f;
        [SerializeField] float lookHeight = 1.5f;
        [Tooltip("Higher = camera sticks tighter to the bus.")]
        [SerializeField] float positionSharpness = 6f;
        [SerializeField] float rotationSharpness = 8f;

        [Header("Top view (toggle while driving)")]
        [Tooltip("Press this to switch between the chase view and a steep top view, handy for lining up with bus stops.")]
        [SerializeField] KeyCode switchViewKey = KeyCode.C;
        [Tooltip("Camera position relative to the bus in top view: high up and a little behind (about 70 degrees down).")]
        [SerializeField] Vector3 topViewOffset = new Vector3(0f, 20f, -6f);
        [SerializeField] float topViewLookAhead = 2f;

        [Header("Overview -> chase switch")]
        [SerializeField] float blendSeconds = 1.5f;

        bool chasing;
        bool topView;
        float blendStartTime = -999f;
        Vector3 blendFromPosition;
        Quaternion blendFromRotation;

        void OnEnable()  { GameEvents.StateChanged += HandleStateChanged; }
        void OnDisable() { GameEvents.StateChanged -= HandleStateChanged; }

        void Start()
        {
            if (bus == null && BusController.Current != null) bus = BusController.Current.transform;

            if (GameManager.Instance == null || overviewPose == null)
            {
                chasing = true;       // sandbox: straight to the chase view
                SnapToChase();
            }
            else if (!chasing)
            {
                transform.SetPositionAndRotation(overviewPose.position, overviewPose.rotation);
            }
        }

        void HandleStateChanged(GameState state)
        {
            bool shouldChase = state != GameState.Overview;
            if (shouldChase && !chasing)
            {
                // Remember where we are so the swoop starts from here.
                blendStartTime = Time.time;
                blendFromPosition = transform.position;
                blendFromRotation = transform.rotation;
            }
            chasing = shouldChase;
        }

        void LateUpdate()
        {
            if (!chasing)
            {
                if (overviewPose != null)
                    transform.SetPositionAndRotation(overviewPose.position, overviewPose.rotation);
                return;
            }
            if (bus == null) return;

            if (Input.GetKeyDown(switchViewKey))
            {
                topView = !topView;
                GameEvents.RaisePopup(topView ? "<size=60%>Top view  (C)</size>" : "<size=60%>Chase view  (C)</size>");
            }

            GetChasePose(out Vector3 targetPosition, out Quaternion targetRotation);

            // During the swoop: ease from the overview pose to the chase pose.
            float blend = blendSeconds > 0f ? (Time.time - blendStartTime) / blendSeconds : 1f;
            if (blend < 1f)
            {
                float t = Mathf.SmoothStep(0f, 1f, blend);
                transform.SetPositionAndRotation(
                    Vector3.Lerp(blendFromPosition, targetPosition, t),
                    Quaternion.Slerp(blendFromRotation, targetRotation, t));
                return;
            }
            // Normal chase: smooth follow. The Exp() form keeps smoothing the same at any frame rate.
            float dt = Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-positionSharpness * dt));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-rotationSharpness * dt));
        }

        void GetChasePose(out Vector3 position, out Quaternion rotation)
        {
            // Follow the bus's heading only, so bumps don't shake the camera.
            Quaternion heading = Quaternion.Euler(0f, bus.eulerAngles.y, 0f);
            position = bus.position + heading * (topView ? topViewOffset : chaseOffset);
            float ahead = topView ? topViewLookAhead : lookAhead;
            Vector3 lookTarget = bus.position + heading * Vector3.forward * ahead + Vector3.up * (topView ? 0f : lookHeight);
            rotation = Quaternion.LookRotation(lookTarget - position, Vector3.up);
        }

        void SnapToChase()
        {
            if (bus == null) return;
            GetChasePose(out Vector3 position, out Quaternion rotation);
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
