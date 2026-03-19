using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public float openAngle = -90f; // Negativ = nach außen
    public float openSpeed = 2f;
    
    private bool isOpening = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private BoxCollider boxCollider;
    
    void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = Quaternion.Euler(0, openAngle, 0);
        boxCollider = GetComponent<BoxCollider>();
    }
    
    public void OpenDoor()
    {
        isOpening = true;
        if (boxCollider != null)
            boxCollider.enabled = false; // Collider deaktivieren
    }
    
    void Update()
    {
        if (isOpening)
        {
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation, 
                openRotation, 
                Time.deltaTime * openSpeed
            );
        }
    }
}