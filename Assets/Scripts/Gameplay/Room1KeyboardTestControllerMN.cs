using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Room1KeyboardTestControllerMN
/// ----------------------------
/// ONE single, scene-wide keyboard testing adapter for Room1.
/// This avoids attaching keyboard scripts to every Bonbon/Door manually.
///
/// What it does:
/// - Finds ALL BonbonMN in the currently loaded scene (including inactive).
/// - Finds the DoorMN (first found).
/// - Finds Room1ResetManagerMN (first found) for full reset.
/// - Maps keys to actions and calls existing backend entry points:
///   - BonbonMN.Collect()
///   - DoorMN.TryOpen()
///   - Room1ResetManagerMN.ResetRoom()
///
/// Key mapping (deterministic, beginner-friendly):
/// - Number keys 1..9: Collect bonbon #1..#9 (sorted by GameObject name)
/// - O: Try open door
/// - R: Reset full room (if ResetManager exists)
///
/// Notes:
/// - This is a TEST harness only.
/// - It contains NO puzzle logic; DoorMN/BonbonMN still enforce the rules.
/// - It is safe to enable/disable via Room1TestMode.
/// </summary>
public class Room1KeyboardTestControllerMN : MonoBehaviour
{
    /// <summary>
    /// Tag to filter logs in the Console.
    /// </summary>
    private const string LogTag = "[Room1KeyboardTest]";

    /// <summary>
    /// Cached list of bonbons found in the scene, sorted by GameObject name for determinism.
    /// </summary>
    private readonly List<BonbonMN> bonbons = new List<BonbonMN>();

    /// <summary>
    /// Cached reference to the door backend component (first found).
    /// </summary>
    private DoorMN door;

    /// <summary>
    /// Cached reference to reset manager (first found). Optional.
    /// </summary>
    private Room1ResetManagerMN resetManager;

    /// <summary>
    /// Public read-only count for Room1TestMode summary logging.
    /// </summary>
    public int BonbonCount => bonbons.Count;

    /// <summary>
    /// Public read-only door presence for Room1TestMode summary logging.
    /// </summary>
    public bool HasDoor => door != null;

    /// <summary>
    /// When enabled, we scan once. This keeps setup deterministic and avoids repeated scene searches.
    /// </summary>
    private void OnEnable()
    {
        ScanSceneNow();

        // Keep logs minimal: one informative line is enough for debugging.
        Debug.Log($"{LogTag} Enabled. Bonbons={bonbons.Count}, Door={(door != null ? door.name : "NONE")}, ResetManager={(resetManager != null ? "YES" : "NO")}");
    }

    /// <summary>
    /// Manual scan entry point (called by Room1TestMode to get immediate counts).
    /// Safe to call multiple times.
    ///
    /// Implementation details:
    /// - Uses FindObjectsInactive.Include so it also catches disabled objects (common in tests).
    /// - Sorts bonbons by name to guarantee stable "1..9" mapping across runs.
    /// </summary>
    public void ScanSceneNow()
    {
        bonbons.Clear();

        // Find bonbons (include inactive so test mode works even if objects are disabled)
        BonbonMN[] foundBonbons = Object.FindObjectsByType<BonbonMN>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < foundBonbons.Length; i++)
        {
            BonbonMN b = foundBonbons[i];
            if (b == null) continue;
            bonbons.Add(b);
        }

        // Deterministic order: sort by GameObject name (Bonbon_01, Bonbon_02, Bonbon_Fake_01, ...)
        bonbons.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

        // Find the door and reset manager (first found)
        door = Object.FindFirstObjectByType<DoorMN>(FindObjectsInactive.Include);
        resetManager = Object.FindFirstObjectByType<Room1ResetManagerMN>(FindObjectsInactive.Include);
    }

    /// <summary>
    /// Polls keyboard input (testing only).
    /// This is the ONLY place where keyboard input exists; backend stays VR-clean.
    /// </summary>
    private void Update()
    {
        if (Keyboard.current == null) return;

        // Door open test
        if (Keyboard.current[Key.O].wasPressedThisFrame)
        {
            if (door == null)
            {
                Debug.LogWarning($"{LogTag} O pressed, but no DoorMN found.");
            }
            else
            {
                door.TryOpen();
            }
        }

        // Full reset test
        if (Keyboard.current[Key.R].wasPressedThisFrame)
        {
            if (resetManager == null)
            {
                Debug.LogWarning($"{LogTag} R pressed, but no Room1ResetManagerMN found.");
            }
            else
            {
                resetManager.ResetRoom();
            }
        }

        // Bonbon collect: 1..9
        HandleBonbonNumberKeys();
    }

    /// <summary>
    /// Handles number key presses 1..9 and collects the corresponding bonbon (index-based).
    ///
    /// Index mapping:
    /// - Key '1' triggers bonbons[0]
    /// - Key '2' triggers bonbons[1]
    /// - ...
    /// - Key '9' triggers bonbons[8]
    ///
    /// Safety:
    /// - If index is out of range, we log a warning and do nothing.
    /// - If bonbon is already collected, we do nothing (backend-safe).
    /// </summary>
    private void HandleBonbonNumberKeys()
    {
        Key[] numberKeys =
        {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
            Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
        };

        for (int i = 0; i < numberKeys.Length; i++)
        {
            if (!Keyboard.current[numberKeys[i]].wasPressedThisFrame)
                continue;

            int index = i; // 0-based bonbon index

            if (index >= bonbons.Count)
            {
                Debug.LogWarning($"{LogTag} Pressed {(i + 1)} but bonbon index {index} is out of range. Bonbons={bonbons.Count}");
                return;
            }

            BonbonMN b = bonbons[index];
            if (b == null)
            {
                Debug.LogWarning($"{LogTag} Bonbon at index {index} is NULL.");
                return;
            }

            if (b.Collected)
            {
                // No spam: we keep this as a normal log (not warning).
                Debug.Log($"{LogTag} {(i + 1)} -> {b.name} already collected (ignored).");
                return;
            }

            Debug.Log($"{LogTag} {(i + 1)} -> Collect {b.name}");
            b.Collect();
            return;
        }
    }
}
