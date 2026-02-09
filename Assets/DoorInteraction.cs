using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorInteraction : MonoBehaviour
{
    public float openAngle = -90f;
    public float speed = 2f;
    private bool isOpen = false;
    
    public void ToggleDoor()
    {
        isOpen = !isOpen;
    }
    
    void Update()
    {
        float targetY = isOpen ? openAngle : 0f;
        float currentY = transform.localEulerAngles.y;
        if (currentY > 180) currentY -= 360;
        
        float newY = Mathf.Lerp(currentY, targetY, Time.deltaTime * speed);
        transform.localRotation = Quaternion.Euler(0, newY, 0);
    }
}