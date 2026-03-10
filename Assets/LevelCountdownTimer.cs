using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NavKeypad
{
    public class LevelCountdownTimer : MonoBehaviour
    {
        [Header("Timer Settings")]
        [SerializeField] private float totalTime = 360f; // 6 Minuten in Sekunden

        [Header("Component References")]
        [SerializeField] private TMP_Text timerDisplayText;

        [Header("Visuals")]
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 1f);       // Weiß
        [SerializeField] private Color warningColor = new Color(1f, 0.5f, 0f, 1f);    // Orange ab 1 Min
        [SerializeField] private Color criticalColor = new Color(1f, 0f, 0f, 1f);     // Rot ab 30 Sek

        [Header("Optional: Keypad Reference")]
        [Tooltip("Falls gesetzt, stoppt der Timer wenn Zugang gewährt wurde")]
        [SerializeField] private Keypad keypad;

        private float remainingTime;
        private bool timerRunning = true;

        private void Start()
        {
            remainingTime = totalTime;

            // Auf AccessGranted lauschen um Timer zu stoppen
            if (keypad != null)
            {
                keypad.OnAccessGranted.AddListener(StopTimer);
            }

            UpdateDisplay();
        }

        private void Update()
        {
            if (!timerRunning) return;

            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                UpdateDisplay();
                TimerFinished();
                return;
            }

            UpdateDisplay();
            UpdateColor();
        }

        private void UpdateDisplay()
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerDisplayText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        private void UpdateColor()
        {
            if (remainingTime <= 30f)
                timerDisplayText.color = criticalColor;
            else if (remainingTime <= 60f)
                timerDisplayText.color = warningColor;
            else
                timerDisplayText.color = normalColor;
        }

        private void TimerFinished()
        {
            timerRunning = false;
            timerDisplayText.text = "00:00";
            timerDisplayText.color = criticalColor;

            // Level nach kurzer Pause neu laden
            StartCoroutine(ReloadSceneAfterDelay(1.5f));
        }

        private IEnumerator ReloadSceneAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void StopTimer()
        {
            timerRunning = false;
            timerDisplayText.color = normalColor;
        }

        public void ResetTimer()
        {
            remainingTime = totalTime;
            timerRunning = true;
            timerDisplayText.color = normalColor;
        }
    }
}