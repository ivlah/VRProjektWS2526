using UnityEngine;


public class ControllerDebug : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor directInteractor;

    void Start()
    {
        directInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();
    }

    void Update()
    {
        if (directInteractor.hasSelection)
            Debug.Log("Controller greift gerade etwas!");
            
        if (directInteractor.isHoverActive)
            Debug.Log("Controller berührt: " + directInteractor.interactablesHovered.Count + " Objekte");
    }
}