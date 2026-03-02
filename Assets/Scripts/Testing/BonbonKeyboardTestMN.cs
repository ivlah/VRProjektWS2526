using UnityEngine; // MonoBehaviour, Debug, etc.
using UnityEngine.InputSystem; // Keyboard input (New Input System)

/// <summary>
/// BonbonKeyboardTestMN
/// --------------------
/// Deterministic keyboard test adapter for BonbonMN.
///
/// Key points (consistent with Room1 deterministic boot):
/// - Does NOT do any auto-start logic in Awake/Start.
/// - Is initialized via Room1BackendRoot (IRoom1Initializable).
/// - Is disabled by default at runtime.
/// - Room1TestMode decides (before Play) whether it becomes active.
/// - Does not change puzzle logic; it only calls BonbonMN.Collect().
/// </summary>
[RequireComponent(typeof(BonbonMN))]
public class BonbonKeyboardTestMN : MonoBehaviour, IRoom1Initializable
{
    /// <summary>
    /// The key that triggers Collect() for this bonbon.
    /// You can assign different keys per bonbon if you want.
    /// </summary>
    [Header("Key Binding")]
    [SerializeField] private Key testKey = Key.B;

    /// <summary>
    /// Optional local toggle for this specific bonbon adapter.
    /// Even if TestMode enables keyboard testing globally, this can still block it per object.
    /// </summary>
    [Header("Local Enable (optional)")]
    [SerializeField] private bool allowKeyboardForThisBonbon = true;

    /// <summary>
    /// Cached reference to the backend component.
    /// </summary>
    private BonbonMN bonbon;

    /// <summary>
    /// Runtime flag controlled by Room1TestMode.
    /// Default is false (inactive).
    /// </summary>
    private bool runtimeEnabled = false;

    /// <summary>
    /// Deterministic order: after core gameplay systems.
    /// (Not critical, but keeps logs clean if you ever add logs here.)
    /// </summary>
    public int InitOrder => 600;

    /// <summary>
    /// Deterministic initialization (called by Room1BackendRoot).
    /// No side effects besides caching references.
    /// </summary>
    public void Initialize(Room1BackendRoot root)
    {
        bonbon = GetComponent<BonbonMN>();
        runtimeEnabled = false; // Always start inactive; TestMode decides.
    }

    /// <summary>
    /// Called by Room1TestMode to enable/disable this adapter for the chosen test kind.
    /// </summary>
    public void SetRuntimeEnabled(bool enabled)
    {
        runtimeEnabled = enabled;
    }

    private void Update()
    {
        // Only active if TestMode enabled it AND this bonbon allows keyboard.
        if (!runtimeEnabled) return;
        if (!allowKeyboardForThisBonbon) return;

        if (bonbon == null) return;
        if (bonbon.Collected) return;

        if (Keyboard.current != null && Keyboard.current[testKey].wasPressedThisFrame)
        {
            bonbon.Collect();
        }
    }
}
