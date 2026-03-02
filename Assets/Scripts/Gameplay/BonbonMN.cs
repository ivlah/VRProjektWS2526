using System;
using UnityEngine;

/// <summary>
/// BonbonMN
/// --------
/// Backend component representing a collectible bonbon (candy) in Room1.
///
/// "Better solution" upgrade:
/// - Adds an event (CollectedEvent) so other systems (e.g., GuidanceManager)
///   can react immediately when a bonbon is collected.
/// - No polling needed.
/// - Still VR-ready: XR will call Collect().
/// </summary>
public class BonbonMN : MonoBehaviour
{
    [Header("Bonbon Backend")]
    [SerializeField] private int digit = 1;

    [Header("Puzzle Logic")]
    [SerializeField] private bool isCorrect = true;
    [SerializeField] private int orderIndex = 0;

    private bool collected = false;

    public int Digit => digit;
    public bool Collected => collected;
    public bool IsCorrect => isCorrect;
    public int OrderIndex => orderIndex;

    /// <summary>
    /// CollectedEvent
    /// --------------
    /// Fired exactly once when this bonbon is collected.
    ///
    /// Parameters:
    /// - BonbonMN bonbon: the instance that was collected.
    ///
    /// Why this is useful:
    /// - Guidance manager can switch the puppet target immediately.
    /// - Debug UI can update without polling.
    /// </summary>
    public event Action<BonbonMN> CollectedEvent;

    public void Collect()
    {
        if (collected) return;
        collected = true;

        Debug.Log($"[BonbonMN] Collected {name} | Digit={digit} | Correct={isCorrect} | OrderIndex={orderIndex}");

        var mgr = FindFirstObjectByType<Room1CodeManagerMN>();
        if (mgr != null)
        {
            if (isCorrect)
            {
                mgr.RegisterDigit(digit, name);
            }
            else
            {
                Debug.LogWarning($"[BonbonMN] Decoy bonbon collected (ignored for code): {name}");
            }
        }
        else
        {
            Debug.LogWarning("[BonbonMN] No Room1CodeManagerMN found in scene.");
        }

        // IMPORTANT: Fire event BEFORE disabling, so listeners still have access to this instance.
        CollectedEvent?.Invoke(this);

        gameObject.SetActive(false);
    }

    /// <summary>
    /// ResetBonbon()
    /// ------------
    /// Resets this bonbon to its initial backend state so it can be collected again.
    ///
    /// What this does:
    /// - Sets the internal "collected" flag back to false.
    /// - Reactivates the GameObject (because Collect() disables it).
    ///
    /// Why this is needed:
    /// - Room resets should be deterministic for testing and for VR gameplay loops.
    /// - Reset logic should not hack private fields from outside; we provide a clean API.
    ///
    /// IMPORTANT:
    /// - This does not change the configured digit/isCorrect/orderIndex values.
    /// - Those are considered design-time configuration and remain unchanged.
    /// </summary>
    public void ResetBonbon()
    {
        collected = false;

        // Reactivate the bonbon so it becomes visible/collectible again.
        // This matches the "disable on collect" behavior used for backend testing.
        gameObject.SetActive(true);

        Debug.Log($"[BonbonMN] ResetBonbon -> {name} (Collected={collected})");
    }
}
