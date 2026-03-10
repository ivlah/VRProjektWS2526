using UnityEngine; // Core Unity types (MonoBehaviour, Debug, GUI, Rect, etc.)

/// <summary>
/// Room1DebugPanelMN
/// -----------------
/// OPTIONAL debug UI panel (buttons + status) for backend testing without VR knowledge.
///
/// Why this is useful for you:
/// - You can test your backend logic deterministically:
///   - Collect bonbons via buttons (instead of grabbing in VR).
///   - Reset the code manager.
///   - Try to open the door.
/// - You see immediate "OK / not OK" status feedback on screen,
///   without needing any VR simulator workflow.
///
/// VR-ready principle:
/// - This script does not contain gameplay logic.
/// - It only calls backend methods that also work in VR:
///   - BonbonMN.Collect()
///   - Room1CodeManagerMN.ResetCode()
///   - DoorMN.TryOpen()
///
/// IMPORTANT:
/// - This script intentionally does NOT reference PuppetMN yet,
///   so the project compiles even if PuppetMN is not created.
/// - After we implement PuppetMN, we can extend this panel with puppet targeting buttons.
///
/// Integration (TestMode / Deterministic Boot):
/// - This component no longer starts itself in Awake/OnEnable.
/// - It is initialized exactly once via Room1BackendRoot -> Initialize().
/// </summary>
public class Room1DebugPanelMN : MonoBehaviour, IRoom1Initializable
{
    [Header("Debug - Puppet")]
    [SerializeField] private PuppetMN puppet;

    // -------------------------
    // Inspector / Toggle
    // -------------------------

    /// <summary>
    /// Enables/disables the debug UI.
    /// If disabled, OnGUI() will not draw anything.
    /// </summary>
    [Header("Enable/Disable")]
    [SerializeField] private bool enableDebugUI = true;

    // -------------------------
    // References (assign or auto-find)
    // -------------------------

    /// <summary>
    /// Reference to the central code manager.
    /// - Used for status display (digits collected, current code).
    /// - Used for calling ResetCode() from a button.
    ///
    /// You can assign this in the Inspector.
    /// If left null, Initialize() will attempt to find it automatically.
    /// </summary>
    [Header("References (assign in Inspector)")]
    [SerializeField] private Room1CodeManagerMN codeManager;

    /// <summary>
    /// Reference to the door backend component.
    /// - Used for status display (unlocked/open).
    /// - Used for calling TryOpen() from a button.
    ///
    /// You can assign this in the Inspector.
    /// If left null, Initialize() will attempt to find it automatically.
    /// </summary>
    [SerializeField] private DoorMN door;

    /// <summary>
    /// List of bonbons in the scene to show one "Collect" button per bonbon.
    ///
    /// You should assign Bonbon_01..Bonbon_0N in the Inspector (preferred).
    /// This avoids relying on scene searches and keeps testing explicit.
    /// </summary>
    [SerializeField] private BonbonMN[] bonbons;

    // -------------------------
    // Simple UI Layout Settings
    // -------------------------

    /// <summary>
    /// X position (pixels) of the debug panel.
    /// </summary>
    [Header("UI Layout")]
    [SerializeField] private int x = 20;

    /// <summary>
    /// Y position (pixels) of the debug panel.
    /// </summary>
    [SerializeField] private int y = 20;

    /// <summary>
    /// Width (pixels) of the panel elements.
    /// </summary>
    [SerializeField] private int width = 320;

    /// <summary>
    /// Height (pixels) of each button/label row.
    /// </summary>
    [SerializeField] private int rowHeight = 28;

    /// <summary>
    /// Vertical spacing between rows.
    /// </summary>
    [SerializeField] private int gap = 6;

    [Header("Room Reset")]
    [SerializeField] private Room1ResetManagerMN resetManager;

    /// <summary>
    /// Guard to ensure Initialize() runs only once.
    /// </summary>
    private bool isInitialized = false;

    /// <summary>
    /// Determines deterministic startup order among all IRoom1Initializable components.
    /// Debug UI should initialize late, after all backend systems are ready.
    /// </summary>
    public int InitOrder => 900;

    // -------------------------
    // Unity Lifecycle
    // -------------------------

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
    /// This method:
    /// - Attempts to auto-find references if you did not assign them in the Inspector.
    /// - Logs readiness so you can verify the debug panel is active.
    ///
    /// Note:
    /// - Auto-find is convenient for you as "backend person".
    /// - Inspector assignment is preferred in a team project for clarity.
    /// </summary>
    public void Initialize(Room1BackendRoot root)
    {
        if (isInitialized) return;
        isInitialized = true;

        if (codeManager == null)
            codeManager = FindFirstObjectByType<Room1CodeManagerMN>();

        if (door == null)
            door = FindFirstObjectByType<DoorMN>();

        Debug.Log($"[Room1DebugPanelMN] Ready. DebugUI={(enableDebugUI ? "ENABLED" : "DISABLED")}. " +
                  $"CodeManager={(codeManager != null ? "OK" : "MISSING")} Door={(door != null ? "OK" : "MISSING")}");
    }

    // -------------------------
    // Debug UI Rendering
    // -------------------------

    /// <summary>
    /// OnGUI()
    /// -------
    /// Unity immediate-mode GUI callback.
    ///
    /// We use OnGUI because it is:
    /// - fast to implement,
    /// - requires no Canvas/UI setup,
    /// - perfect for backend testing.
    ///
    /// This is not meant as final user UI.
    /// It is a developer testing tool.
    /// </summary>
    private void OnGUI()
    {
        if (!enableDebugUI) return;

        int curY = y;

        // -------------------------
        // STATUS SECTION
        // -------------------------

        // Code manager status
        GUI.Label(new Rect(x, curY, width, rowHeight),
            $"CodeManager: {(codeManager != null ? "OK" : "MISSING")}");
        curY += rowHeight;

        // Code manager details (only if available)
        if (codeManager != null)
        {
            GUI.Label(new Rect(x, curY, width, rowHeight),
                $"Digits: {codeManager.CollectedCount}/{codeManager.RequiredDigits}   Code: {codeManager.CurrentCode}");
            curY += rowHeight;
        }

        // Door status
        GUI.Label(new Rect(x, curY, width, rowHeight),
            $"Door: {(door != null ? "OK" : "MISSING")}   Unlocked={(door != null && door.IsUnlocked)}   Open={(door != null && door.IsOpen)}");
        curY += rowHeight + gap;

        // -------------------------
        // ACTION BUTTONS SECTION
        // -------------------------
        GUI.enabled = (resetManager != null);
        if (GUI.Button(new Rect(x, curY, width, rowHeight), "RESET ROOM1 (Full)"))
        {
            resetManager.ResetRoom();
        }
        GUI.enabled = true;
        curY += rowHeight + gap;

        // Reset CodeManager button
        GUI.enabled = (codeManager != null);
        if (GUI.Button(new Rect(x, curY, width, rowHeight), "Reset CodeManager"))
        {
            codeManager.ResetCode();
        }
        GUI.enabled = true;
        curY += rowHeight + gap;

        // -------------------------
        // PUPPET STATUS SECTION
        // -------------------------

        // Display the current puppet target so backend testing is visible without logs.
        if (puppet != null)
        {
            string targetName = puppet.Target != null ? puppet.Target.name : "NONE";

            GUI.Label(new Rect(x, curY, width, rowHeight),
                $"Puppet Target: {targetName}");

            curY += rowHeight + gap;
        }
        else
        {
            GUI.Label(new Rect(x, curY, width, rowHeight),
                "Puppet Target: (No Puppet Assigned)");

            curY += rowHeight + gap;
        }

        // Try open door button
        GUI.enabled = (door != null);
        if (GUI.Button(new Rect(x, curY, width, rowHeight), "Try Open Door"))
        {
            door.TryOpen();
        }
        GUI.enabled = true;
        curY += rowHeight + gap;

        // -------------------------
        // BONBON BUTTONS SECTION
        // -------------------------

        GUI.Label(new Rect(x, curY, width, rowHeight), "Bonbons:");
        curY += rowHeight;

        if (bonbons != null && bonbons.Length > 0)
        {
            for (int i = 0; i < bonbons.Length; i++)
            {
                BonbonMN b = bonbons[i];

                // Create a readable label for each bonbon button.
                string label = (b != null)
                    ? $"Collect {b.name} (Digit={b.Digit})"
                    : $"Collect Bonbon #{i + 1} (MISSING REF)";

                // Button should only be clickable if bonbon exists and is not yet collected.
                bool canPress = (b != null && !b.Collected);

                GUI.enabled = canPress;
                if (GUI.Button(new Rect(x, curY, width, rowHeight), label))
                {
                    // Call the backend entry point.
                    b.Collect();

                    // -------------------------------------------------
                    // ✅ MINIMAL FIX (wichtig):
                    // OnGUI arbeitet mit Events (Layout/Repaint/MouseUp…).
                    // In seltenen Fällen kann ein Klick dazu führen, dass
                    // mehrere Controls "im selben Event" reagieren.
                    //
                    // Damit pro Klick garantiert nur EIN Bonbon collected wird,
                    // brechen wir nach dem ersten Collect sofort ab.
                    // -------------------------------------------------
                    break;
                }
                GUI.enabled = true;

                curY += rowHeight + gap;
            }
        }
        else
        {
            // If no bonbons are assigned, show a hint.
            GUI.Label(new Rect(x, curY, width, rowHeight), "(Assign BonbonMN objects in the Inspector)");
            curY += rowHeight + gap;
        }
    }
}