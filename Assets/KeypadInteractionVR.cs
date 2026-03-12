using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

namespace NavKeypad
{
    public class KeypadInteractionVR : MonoBehaviour
    {
        [Header("Controller References")]
        [SerializeField] private Transform rightHandTransform;
        [SerializeField] private Transform leftHandTransform;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference rightHandTrigger;
        [SerializeField] private InputActionReference leftHandTrigger;

        [Header("Settings")]
        [SerializeField] private float raycastDistance = 5f;
        [SerializeField] private LayerMask keypadLayer;

        [Header("Visual Ray (optional)")]
        [SerializeField] private LineRenderer rightHandRay;
        [SerializeField] private LineRenderer leftHandRay;

        private bool rightTriggerWasPressed = false;
        private bool leftTriggerWasPressed = false;

        private void OnEnable()
        {
            rightHandTrigger?.action.Enable();
            leftHandTrigger?.action.Enable();
        }

        private void OnDisable()
        {
            rightHandTrigger?.action.Disable();
            leftHandTrigger?.action.Disable();
        }

        private void Update()
        {
            HandleController(
                rightHandTransform,
                rightHandTrigger,
                ref rightTriggerWasPressed,
                rightHandRay
            );

            HandleController(
                leftHandTransform,
                leftHandTrigger,
                ref leftTriggerWasPressed,
                leftHandRay
            );
        }

        private void HandleController(Transform controllerTransform, InputActionReference triggerAction,
                                       ref bool wasPressed, LineRenderer lineRenderer)
        {
            if (controllerTransform == null || triggerAction == null) return;

            Ray ray = new Ray(controllerTransform.position, controllerTransform.forward);
            bool triggerPressed = triggerAction.action.ReadValue<float>() > 0.5f;
            bool triggerDown = triggerPressed && !wasPressed;
            wasPressed = triggerPressed;

            // Visuellen Ray aktualisieren (optional)
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, controllerTransform.position);
                if (Physics.Raycast(ray, out var hitVis, raycastDistance, keypadLayer))
                    lineRenderer.SetPosition(1, hitVis.point);
                else
                    lineRenderer.SetPosition(1, ray.GetPoint(raycastDistance));
            }

            // Beim Drücken des Triggers prüfen ob ein KeypadButton oder CabinetDoor getroffen wird
            if (triggerDown)
            {
                Debug.Log("Trigger gedrückt!");  // ← neu
                if (Physics.Raycast(ray, out var hit, raycastDistance, keypadLayer))
                {
                    Debug.Log("Raycast trifft: " + hit.collider.gameObject.name);  // ← neu
                    if (hit.collider.TryGetComponent(out KeypadButton keypadButton))
                    {
                        keypadButton.PressButton();
                    }
                    else if (hit.collider.GetComponentInParent<CabinetDoor>() is CabinetDoor cabinetDoor)
                    {
                        cabinetDoor.ToggleDoor();
                    }
                }
            }
        }
    }
}
