using UnityEngine;

public class DrawerController : MonoBehaviour
{
    public Transform drawer;
    public Vector3 openPosition;
    public Vector3 closedPosition;

    public float speed = 2f;

    private bool isOpen = false;

    void Update()
    {
        if (isOpen)
        {
            drawer.localPosition = Vector3.Lerp(drawer.localPosition, openPosition, Time.deltaTime * speed);
        }
        else
        {
            drawer.localPosition = Vector3.Lerp(drawer.localPosition, closedPosition, Time.deltaTime * speed);
        }
    }

    public void ToggleDrawer()
    {
        isOpen = !isOpen;
    }
}