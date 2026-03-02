using UnityEngine;

/// <summary>
/// PuppetMN
/// --------
/// Whole-body guidance rotation using a FaceForward reference.
///
/// Problem:
/// - Many FBX models are not aligned so that their "face forward" matches Unity's +Z.
/// - Result: LookRotation points to the target, but the model visually looks sideways/backwards.
///
/// Solution:
/// - You place a child transform (FaceForward) under the rotating pivot.
/// - FaceForward's BLUE axis (local Z) must point out of the face (nose/eyes direction).
/// - The script rotates the pivot so that FaceForward.forward aims at the target.
///
/// Your current setup (based on your hierarchy screenshot):
/// Puppet_01
///   └─ Visual
///        └─ Puppet          <-- rotateTransform (assign this)
///             ├─ FaceForward <-- faceForward (assign this), Rotation Y = -90 (your calibration)
///             ├─ Head ...
///             ├─ PuppeHead ...
///             └─ PuppetBody ...
/// </summary>
public class PuppetMN : MonoBehaviour, IRoom1Initializable
{
    // ---------------------------------------------------------------------
    // Inspector References
    // ---------------------------------------------------------------------

    [Header("Look / Guidance")]
    [SerializeField] private Transform rotateTransform;  // e.g. Puppet (the whole body pivot)
    [SerializeField] private Transform faceForward;      // e.g. FaceForward (child under rotateTransform)
    [SerializeField] private Transform target;           // set by GuidanceManager

    // ---------------------------------------------------------------------
    // Tuning
    // ---------------------------------------------------------------------

    [Header("Tuning")]
    [SerializeField] private float rotateSpeed = 10f;

    [Tooltip("If true, we rotate only around Y (no head tilt up/down). Recommended for room navigation.")]
    [SerializeField] private bool horizontalOnly = true;

    // ---------------------------------------------------------------------
    // Public Read-Only
    // ---------------------------------------------------------------------

    public Transform Target => target;

    private bool isInitialized = false;
    public int InitOrder => 20;

    // ---------------------------------------------------------------------
    // Runtime
    // ---------------------------------------------------------------------

    private void Update()
    {
        if (target == null) return;

        Transform pivot = rotateTransform != null ? rotateTransform : transform;

        // Direction from pivot to target (WORLD space)
        Vector3 desiredDir = target.position - pivot.position;
        if (horizontalOnly) desiredDir.y = 0f;

        if (desiredDir.sqrMagnitude < 0.0001f) return;
        desiredDir.Normalize();

        // If no faceForward assigned, fallback to default LookRotation (uses pivot.forward).
        if (faceForward == null)
        {
            Quaternion desired = Quaternion.LookRotation(desiredDir, Vector3.up);
            pivot.rotation = Quaternion.Slerp(pivot.rotation, desired, rotateSpeed * Time.deltaTime);
            return;
        }

        // Current "face forward" direction in WORLD space
        Vector3 currentFace = faceForward.forward;
        if (horizontalOnly) currentFace.y = 0f;

        if (currentFace.sqrMagnitude < 0.0001f) return;
        currentFace.Normalize();

        // Rotation that turns current face direction into the desired direction (WORLD space)
        Quaternion deltaWorld = Quaternion.FromToRotation(currentFace, desiredDir);

        // Desired pivot rotation in WORLD space
        Quaternion desiredWorldRotation = deltaWorld * pivot.rotation;

        // Smoothly rotate pivot
        pivot.rotation = Quaternion.Slerp(pivot.rotation, desiredWorldRotation, rotateSpeed * Time.deltaTime);
    }

    // ---------------------------------------------------------------------
    // Deterministic Initialization
    // ---------------------------------------------------------------------

    public void Initialize(Room1BackendRoot root)
    {
        if (isInitialized) return;
        isInitialized = true;

        Debug.Log($"[PuppetMN] Ready: {name} " +
                  $"Rotate={(rotateTransform != null ? rotateTransform.name : "SELF")} " +
                  $"FaceForward={(faceForward != null ? faceForward.name : "NONE")} " +
                  $"Target={(target != null ? target.name : "NONE")}");
    }

    // ---------------------------------------------------------------------
    // API
    // ---------------------------------------------------------------------

    public void SetTarget(Transform newTarget)
    {
        if (target == newTarget) return;
        target = newTarget;

        Debug.Log($"[PuppetMN] {name} SetTarget -> {(target != null ? target.name : "NONE")}");
    }

    public void ClearTarget()
    {
        target = null;
        Debug.Log($"[PuppetMN] {name} ClearTarget");
    }
}