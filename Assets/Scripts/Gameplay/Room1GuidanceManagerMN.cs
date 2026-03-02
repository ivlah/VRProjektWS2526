using UnityEngine;

/// <summary>
/// Room1GuidanceManagerMN
/// ----------------------
/// Event-driven guidance controller for Room1.
///
/// WHAT IT DOES
/// - No Update() loop (event-driven).
/// - Reacts only to real state changes:
///   - A correct bonbon was collected.
///   - The code was completed.
/// - Updates puppet targets only when necessary (avoids redundant SetTarget spam).
///
/// SETUP (Inspector)
/// - CodeManager (optional; auto-found if missing)
/// - DoorTarget (required if you want door guidance)
/// - Puppets (one or more PuppetMN)
/// - CorrectBonbons (only the correct bonbons, in intended order)
///
/// DETERMINISTIC BOOT
/// - Does NOT subscribe in Awake/OnEnable.
/// - Is initialized exactly once via Room1BackendRoot -> Initialize().
///
/// NOTE ABOUT YOUR NEW "HEAD-ONLY" PUPPET
/// - GuidanceManager does NOT need to change for head-only rotation.
/// - It only assigns targets. PuppetMN decides whether to rotate head or body.
/// </summary>
public class Room1GuidanceManagerMN : MonoBehaviour, IRoom1Initializable
{
    // ---------------------------------------------------------------------
    // Inspector References
    // ---------------------------------------------------------------------

    [Header("References")]
    [SerializeField] private Room1CodeManagerMN codeManager;
    [SerializeField] private Transform doorTarget;
    [SerializeField] private PuppetMN[] puppets;

    [Header("Guidance Order (Correct Bonbons Only)")]
    [SerializeField] private BonbonMN[] correctBonbons;

    // ---------------------------------------------------------------------
    // Internal State
    // ---------------------------------------------------------------------

    /// <summary>
    /// Index pointer used to find the next uncollected correct bonbon efficiently.
    /// </summary>
    private int nextIndex = 0;

    /// <summary>
    /// Cache of the last applied target so we don't re-apply the same one.
    /// Keeps logs clean and avoids redundant work.
    /// </summary>
    private Transform lastAppliedTarget;

    /// <summary>
    /// Guard to ensure Initialize() runs only once.
    /// Also ensures we only subscribe once and avoid duplicate handlers.
    /// </summary>
    private bool isInitialized = false;

    /// <summary>
    /// Determines deterministic startup order among all IRoom1Initializable components.
    /// Guidance should initialize after:
    /// - CodeManager (0)
    /// - Door (10)
    /// - Puppets (20)
    /// </summary>
    public int InitOrder => 30;

    // ---------------------------------------------------------------------
    // Unity Lifecycle (kept minimal for deterministic boot)
    // ---------------------------------------------------------------------

    private void Awake()
    {
        // Intentionally empty (deterministic boot via Initialize()).
    }

    private void OnEnable()
    {
        // Intentionally empty (deterministic boot via Initialize()).
    }

    private void OnDisable()
    {
        // Only unsubscribe if we were initialized (prevents null/unsubscribe issues).
        if (!isInitialized) return;

        UnsubscribeFromBonbonEvents();
        UnsubscribeFromCodeManagerEvents();
    }

    // ---------------------------------------------------------------------
    // Deterministic Initialization
    // ---------------------------------------------------------------------

    /// <summary>
    /// Initialize()
    /// ------------
    /// Deterministic initialization entry point (called once by Room1BackendRoot).
    /// Responsibilities:
    /// - Ensure CodeManager reference exists
    /// - Subscribe to events
    /// - Apply initial guidance exactly once (after deterministic init order)
    /// - Log readiness
    ///
    /// Extra robustness:
    /// - We also reset internal caches here to guarantee that initial guidance
    ///   is always applied correctly after scene reloads / domain reloads.
    /// </summary>
    public void Initialize(Room1BackendRoot root)
    {
        if (isInitialized) return;
        isInitialized = true;

        // Auto-find CodeManager only if it wasn't assigned in the Inspector.
        if (codeManager == null)
            codeManager = FindFirstObjectByType<Room1CodeManagerMN>();

        SubscribeToBonbonEvents();
        SubscribeToCodeManagerEvents();

        Debug.Log(
            $"[Room1GuidanceManagerMN] Ready. " +
            $"CodeManager={(codeManager != null ? "OK" : "MISSING")}, " +
            $"DoorTarget={(doorTarget != null ? doorTarget.name : "MISSING")}, " +
            $"Puppets={(puppets != null ? puppets.Length : 0)}, " +
            $"CorrectBonbons={(correctBonbons != null ? correctBonbons.Length : 0)}"
        );

        // Ensure a deterministic "clean start" for guidance.
        nextIndex = 0;
        lastAppliedTarget = null;

        // Apply initial guidance now (deterministic point in boot chain).
        ApplyGuidanceNow();
    }

    // ---------------------------------------------------------------------
    // Event Subscription
    // ---------------------------------------------------------------------

    private void SubscribeToBonbonEvents()
    {
        if (correctBonbons == null) return;

        for (int i = 0; i < correctBonbons.Length; i++)
        {
            BonbonMN b = correctBonbons[i];
            if (b == null) continue;

            // Listen for "collected" events.
            b.CollectedEvent += OnAnyCorrectBonbonCollected;
        }
    }

    private void UnsubscribeFromBonbonEvents()
    {
        if (correctBonbons == null) return;

        for (int i = 0; i < correctBonbons.Length; i++)
        {
            BonbonMN b = correctBonbons[i];
            if (b == null) continue;

            b.CollectedEvent -= OnAnyCorrectBonbonCollected;
        }
    }

    private void SubscribeToCodeManagerEvents()
    {
        if (codeManager == null) return;

        // Fired once when code becomes complete.
        codeManager.CodeCompletedEvent += OnCodeCompleted;
    }

    private void UnsubscribeFromCodeManagerEvents()
    {
        if (codeManager == null) return;

        codeManager.CodeCompletedEvent -= OnCodeCompleted;
    }

    // ---------------------------------------------------------------------
    // Event Handlers
    // ---------------------------------------------------------------------

    /// <summary>
    /// Called when any bonbon in correctBonbons[] is collected.
    /// We don't rely on the event index; we recompute the next target and apply it.
    /// </summary>
    private void OnAnyCorrectBonbonCollected(BonbonMN collectedBonbon)
    {
        ApplyGuidanceNow();
    }

    /// <summary>
    /// Called once when the code is completed.
    /// Puppets should then guide to the door.
    /// </summary>
    private void OnCodeCompleted(string finalCode)
    {
        ApplyDoorTarget();
    }

    // ---------------------------------------------------------------------
    // Guidance Logic
    // ---------------------------------------------------------------------

    /// <summary>
    /// Applies guidance based on current state:
    /// - If code complete -> door
    /// - else -> next uncollected correct bonbon
    /// </summary>
    private void ApplyGuidanceNow()
    {
        // Without a code manager, we don't know if we should guide to the door yet.
        if (codeManager == null) return;

        if (codeManager.IsComplete)
        {
            ApplyDoorTarget();
            return;
        }

        Transform next = FindNextCorrectBonbonTarget();
        ApplyTargetToAllPuppets(next);
    }

    private void ApplyDoorTarget()
    {
        // Door target might be missing in early test scenes; handle gracefully.
        ApplyTargetToAllPuppets(doorTarget);
    }

    /// <summary>
    /// Finds the next uncollected correct bonbon transform.
    /// Uses nextIndex as a forward-scan pointer to avoid re-checking earlier bonbons repeatedly.
    /// </summary>
    private Transform FindNextCorrectBonbonTarget()
    {
        if (correctBonbons == null || correctBonbons.Length == 0)
            return null;

        for (int i = nextIndex; i < correctBonbons.Length; i++)
        {
            BonbonMN b = correctBonbons[i];
            if (b == null) continue;

            // Safety: this list should already contain only correct bonbons,
            // but we keep the check to stay robust.
            if (!b.IsCorrect) continue;

            if (!b.Collected)
            {
                nextIndex = i;
                return b.transform;
            }
        }

        // All collected (or missing) -> no bonbon target left.
        nextIndex = correctBonbons.Length;
        return null;
    }

    /// <summary>
    /// Applies a target to all puppets.
    /// Uses lastAppliedTarget to avoid repeating the same assignment.
    /// </summary>
    private void ApplyTargetToAllPuppets(Transform newTarget)
    {
        if (lastAppliedTarget == newTarget) return;
        lastAppliedTarget = newTarget;

        if (puppets == null) return;

        for (int i = 0; i < puppets.Length; i++)
        {
            PuppetMN p = puppets[i];
            if (p == null) continue;

            if (newTarget != null)
                p.SetTarget(newTarget);
            else
                p.ClearTarget();
        }
    }

    // ---------------------------------------------------------------------
    // Public Debug / Control API
    // ---------------------------------------------------------------------

    /// <summary>
    /// ForceRefreshGuidance()
    /// ----------------------
    /// Forces the guidance manager to recompute and apply the correct target now.
    ///
    /// Use cases:
    /// - Room reset should be able to force guidance back to the first correct bonbon.
    /// - Debugging without adding polling (Update()).
    /// </summary>
    public void ForceRefreshGuidance()
    {
        nextIndex = 0;
        lastAppliedTarget = null;

        ApplyGuidanceNow();

        Debug.Log("[Room1GuidanceManagerMN] ForceRefreshGuidance executed.");
    }
}