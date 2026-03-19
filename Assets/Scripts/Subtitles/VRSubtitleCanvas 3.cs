using UnityEngine;

/// <summary>
/// VR HEAD-LOCKED SUBTITLE CANVAS
/// ===============================
/// Keeps the subtitle Canvas fixed in front of the VR camera.
/// Attach this to your World Space Canvas GameObject.
///
/// FULL SETUP GUIDE:
/// ─────────────────
/// 1. In your Scene: Right-click Hierarchy → UI → Canvas
///
/// 2. Canvas Settings:
///    - Render Mode: "World Space"
///    - Event Camera: leave empty (we handle positioning via script)
///    - Width: 800, Height: 150 (in the RectTransform)
///    - Scale: 0.001, 0.001, 0.001 (so 800px = 0.8m in world)
///
/// 3. Add this Script to the Canvas:
///    - "VR Camera" slot: Drag your Main Camera 
///      (inside XR Origin → Camera Offset → Main Camera)
///    - Distance: 2.0 (meters in front of you)
///    - Vertical Offset: -0.35 (below eye level, like TV subs)
///    - Smoothing: 8 (higher = snappier follow, lower = floatier)
///
/// 4. Add a TextMeshProUGUI as child of the Canvas:
///    - Anchor: stretch horizontal, bottom
///    - Alignment: Center, Middle
///    - Font Size: 36
///    - Rich Text: ON
///    - Color: White
///
/// 5. Optional: Add an Image behind the text
///    - Color: Black, Alpha ~0.5
///    - This is your "Background Panel" for the SubtitleSystem
///
/// 6. On any GameObject, add the SubtitleSystem script:
///    - Drag the TMP text into "Subtitle Text"
///    - Drag the background Image into "Background Panel"
///    - Drag your fonts into "Normal Font" and "Highlight Font"
///    - Set your timings per line
///
/// DONE! The canvas will float in front of you in VR.
/// </summary>
public class VRSubtitleCanvas : MonoBehaviour
{
    [Header("═══ VR Camera ═══")]
    [Tooltip("Drag your XR Main Camera here (XR Origin → Camera Offset → Main Camera)")]
    public Transform vrCamera;

    [Header("═══ Positioning ═══")]
    [Tooltip("Distance in meters in front of the camera")]
    [Range(0.5f, 5f)]
    public float distance = 2f;

    [Tooltip("Vertical offset in meters (negative = below eye level)")]
    [Range(-1f, 1f)]
    public float verticalOffset = -0.35f;

    [Header("═══ Follow Behavior ═══")]
    [Tooltip("How quickly the canvas follows your head. Higher = snappier, Lower = smoother/floatier")]
    [Range(1f, 20f)]
    public float smoothing = 8f;

    [Tooltip("Only follow horizontal head rotation (keeps subtitles stable when looking up/down)")]
    public bool lockPitch = true;

    private void LateUpdate()
    {
        if (vrCamera == null) return;

        // Calculate target position in front of camera
        Vector3 forward;

        if (lockPitch)
        {
            // Use only the Y-axis rotation (horizontal look direction)
            forward = Vector3.ProjectOnPlane(vrCamera.forward, Vector3.up).normalized;

            // Fallback if looking straight up/down
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.ProjectOnPlane(vrCamera.up, Vector3.up).normalized;
        }
        else
        {
            forward = vrCamera.forward;
        }

        Vector3 targetPos = vrCamera.position
            + forward * distance
            + Vector3.up * verticalOffset;

        // Smoothly move to target
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * smoothing
        );

        // Always face the camera
        Vector3 lookDir = transform.position - vrCamera.position;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * smoothing
            );
        }
    }

    /// <summary>
    /// Auto-find the XR camera if not assigned.
    /// </summary>
    private void Start()
    {
        if (vrCamera == null)
        {
            Camera main = Camera.main;
            if (main != null)
            {
                vrCamera = main.transform;
                Debug.Log("[VRSubtitleCanvas] Auto-assigned Main Camera as VR Camera.");
            }
            else
            {
                Debug.LogError("[VRSubtitleCanvas] No VR Camera assigned and no Main Camera found!");
            }
        }
    }
}
