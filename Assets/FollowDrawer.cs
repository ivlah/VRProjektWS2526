using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FollowDrawer : MonoBehaviour
{
    public Transform drawer; // Schublade
    private Vector3 offset;

    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    private bool isGrabbed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Abstand zwischen Schublade und Brief merken
        offset = transform.position - drawer.position;

        // Anfangszustand: Brief liegt ruhig in der Schublade
        rb.useGravity = false;
        rb.isKinematic = true;

        // Event registrieren
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void LateUpdate()
    {
        // Nur mitbewegen, wenn der Brief nicht gegriffen wird
        if (!isGrabbed && drawer != null)
        {
            transform.position = drawer.position + offset;
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        // Beim Aufheben kinematic lassen, der Interactor übernimmt
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        // Physik aktivieren, sobald der Spieler den Brief loslässt
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}