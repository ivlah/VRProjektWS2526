using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class RoleCardManager : MonoBehaviour
{
    public int currentStep = 1;
    public float timeLimit = 180f;
    public float timePenalty = 30f;
    
    public TextMeshProUGUI timerText;
    public GameObject failMessage; // NEU
    public AudioSource torvaldVoice;
    public AudioClip[] torvaldClips;
    public UnityEvent onLevelComplete;
    public UnityEvent onLevelFailed;
    
    private float timeRemaining;
    private bool levelActive = true;
    
    void Start()
    {
        timeRemaining = timeLimit;
    }
    
    void Update()
    {
        if (!levelActive) return;
        
        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();
        
        if (timeRemaining <= 0)
        {
            LevelFailed();
        }
    }
    
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    public void OnCorrectCut()
    {
        currentStep++;
        
        if (currentStep > 4)
        {
            LevelComplete();
        }
    }
    
    public void OnWrongCut()
    {
        timeRemaining -= timePenalty;
        PlayTorvaldVoice();
    }
    
    void PlayTorvaldVoice()
    {
        if (torvaldVoice != null && torvaldClips.Length > 0)
        {
            int randomIndex = Random.Range(0, torvaldClips.Length);
            torvaldVoice.clip = torvaldClips[randomIndex];
            torvaldVoice.Play();
        }
    }
    
    void LevelComplete()
    {
        levelActive = false;
        onLevelComplete.Invoke();
    }
    
    void LevelFailed()
    {
        levelActive = false;
        
        // Nachricht anzeigen
        if (failMessage != null)
        {
            failMessage.SetActive(true);
        }
        
        onLevelFailed.Invoke();
        StartCoroutine(ResetLevel());
    }
    
    System.Collections.IEnumerator ResetLevel()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}