using UnityEngine; // Unity core types (MonoBehaviour, Debug)

/// <summary>
/// Room1ResetManagerMN
/// -------------------
/// Central reset controller for Room1.
///
/// PURPOSE:
/// - Provides one single backend entry point to reset the whole room puzzle state:
///   - Reset CodeManager
///   - Reset all bonbons (reactivate + clear collected flags)
///   - Reset door (locked + closed + restore pivot rotation)
///   - Force guidance manager to point puppets back to the first correct bonbon
///
/// DESIGN PRINCIPLES:
/// - Backend-first: does not depend on XR or UI.
/// - VR-ready: works with DebugPanel button, simulator tests, or future in-game logic.
/// - Deterministic: always returns to the same known initial state.
///
/// HOW TO USE:
/// - Attach this component to a backend GameObject (e.g., Room1_Backend/ResetManager).
/// - Assign references in Inspector.
/// - Call ResetRoom() from DebugPanel or other test driver.
///
/// Integration (TestMode / Deterministic Boot):
/// - This component no longer starts itself in Awake/OnEnable.
/// - It is initialized exactly once via Room1BackendRoot -> Initialize().
/// </summary>
public class Room1ResetManagerMN : MonoBehaviour, IRoom1Initializable
{
    // ---------------------------------------------------------------------
    // References (assign in Inspector)
    // ---------------------------------------------------------------------

    /// <summary>
    /// Central code manager that stores the collected code digits.
    /// ResetRoom() calls ResetCode() on it.
    /// </summary>
    [Header("References")]
    [SerializeField] private Room1CodeManagerMN codeManager;

    /// <summary>
    /// Door backend controller. ResetRoom() calls ResetDoor() on it.
    /// </summary>
    [SerializeField] private DoorMN door;

    /// <summary>
    /// Guidance manager controlling puppet targets. ResetRoom() calls ForceRefreshGuidance().
    /// </summary>
    [SerializeField] private Room1GuidanceManagerMN guidanceManager;

    /// <summary>
    /// All bonbons in the room, including correct and fake.
    /// ResetRoom() calls ResetBonbon() on each.
    /// </summary>
    [SerializeField] private BonbonMN[] allBonbons;

    /// <summary>
    /// Optional: puppets that should be cleared on reset before guidance is applied.
    /// Not strictly required because guidance manager will set targets,
    /// but clearing first can avoid temporary "old target" display.
    /// </summary>
    [SerializeField] private PuppetMN[] puppets;

    /// <summary>
    /// Guard to ensure Initialize() runs only once.
    /// </summary>
    private bool isInitialized = false;

    /// <summary>
    /// Determines deterministic startup order among all IRoom1Initializable components.
    /// Reset manager depends on other systems (code, door, guidance), so it should initialize later.
    /// </summary>
    public int InitOrder => 200;

    // ---------------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------------

    /// <summary>
    /// Awake()
    /// ------
    /// Kept intentionally minimal for deterministic boot.
    /// No auto-finds or readiness logging here.
    /// </summary>
    private void Awake()
    {
        // Intentionally empty (deterministic boot via Initialize()).
    }

    /// <summary>
    /// Initialize()
    /// ------------
    /// Deterministic initialization entry point (called once by Room1BackendRoot).
    ///
    /// Convenience auto-find for development if references are missing.
    /// Inspector assignment is still recommended for team clarity.
    /// </summary>
    public void Initialize(Room1BackendRoot root)
    {
        if (isInitialized) return;
        isInitialized = true;

        if (codeManager == null) codeManager = FindFirstObjectByType<Room1CodeManagerMN>();
        if (door == null) door = FindFirstObjectByType<DoorMN>();
        if (guidanceManager == null) guidanceManager = FindFirstObjectByType<Room1GuidanceManagerMN>();

        Debug.Log($"[Room1ResetManagerMN] Ready. " +
                  $"CodeManager={(codeManager != null ? "OK" : "MISSING")}, " +
                  $"Door={(door != null ? "OK" : "MISSING")}, " +
                  $"GuidanceManager={(guidanceManager != null ? "OK" : "MISSING")}, " +
                  $"Bonbons={(allBonbons != null ? allBonbons.Length : 0)}, " +
                  $"Puppets={(puppets != null ? puppets.Length : 0)}");
    }

    // ---------------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------------

    /// <summary>
    /// ResetRoom()
    /// ----------
    /// Resets the entire Room1 puzzle state in a deterministic order.
    ///
    /// Reset order (important):
    /// 1) Reset door first (so it is locked/closed before new gameplay begins)
    /// 2) Reset code manager (digits cleared)
    /// 3) Reset bonbons (reactivate + clear collected flags)
    /// 4) Clear puppet targets (optional, avoids stale display)
    /// 5) Force guidance manager to recompute and apply the first target
    ///
    /// This method is safe to call multiple times.
    /// </summary>
    public void ResetRoom()
    {
        Debug.Log("[Room1ResetManagerMN] ResetRoom started.");

        // 1) Reset door state (locked/closed).
        if (door != null)
            door.ResetDoor();

        // 2) Reset code manager digits and completion state.
        if (codeManager != null)
            codeManager.ResetCode();

        // 3) Reset all bonbons (correct + fake).
        if (allBonbons != null)
        {
            for (int i = 0; i < allBonbons.Length; i++)
            {
                BonbonMN b = allBonbons[i];
                if (b == null) continue;

                b.ResetBonbon();
            }
        }

        // 4) Optional: clear puppet targets first (clean state).
        if (puppets != null)
        {
            for (int i = 0; i < puppets.Length; i++)
            {
                PuppetMN p = puppets[i];
                if (p == null) continue;

                p.ClearTarget();
            }
        }

        // 5) Force guidance manager to apply the correct initial target.
        if (guidanceManager != null)
            guidanceManager.ForceRefreshGuidance();

        Debug.Log("[Room1ResetManagerMN] ResetRoom finished.");
    }
}
