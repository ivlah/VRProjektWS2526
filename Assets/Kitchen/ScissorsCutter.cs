using UnityEngine;

public class ScissorsCutter : MonoBehaviour
{
    private RoleCardManager manager;
    
    void Start()
    {
        manager = FindObjectOfType<RoleCardManager>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("String"))
        {
            CuttableString cuttable = other.GetComponent<CuttableString>();
            if (cuttable != null)
            {
                cuttable.Cut(manager.currentStep);
            }
        }
    }
}