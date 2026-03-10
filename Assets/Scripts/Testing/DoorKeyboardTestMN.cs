using UnityEngine; // Core Unity types (MonoBehaviour, etc.)
using UnityEngine.InputSystem; // New Input System API (Keyboard.current, Key, etc.)

/// <summary>
/// DoorKeyboardTestMN
/// ------------------
/// Deterministic keyboard test adapter for DoorMN.
///
/// Key points (consistent with Room1 deterministic boot):
/// - Does NOT do any auto-start logic in Awake/Start.
/// - Is initialized via Room1BackendRoot (IRoom1Initializable).
/// - Is disabled by default at runtime.
/// - Room1TestMode decides (before Play) whether it becomes active.
/// - Does not change puzzle logic; it only calls DoorMN.TryOpen().
/// </summary>
[RequireComponent(typeof(DoorMN))] // Ensures the GameObject has DoorMN (required backend).
public class DoorKeyboardTestMN : MonoBehaviour, IRoom1Initializable
{
    // -------------------------
    // Key Binding
    // -------------------------

    /// <summary>
    /// Keyboard key used to attempt opening the door.
    /// Default is "O" (Open).
    /// </summary>
    [Header("Key Binding")]
    [SerializeField] private Key openKey = Key.O;

    // -------------------------
    // Local Toggle (optional)
    // -------------------------

    /// <summary>
    /// Optional local toggle for this adapter instance.
    /// Even if TestMode enables keyboard testing globally, this can still block it per object.
    /// </summary>
    [Header("Local Enable (optional)")]
    [SerializeField] private bool allowKeyboardForThisDoor = true;

    // -------------------------
    // Cached References
    // -------------------------

    /// <summary>
    /// Cached reference to the DoorMN backend component on the same GameObject.
    /// </summary>
    private DoorMN door;

    // -------------------------
    // Runtime Control
    // -------------------------

    /// <summary>
    /// Runtime flag controlled by Room1TestMode.
    /// Default is false (inactive).
    /// </summary>
    private bool runtimeEnabled = false;

    /// <summary>
    /// Deterministic order: after core gameplay systems.
    /// </summary>
    public int InitOrder => 610;

    /// <summary>
    /// Deterministic initialization (called by Room1BackendRoot).
    /// No side effects besides caching references.
    /// </summary>
    public void Initialize(Room1BackendRoot root)
    {
        door = GetComponent<DoorMN>();
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
        // Only active if TestMode enabled it AND this adapter allows keyboard locally.
        if (!runtimeEnabled) return;
        if (!allowKeyboardForThisDoor) return;

        if (door == null) return;

        if (Keyboard.current != null && Keyboard.current[openKey].wasPressedThisFrame)
        {
            // Attempt to open; DoorMN will decide if it is allowed (locked/unlocked/open).
            door.TryOpen();
        }
    }
}
