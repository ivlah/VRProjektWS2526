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
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        card = transform.parent.GetComponentInChildren<MeshRenderer>().transform;
        cardRenderer = card.GetComponent<MeshRenderer>();
        originalColor = cardRenderer.material.color;
    }
    
    public void Cut(int currentStep)
    {
        if (isCut || isLocked) return;
        
        if (correctOrder == currentStep)
        {
            // Richtig geschnitten
            isCut = true;
            lineRenderer.enabled = false;
            StartCoroutine(FallCard());
            onCut.Invoke();
        }
        else
        {
            // Falsch geschnitten
            StartCoroutine(WrongCutSequence());
            onWrongCut.Invoke();
        }
    }
    
    System.Collections.IEnumerator WrongCutSequence()
    {
        isLocked = true;
        
        // Fail Sound
        if (audioSource != null && failSound != null)
        {
            audioSource.PlayOneShot(failSound);
        }
        
        // Karte rot färben
        cardRenderer.material.color = Color.red;
        
        // 5 Sekunden warten
        yield return new WaitForSeconds(5f);
        
        // Zurücksetzen
        cardRenderer.material.color = originalColor;
        isLocked = false;
    }
    
    System.Collections.IEnumerator FallCard()
    {
        Rigidbody rb = card.gameObject.AddComponent<Rigidbody>();
        rb.mass = 0.1f;
        yield return new WaitForSeconds(2f);
        card.gameObject.SetActive(false);
    }
}