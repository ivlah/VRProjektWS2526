using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public float distance = 2f;
    public float height = 0f;
    
    private Transform cam;
    
    void Start()
    {
        cam = Camera.main.transform;
    }
    
    void OnEnable()
    {
        if (cam == null) cam = Camera.main.transform;
        PositionInFront();
    }
    
    void PositionInFront()
    {
        Vector3 forward = cam.forward;
        forward.y = 0; // Nur horizontal
        forward.Normalize();
        
        transform.position = cam.position + forward * distance + Vector3.up * height;
        transform.rotation = Quaternion.LookRotation(forward);
    }
}