using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Room1BackendRoot
/// ----------------
/// Clean composition root for Room1 (LivingRoom).
///
/// RESPONSIBILITY:
/// - Acts as the single deterministic bootstrap anchor.
/// - Initializes all backend systems in a defined order.
/// - Does NOT scan the scene.
/// - Uses explicit Inspector wiring only (team-safe).
///
/// DESIGN:
/// - Deterministic startup
/// - No hidden dependencies
/// - No scene-wide reflection scans
/// - Explicit architecture
/// </summary>
public class Room1BackendRoot : MonoBehaviour
{
    /// <summary>
    /// Enable/disable backend logging.
    /// </summary>
    [SerializeField] private bool enableLogs = true;

    /// <summary>
    /// Explicit list of backend systems to initialize.
    /// Order does NOT matter here — it will be sorted by InitOrder.
    ///
    /// IMPORTANT:
    /// Drag ALL IRoom1Initializable components here in Inspector.
    /// </summary>
    [Header("Backend Systems (Assign in Inspector)")]
    [SerializeField] private MonoBehaviour[] backendSystems;

    private readonly List<IRoom1Initializable> initializables = new();
    private bool isInitialized = false;

    private const string LogTag = "[Room1BackendRoot]";

    /// <summary>
    /// Deterministic entry point.
    /// Called once from Room1TestMode.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
        {
            if (enableLogs)
                Debug.Log($"{LogTag} Initialize() skipped (already initialized).");
            return;
        }

        isInitialized = true;

        if (enableLogs)
            Debug.Log($"{LogTag} Initialization started...");

        CollectInitializables();
        SortByOrder();
        RunInitializationChain();

        if (enableLogs)
            Debug.Log($"{LogTag} Initialization finished ✅");
    }

    /// <summary>
    /// Collects only explicitly assigned backend systems.
    /// No scene scanning.
    /// </summary>
    private void CollectInitializables()
    {
        initializables.Clear();

        if (backendSystems == null || backendSystems.Length == 0)
        {
            Debug.LogError($"{LogTag} No backend systems assigned in Inspector.");
            return;
        }

        foreach (var mb in backendSystems)
        {
            if (mb == null)
            {
                Debug.LogError($"{LogTag} NULL reference in backendSystems array.");
                continue;
            }

            if (mb is IRoom1Initializable init)
            {
                initializables.Add(init);
            }
            else
            {
                Debug.LogError($"{LogTag} {mb.GetType().Name} does not implement IRoom1Initializable.");
            }
        }

        if (enableLogs)
            Debug.Log($"{LogTag} Found {initializables.Count} initializable component(s).");
    }

    /// <summary>
    /// Sort systems by InitOrder to guarantee deterministic startup.
    /// </summary>
    private void SortByOrder()
    {
        initializables.Sort((a, b) => a.InitOrder.CompareTo(b.InitOrder));

        if (!enableLogs) return;

        for (int i = 0; i < initializables.Count; i++)
        {
            Debug.Log($"{LogTag} Init[{i}] = {initializables[i].GetType().Name} (Order {initializables[i].InitOrder})");
        }
    }

    /// <summary>
    /// Executes Initialize() on each backend system.
    /// </summary>
    private void RunInitializationChain()
    {
        foreach (var init in initializables)
        {
            try
            {
                init.Initialize(this);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{LogTag} Initialization failed in {init.GetType().Name}: {ex}");
            }
        }
    }
}
