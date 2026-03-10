using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LetterPhysicsOnGrab : MonoBehaviour
{
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Anfangszustand: Brief liegt ruhig in der Schublade
        rb.useGravity = false;
        rb.isKinematic = true;

        // Event registrieren
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // Sobald Spieler den Brief nimmt → normale Physik
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}