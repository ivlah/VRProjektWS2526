using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// XRDoorOpenAdapterMN
/// -------------------
/// Minimal adapter that connects XR interaction (select) to the existing DoorMN backend.
/// 
/// WHY THIS EXISTS:
/// - DoorMN unlocks itself automatically when the code is complete.
/// - DoorMN does NOT open automatically by design.
/// - In DebugUI/Keyboard, opening is triggered by a button or key that calls DoorMN.TryOpen().
/// - In XR mode, we need the equivalent trigger: selecting the door with the XR ray should call TryOpen().
/// 
/// GUARANTEES:
/// - Does not modify DoorMN logic.
/// - Does not change unlocking rules.
/// - Only calls DoorMN.TryOpen() when the user selects the door.
/// - Safe even if the door is still locked (DoorMN will log "[DoorMN] LOCKED..." and do nothing).
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class XRDoorOpenAdapterMN : MonoBehaviour
{
    [Header("Backend Reference")]
    [Tooltip("DoorMN backend component that controls unlocking/opening logic.")]
    [SerializeField] private DoorMN door;

    private XRSimpleInteractable interactable;

    private void Awake()
    {
        // Cache and subscribe once.
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnSelectEntered);

        // Convenience fallback: if not assigned, try to get DoorMN from the same GameObject.
        if (door == null)
            door = GetComponent<DoorMN>();
    }

    private void OnDestroy ()
    {
        // Unsubscribe to avoid duplicate listeners after domain reload / re-entering play mode.
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (door == null)
        {
            Debug.LogWarning("[XRDoorOpenAdapterMN] DoorMN reference missing. Assign it in the Inspector.");
            return;
        }

        // Calls the existing VR-ready entry point.
        door.TryOpen();
    }
}