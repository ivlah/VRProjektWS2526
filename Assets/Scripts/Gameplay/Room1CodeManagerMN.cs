using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Room1CodeManagerMN
/// ------------------
/// Central backend manager that collects digits and builds the current code.
///
/// "Better solution" upgrade:
/// - Provides events so other systems do NOT need polling:
///   - DigitRegisteredEvent: fired when a digit is added.
///   - CodeCompletedEvent: fired once when the required digits are reached.
///
/// Integration (TestMode / Deterministic Boot):
/// - This component no longer "boots" itself in Awake/OnEnable.
/// - It is initialized exactly once via Room1BackendRoot -> Initialize().
/// </summary>
public class Room1CodeManagerMN : MonoBehaviour, IRoom1Initializable
{
    [Header("Room1 Code Settings")]
    [SerializeField] private int requiredDigits = 4;

    private readonly List<int> collectedDigits = new List<int>();

    public int RequiredDigits => requiredDigits;
    public int CollectedCount => collectedDigits.Count;
    public bool IsComplete => collectedDigits.Count >= requiredDigits;

    public string CurrentCode
    {
        get
        {
            var sb = new StringBuilder();
            for (int i = 0; i < collectedDigits.Count; i++)
                sb.Append(collectedDigits[i]);
            return sb.ToString();
        }
    }

    /// <summary>
    /// DigitRegisteredEvent
    /// --------------------
    /// Fired whenever a digit is successfully registered.
    ///
    /// Parameters:
    /// - int digit: the digit that was added
    /// - string sourceName: the bonbon name (source)
    /// - string currentCode: the code after adding this digit
    /// </summary>
    public event Action<int, string, string> DigitRegisteredEvent;

    /// <summary>
    /// CodeCompletedEvent
    /// ------------------
    /// Fired exactly once when the code becomes complete.
    ///
    /// Parameter:
    /// - string finalCode: the completed code.
    /// </summary>
    public event Action<string> CodeCompletedEvent;

    private bool completeEventFired = false;

    /// <summary>
    /// Guard to ensure Initialize() runs only once.
    /// This prevents accidental double-boot if multiple systems call it.
    /// </summary>
    private bool isInitialized = false;

    /// <summary>
    /// Determines deterministic startup order among all IRoom1Initializable components.
    /// Code manager should initialize early, because other systems depend on it.
    /// </summary>
    public int InitOrder => 0;

    /// <summary>
    /// Awake is kept intentionally minimal for deterministic boot.
    /// It must NOT perform startup logic that would break ordered initialization.
    /// </summary>
    private void Awake()
    {
        // Intentionally empty (deterministic boot via Initialize()).
    }

    /// <summary>
    /// Deterministic initialization entry point (called once by Room1BackendRoot).
    /// </summary>
    public void Initialize(Room1BackendRoot root)
    {
        if (isInitialized) return;
        isInitialized = true;

        Debug.Log($"[Room1CodeManagerMN] Ready. RequiredDigits={requiredDigits}");
    }

    public void RegisterDigit(int digit, string sourceName = "")
    {
        if (IsComplete)
        {
            Debug.Log($"[Room1CodeManagerMN] Ignored digit {digit} (code already complete). Source={sourceName}");
            return;
        }

        collectedDigits.Add(digit);

        Debug.Log($"[Room1CodeManagerMN] Added digit {digit} (#{collectedDigits.Count}/{requiredDigits}) " +
                  $"Source={sourceName} CurrentCode={CurrentCode}");

        // Notify listeners that a digit was added.
        DigitRegisteredEvent?.Invoke(digit, sourceName, CurrentCode);

        // Fire the completion event exactly once.
        if (IsComplete && !completeEventFired)
        {
            completeEventFired = true;
            Debug.Log($"[Room1CodeManagerMN] CODE COMPLETE -> {CurrentCode}");
            CodeCompletedEvent?.Invoke(CurrentCode);
        }
    }

    public void ResetCode()
    {
        collectedDigits.Clear();
        completeEventFired = false;
        Debug.Log("[Room1CodeManagerMN] Code reset.");
    }
}
