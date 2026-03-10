using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace NavKeypad
{
    public class LetterInteractable : MonoBehaviour
    {
        [Header("Puppen Referenz")]
        [Tooltip("Frau-Linde-Puppe wird aktiviert wenn der Brief aufgenommen wird")]
        [SerializeField] private DollSpeaker frauLindeDoll;

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

        private void Awake()
        {
            grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        private void OnEnable()
        {
            grabInteractable.selectEntered.AddListener(OnLetterGrabbed);
        }

        private void OnDisable()
        {
            grabInteractable.selectEntered.RemoveListener(OnLetterGrabbed);
        }

        private void OnLetterGrabbed(SelectEnterEventArgs args)
        {
            frauLindeDoll?.Speak();
        }
    }
}