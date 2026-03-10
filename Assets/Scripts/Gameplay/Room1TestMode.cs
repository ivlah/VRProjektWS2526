using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors; // ✅ For toggling XR interactors safely (no inspector rewiring)

/// <summary>
/// Room1TestMode is the single controlled entry point for Room1 runtime.
/// It boots the backend root deterministically and then applies a selected test kind.
///
/// IMPORTANT:
/// - Select TestKind BEFORE pressing Play.
/// - This script only toggles test harness visibility / input helpers.
/// - It does NOT change any puzzle logic.
/// </summary>
public class Room1TestMode : MonoBehaviour
{
    /// <summary>
    /// Tag to filter logs in the Console.
    /// </summary>
    private const string LogTag = "[Room1TestMode]";

    /// <summary>
    /// Selectable test mode (choose BEFORE pressing Play).
    /// This controls which developer test harnesses are visible/active.
    /// </summary>
    public enum Room1TestKind
    {
        BackendOnly, // Only backend systems boot. No debug UI, no keyboard, no simulator toggles.
        DebugUI,     // Show debug panel (GUI buttons).
        Keyboard,    // Enable a scene-wide keyboard controller (no per-object adapters required).
        XR           // Hide debug panel; optionally show XR Device Simulator.
    }

    [Header("Test Mode (Select before Play)")]
    [SerializeField] private Room1TestKind testKind = Room1TestKind.DebugUI;

    /// <summary>
    /// Reference to the Room1 backend root (Room1_Backend).
    /// Assign this in the Inspector for maximum reliability.
    /// </summary>
    [SerializeField] private Room1BackendRoot backendRoot;

    /// <summary>
    /// If true, we auto-find the backend root if not assigned.
    /// Useful during fast iteration, but Inspector assignment is preferred.
    /// </summary>
    [SerializeField] private bool autoFindBackendRoot = true;

    // -------------------------
    // Optional test harness references (assign in Inspector)
    // -------------------------

    /// <summary>
    /// Optional debug panel GameObject/script.
    /// We toggle visibility via SetActive (visible/hidden).
    /// </summary>
    [Header("Optional Test Harness (Assign in Inspector)")]
    [SerializeField] private Room1DebugPanelMN debugPanel;

    /// <summary>
    /// Optional XR Device Simulator root GameObject (Hierarchy object).
    /// We toggle visibility via SetActive (visible/hidden).
    /// </summary>
    [SerializeField] private GameObject xrDeviceSimulatorRoot;

    /// <summary>
    /// Optional: If you already placed a Room1KeyboardTestControllerMN in the scene,
    /// assign it here. If left null, Room1TestMode will auto-find or create one at runtime
    /// when TestKind=Keyboard.
    /// </summary>
    [SerializeField] private Room1KeyboardTestControllerMN keyboardController;

    /// <summary>
    /// Unity lifecycle method. Called once on startup.
    /// We use it to kick off the backend initialization chain.
    /// </summary>
    private void Start()
    {
        Debug.Log($"{LogTag} Started ✅ (TestKind={testKind})");

        if (backendRoot == null && autoFindBackendRoot)
        {
            backendRoot = Object.FindFirstObjectByType<Room1BackendRoot>(FindObjectsInactive.Include);
        }

        if (backendRoot == null)
        {
            Debug.LogError($"{LogTag} No Room1BackendRoot found! Please assign it in the Inspector.");
            return;
        }

        // 0) PRE-BOOT: Apply visibility toggles BEFORE backend boot
        // This ensures Room1BackendRoot does not collect/initialize hidden test harness objects.
        ApplyPreBootVisibility();

        // 1) Deterministic backend boot
        backendRoot.Initialize();

        // 2) Post-boot: enable runtime helpers (keyboard controller, etc.) + one summary log line
        ApplyTestKind();
    }

    /// <summary>
    /// Applies visibility-related toggles BEFORE backend initialization.
    /// This prevents backendRoot from collecting/initializing hidden test harness systems.
    /// </summary>
    private void ApplyPreBootVisibility()
    {
        bool showDebugUI = (testKind == Room1TestKind.DebugUI);
        bool showXRSimulator = (testKind == Room1TestKind.XR);

        if (debugPanel != null)
            debugPanel.gameObject.SetActive(showDebugUI);

        if (xrDeviceSimulatorRoot != null)
            xrDeviceSimulatorRoot.SetActive(showXRSimulator);

        // ✅ Robust isolation:
        // XR input must NOT run in BackendOnly/DebugUI/Keyboard, otherwise XR can trigger Collect() in the background.
        // We do NOT touch inspector wiring; we only enable/disable XR interactor components.
        if (testKind == Room1TestKind.XR)
            EnableXRInput();
        else
            DisableXRInput();
    }

    /// <summary>
    /// Applies the selected test kind by toggling visibility/activation of test harness helpers.
    /// This is intentionally "side-effect only" (no gameplay logic changes).
    /// </summary>
    private void ApplyTestKind()
    {
        bool showDebugUI = (testKind == Room1TestKind.DebugUI);
        bool showXRSimulator = (testKind == Room1TestKind.XR);
        bool enableKeyboardController = (testKind == Room1TestKind.Keyboard);

        // -------------------------
        // XR safety (prevents auto-collect / double triggering)
        // -------------------------
        // ✅ Important: This is the core fix for your logged bug.
        // In DebugUI, XR was still active and called BonbonMN.Collect() via XRInteractionManager.Update().
        // Therefore we hard-isolate:
        // - XR mode: XR input ON (Ray/Direct/Poke enabled)
        // - all other modes: XR input OFF
        if (testKind == Room1TestKind.XR)
            EnableXRInput();
        else
            DisableXRInput();

        // -------------------------
        // UI / Scene helpers (visibility)
        // -------------------------
        if (debugPanel != null)
            debugPanel.gameObject.SetActive(showDebugUI);

        if (xrDeviceSimulatorRoot != null)
            xrDeviceSimulatorRoot.SetActive(showXRSimulator);

        // -------------------------
        // Keyboard controller (one scene-wide input adapter)
        // -------------------------
        int keyboardBonbonCount = 0;
        bool keyboardDoorFound = false;

        if (enableKeyboardController)
        {
            EnsureKeyboardControllerExists();

            if (keyboardController != null)
            {
                keyboardController.enabled = true;

                // Scan now so we can log an accurate summary.
                keyboardController.ScanSceneNow();
                keyboardBonbonCount = keyboardController.BonbonCount;
                keyboardDoorFound = keyboardController.HasDoor;
            }
        }
        else
        {
            if (keyboardController != null)
                keyboardController.enabled = false;
        }

        // -------------------------
        // Option B: exactly ONE summary log line
        // -------------------------
        Debug.Log(
            $"{LogTag} Applied TestKind={testKind} | " +
            $"DebugUI={(showDebugUI ? "ON" : "OFF")} | " +
            $"XRSim={(showXRSimulator ? "ON" : "OFF")} | " +
            $"KeyboardController={(enableKeyboardController ? "ON" : "OFF")} " +
            $"(Bonbons={keyboardBonbonCount}, Door={(keyboardDoorFound ? "YES" : "NO")})"
        );
    }

    /// <summary>
    /// Ensures we have exactly one Room1KeyboardTestControllerMN available.
    /// If none is assigned/found, we create a dedicated runtime GameObject under this TestMode object.
    ///
    /// Why this approach:
    /// - No manual attaching to Door/Bonbon objects.
    /// - No need to change existing hierarchy (Livingroom_Root stays as-is).
    /// - Deterministic and reversible: only exists while Play is running.
    /// </summary>
    private void EnsureKeyboardControllerExists()
    {
        // If assigned in Inspector, keep it.
        if (keyboardController != null) return;

        // Otherwise try to find one in the scene (including inactive).
        keyboardController = Object.FindFirstObjectByType<Room1KeyboardTestControllerMN>(FindObjectsInactive.Include);
        if (keyboardController != null) return;

        // Create a runtime-only controller object (no scene hierarchy changes needed).
        GameObject go = new GameObject("Room1_KeyboardTestController (Runtime)");
        go.transform.SetParent(transform, worldPositionStays: false);

        keyboardController = go.AddComponent<Room1KeyboardTestControllerMN>();
    }

    // ============================================================
    // XR INPUT ISOLATION (ROBUST, BUT STILL MINIMAL)
    // ============================================================

    /// <summary>
    /// Disables XR input so it cannot interfere with DebugUI/Keyboard/BackendOnly testing.
    ///
    /// What we disable:
    /// - XRRayInteractor   (ray-based pointing/selecting)
    /// - XRDirectInteractor (hand/direct touching/selecting)
    /// - XRPokeInteractor  (poke/select in UI / near interaction)
    ///
    /// Notes:
    /// - We DO NOT destroy objects.
    /// - We DO NOT change inspector events.
    /// - We DO NOT modify any puzzle logic.
    /// - This is reversible: XR mode re-enables everything.
    /// </summary>
    private void DisableXRInput()
    {
        // Include inactive to avoid "hidden" interactor objects still firing later.
        var rays = Object.FindObjectsByType<XRRayInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < rays.Length; i++)
            if (rays[i] != null) rays[i].enabled = false;

        var directs = Object.FindObjectsByType<XRDirectInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < directs.Length; i++)
            if (directs[i] != null) directs[i].enabled = false;

        var pokes = Object.FindObjectsByType<XRPokeInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < pokes.Length; i++)
            if (pokes[i] != null) pokes[i].enabled = false;
    }

    /// <summary>
    /// Enables XR input (used only in XR mode).
    /// </summary>
    private void EnableXRInput()
    {
        var rays = Object.FindObjectsByType<XRRayInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < rays.Length; i++)
            if (rays[i] != null) rays[i].enabled = true;

        var directs = Object.FindObjectsByType<XRDirectInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < directs.Length; i++)
            if (directs[i] != null) directs[i].enabled = true;

        var pokes = Object.FindObjectsByType<XRPokeInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < pokes.Length; i++)
            if (pokes[i] != null) pokes[i].enabled = true;
    }
}