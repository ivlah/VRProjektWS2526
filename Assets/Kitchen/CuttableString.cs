using UnityEngine;
using UnityEngine.Events;

public class CuttableString : MonoBehaviour
{
    public int correctOrder;
    public UnityEvent onCut;
    public UnityEvent onWrongCut;
    public MeshRenderer cardRenderer;
    public AudioSource audioSource;
    public AudioClip failSound;
    
    private bool isCut = false;
    private bool isLocked = false;
    private Color originalColor;
    private LineRenderer lineRenderer;
    private Transform card;
    private Vector3 originalCardPosition;
    private Quaternion originalCardRotation;
    private Rigidbody cardRigidbody;
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        card = transform.parent.GetComponentInChildren<MeshRenderer>().transform;
        cardRenderer = card.GetComponent<MeshRenderer>();
        originalColor = cardRenderer.material.color;
        originalCardPosition = card.localPosition;
        originalCardRotation = card.localRotation;
    }
    
    public void Cut(int currentStep)
    {
        if (isCut || isLocked) return;
        
        if (correctOrder == currentStep)
        {
            isCut = true;
            lineRenderer.enabled = false;
            StartCoroutine(FallCard());
            onCut.Invoke();
        }
        else
        {
            StartCoroutine(WrongCutSequence());
            onWrongCut.Invoke();
        }
    }
    
    System.Collections.IEnumerator WrongCutSequence()
    {
        isLocked = true;
        
        if (audioSource != null && failSound != null)
        {
            audioSource.PlayOneShot(failSound);
        }
        
        cardRenderer.material.color = Color.red;
        
        yield return new WaitForSeconds(5f);
        
        cardRenderer.material.color = originalColor;
        isLocked = false;
    }
    
    System.Collections.IEnumerator FallCard()
    {
        cardRigidbody = card.gameObject.AddComponent<Rigidbody>();
        cardRigidbody.mass = 0.1f;
        yield return null;
    }
    
    public void ResetCard()
    {
        // Rigidbody entfernen falls vorhanden
        if (cardRigidbody != null)
        {
            Destroy(cardRigidbody);
        }
        
        // Karte zurücksetzen
        card.gameObject.SetActive(true);
        card.localPosition = originalCardPosition;
        card.localRotation = originalCardRotation;
        
        // String zurücksetzen
        lineRenderer.enabled = true;
        isCut = false;
        isLocked = false;
        
        // Farbe zurücksetzen
        cardRenderer.material.color = originalColor;
    }
}