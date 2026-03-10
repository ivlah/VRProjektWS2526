using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorPull : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private Transform handTransform;
    private float startAngle;
    private Vector3 pivotPos;

    void Start()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        pivotPos = transform.position;
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        handTransform = args.interactorObject.transform;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        handTransform = null;
    }

    void Update()
    {
        if (handTransform != null)
        {
            Vector3 dir = handTransform.position - pivotPos;
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            angle = Mathf.Clamp(angle, -90f, 0f);
            transform.localRotation = Quaternion.Euler(0, angle, 0);
        }
    }
}