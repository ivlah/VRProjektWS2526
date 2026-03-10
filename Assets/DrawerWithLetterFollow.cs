using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DrawerWithLetterFollow : MonoBehaviour
{
    public Transform drawer;
    public Transform letter;

    public Vector3 openPosition;
    public Vector3 closedPosition;

    public float speed = 2f;

    private bool isOpen = false;

    private Vector3 letterOffset;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable letterGrab;
    private bool letterGrabbed = false;

    void Start()
    {
        if (drawer != null && letter != null)
        {
            // Abstand zwischen Schublade und Brief speichern
            letterOffset = letter.position - drawer.position;

            // XR Grab Interactable holen
            letterGrab = letter.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

            if (letterGrab != null)
            {
                letterGrab.selectEntered.AddListener(OnLetterGrabbed);
            }
        }
    }

    void Update()
    {
        // Schublade bewegen
        if (isOpen)
        {
            drawer.localPosition = Vector3.Lerp(drawer.localPosition, openPosition, Time.deltaTime * speed);
        }
        else
        {
            drawer.localPosition = Vector3.Lerp(drawer.localPosition, closedPosition, Time.deltaTime * speed);
        }

        // Brief nur mitbewegen solange er nicht aufgehoben wurde
        if (!letterGrabbed && letter != null)
        {
            letter.position = drawer.position + letterOffset;
        }
    }

    public void ToggleDrawer()
    {
        isOpen = !isOpen;
    }

    void OnLetterGrabbed(SelectEnterEventArgs args)
    {
        letterGrabbed = true;
    }
}