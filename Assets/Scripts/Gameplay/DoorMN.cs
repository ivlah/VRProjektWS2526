using UnityEngine; // Core Unity types (MonoBehaviour, Transform, Debug, etc.)

/// <summary>
/// DoorMN
/// ------
/// Backend component controlling the door logic for Room1 (LivingRoom).
///
/// Responsibilities:
/// - Monitors the Room1CodeManagerMN to determine when the code is complete.
/// - Transitions the door from locked -> unlocked automatically when conditions are met.
/// - Provides a VR-ready public method (TryOpen) that can be called by XR Interaction Toolkit later.
/// - Optionally rotates a pivot transform to simulate door opening (visual placeholder).
///
/// Design principles:
/// - Backend-first: no direct input handling in production logic.
/// - VR-ready: TryOpen() is the only entry point XR systems need to call.
/// - Clear state tracking: isUnlocked and isOpen are explicit and readable.
///
/// Integration (TestMode / Deterministic Boot):
/// - This component no longer "boots" itself in Awake/OnEnable.
/// - It is initialized exactly once via Room1BackendRoot -> Initialize().
/// </summary>
public class DoorMN : MonoBehaviour, IRoom1Initializable
{
    // -------------------------
    // Inspector / References
    // -------------------------

    /// <summary>
    /// Reference to the central Room1CodeManagerMN.
    /// This is used to check whether the required digits have been collected.
    ///
    /// Ideally assigned manually in the Inspector for clarity.
    /// If left null, it will be auto-found during Initialize() (not Awake).
    /// </summary>
    [Header("References")]
    [SerializeField] private Room1CodeManagerMN codeManager;

    /// <summary>
    /// Optional pivot transform used to rotate the door visually.
    /// This should usually point to a child object such as "Door_pivot".
    ///
    /// If null, the door will still function logically but will not rotate.
    /// </summary>
    [SerializeField] private Transform doorPivot; // optional: Drehpunkt

    // -------------------------
    // Runtime State
    // -------------------------

    /// <summary>
    /// Indicates whether the door has been unlocked.
    /// This changes automatically once the required code is complete.
    /// </summary>
    [Header("Door State")]
    [SerializeField] private bool isUnlocked = false;

    /// <summary>
    /// Indicates whether the door has already been opened.
    /// Prevents repeated open logic and duplicate rotations.
    /// </summary>
    [SerializeField] private bool isOpen = false;

    // -------------------------
    // Public Read-Only Accessors
    // -------------------------

    /// <summary>
    /// Exposes whether the door is currently unlocked.
    /// Other systems (e.g., UI or analytics) can query this.
    /// </summary>
    public bool IsUnlocked => isUnlocked;

    /// <summary>
    /// Exposes whether the door is currently open.
    /// Useful for debugging or extended gameplay logic.
    /// </summary>
    public bool IsOpen => isOpen;

    /// <summary>
    /// Stores the initial rotation of the door pivot so we can restore it on reset.
    /// This makes resets deterministic even if the door was rotated open.
    /// </summary>
    private Quaternion initialPivotRotation;

    /// <summary>
    /// Tracks whether we have captured the initial pivot rotation already.
    /// </summary>
    private bool hasCapturedInitialPivotRotation = false;

    /// <summary>
    /// Guard to ensure Initialize() runs only once.
    /// </summary>
    private bool isInitialized = false;

    /// <summary>
    /// Determines deterministic startup order among all IRoom1Initializable components.
    /// Door should initialize after the code manager.
    /// </summary>
    public int InitOrder => 10;

    // -------------------------
    // Unity Lifecycle
    // -------------------------

    /// <summary>
    /// Awake()
    /// ------
    /// Kept intentionally minimal for deterministic boot.
    ///
    /// Responsibilities (allowed here because they are side-effect free):
    /// - Cache the initial pivot rotation so reset can restore it deterministically.
    ///
    /// Not allowed here (moved to Initialize):
    /// - Auto-finding dependencies
    /// - Logging "Ready" / boot messages
    /// - Subscribing to events / starting gameplay logic
    /// </summary>
    private void Awake()
    {
        // Cache the initial pivot rotation once, so reset can restore it deterministically.
        if (doorPivot != null && !hasCapturedInitialPivotRotation)
        {
            initialPivotRotation = doorPivot.rotation;
            hasCapturedInitialPivotRotation = true;
        }
    }

    /// <summary>
    /// Update()
    /// --------
    /// Runs once per frame.
    ///
    /// Purpose:
    /// - Continuously checks whether the required code is complete.
    /// - Automatically unlocks the door once the condition is satisfied.
    ///
    /// No input is handled here.
    /// Unlocking is purely state-driven based on the CodeManager.
    /// </summary>
    private void Update()
    {
        // If not yet unlocked, and a valid code manager exists,
        // and the code is complete -> unlock the door.
        if (!isUnlocked && codeManager != null && codeManager.IsComplete)
            Unlock();
    }

    // -------------------------
    // Deterministic Initialization
    // -------------------------

    /// <summary>
    /// Initialize()
    /// ------------
    /// Deterministic initialization entry point (called once by Room1BackendRoot).
    ///
    /// Responsibilities:
    /// - Ensure dependency reference exists (CodeManager).
    /// - Log readiness status for backend validation.
    /// </summary>
    public void Initialize(Room1BackendRoot root)
    {
        if (isInitialized) return;
        isInitialized = true;

        // If no manager was assigned in the Inspector, try to find one in the scene.
        if (codeManager == null)
            codeManager = FindFirstObjectByType<Room1CodeManagerMN>();

        Debug.Log($"[DoorMN] Ready: {name} CodeManager={(codeManager != null ? "OK" : "MISSING")}");
    }

    // -------------------------
    // Core Gameplay / Backend API
    // -------------------------

    /// <summary>
    /// Unlock()
    /// --------
    /// Transitions the door from locked to unlocked state.
    ///
    /// Called automatically when the code becomes complete.
    /// Can also be called manually for debugging if needed.
    ///
    /// Behavior:
    /// - Sets isUnlocked to true.
    /// - Logs the unlock event including the final code.
    /// - Does NOT open the door automatically.
    /// </summary>
    public void Unlock()
    {
        // Prevent duplicate unlock logic.
        if (isUnlocked) return;

        isUnlocked = true;

        Debug.Log($"[DoorMN] UNLOCKED: {name} (Code={codeManager?.CurrentCode})");
    }

    /// <summary>
    /// TryOpen()
    /// ---------
    /// Public VR-ready entry point for attempting to open the door.
    ///
    /// This method is intended to be called by:
    /// - XR Interaction Toolkit events (Select/Activate)
    /// - Future input adapters
    ///
    /// Behavior:
    /// 1) If the door is locked -> log and exit.
    /// 2) If already open -> log and exit.
    /// 3) Otherwise -> mark as open and optionally rotate pivot.
    /// </summary>
    public void TryOpen()
    {
        // If the door is still locked, opening is not allowed.
        if (!isUnlocked)
        {
            Debug.Log($"[DoorMN] LOCKED: {name} (need complete code)");
            return;
        }

        // Prevent repeated open logic.
        if (isOpen)
        {
            Debug.Log($"[DoorMN] Already open: {name}");
            return;
        }

        isOpen = true;
        Debug.Log($"[DoorMN] OPEN: {name}");

        // Backend placeholder:
        // Rotate the pivot transform if assigned.
        // This simulates a simple door-opening animation.
        if (doorPivot != null)
            doorPivot.Rotate(0f, 90f, 0f);
    }

    /// <summary>
    /// ResetDoor()
    /// -----------
    /// Resets the door backend state to a fresh "locked & closed" condition.
    ///
    /// What this does:
    /// - Sets isUnlocked = false
    /// - Sets isOpen = false
    /// - Restores the doorPivot rotation (if a pivot was assigned and cached)
    ///
    /// Why this is needed:
    /// - Room reset must restore both logic state and the placeholder visual rotation.
    /// - VR-ready: XR will still call TryOpen(); reset simply restores initial state.
    /// </summary>
    public void ResetDoor()
    {
        isUnlocked = false;
        isOpen = false;

        // Restore the pivot rotation if possible.
        if (doorPivot != null && hasCapturedInitialPivotRotation)
        {
            doorPivot.rotation = initialPivotRotation;
        }

        Debug.Log($"[DoorMN] ResetDoor -> {name} (Unlocked={isUnlocked}, Open={isOpen})");
    }
}
