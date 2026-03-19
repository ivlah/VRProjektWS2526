using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// A Doll's House — Level Timer (Wall Placement)
///
/// Timer als 3D Objekt das du frei in der Szene platzieren kannst.
/// Funktioniert wie eine Uhr an der Wand.
///
/// SETUP:
///   1. Leeres GameObject → "LevelTimer" → an Wand positionieren
///   2. Dieses Script dranhängen
///   3. Zeit im Inspector eintragen:
///      Room1 + Room2 → 225 Sekunden (3:45)
///      Room3         → 285 Sekunden (4:45)
///   4. Script erstellt den Timer Canvas automatisch
///   5. GameObject in der Szene frei verschieben und drehen
/// </summary>
public class LevelTimer : MonoBehaviour
{
    [Header("Zeit")]
    [Tooltip("Room1 + Room2: 225  |  Room3: 285")]
    [SerializeField] private float startTime = 225f;

    [Header("Schrift")]
    [Tooltip("Deine TMP Font Asset — einfach reinziehen.")]
    [SerializeField] private TMP_FontAsset font;

    [Tooltip("Schriftgröße des Timers.")]
    [SerializeField] private float fontSize = 18f;

    [Tooltip("Normale Farbe.")]
    [SerializeField] private Color normalColor  = new Color(0.95f, 0.92f, 0.85f, 1f);

    [Tooltip("Farbe wenn wenig Zeit übrig.")]
    [SerializeField] private Color warningColor = new Color(0.95f, 0.20f, 0.15f, 1f);

    [Tooltip("Ab wieviel Sekunden wird der Timer rot.")]
    [SerializeField] private float warningAt = 30f;

    [Header("Canvas Größe")]
    [Tooltip("Breite des Timer-Canvas in World Units.")]
    [SerializeField] private float canvasWidth  = 4f;
    [Tooltip("Höhe des Timer-Canvas in World Units.")]
    [SerializeField] private float canvasHeight = 1.5f;

    [Header("Hintergrund")]
    [Tooltip("Hintergrundfarbe hinter dem Timer.")]
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.55f);
    [Tooltip("Hintergrund anzeigen?")]
    [SerializeField] private bool showBackground = true;

    // ---------------------------------------------------------------
    // Private
    // ---------------------------------------------------------------

    private float timeRemaining;
    private bool  isRunning = true;
    private TextMeshProUGUI timerText;
    private UnityEngine.UI.Image bgImage;

    // ---------------------------------------------------------------
    // Setup
    // ---------------------------------------------------------------

    private void Awake()
    {
        timeRemaining = startTime;
        BuildTimerCanvas();
    }

    private void BuildTimerCanvas()
    {
        // World Space Canvas direkt auf diesem GameObject
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta  = new Vector2(canvasWidth * 100f, canvasHeight * 100f);
        rt.localScale = Vector3.one * 0.01f;

        // Hintergrund
        if (showBackground)
        {
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(transform, false);
            bgImage = bgGO.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = backgroundColor;
            RectTransform bgRt = bgGO.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        }

        // Timer Text
        GameObject textGO = new GameObject("TimerText");
        textGO.transform.SetParent(transform, false);
        timerText = textGO.AddComponent<TextMeshProUGUI>();
        timerText.text      = FormatTime(startTime);
        timerText.fontSize  = fontSize;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.color     = normalColor;

        if (font != null)
            timerText.font = font;

        RectTransform textRt = textGO.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;
    }

    // ---------------------------------------------------------------
    // Timer Logic
    // ---------------------------------------------------------------

    private void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;
        timeRemaining  = Mathf.Max(0f, timeRemaining);

        UpdateDisplay();

        if (timeRemaining <= 0f)
        {
            isRunning = false;
            StartCoroutine(RestartScene());
        }
    }

    private void UpdateDisplay()
    {
        if (timerText == null) return;

        timerText.text  = FormatTime(timeRemaining);
        timerText.color = timeRemaining <= warningAt ? warningColor : normalColor;

        // Pulsieren wenn Warnung
        if (timeRemaining <= warningAt)
        {
            float pulse = Mathf.Sin(Time.time * 4f) * 0.15f + 0.85f;
            timerText.transform.localScale = Vector3.one * pulse;
        }
    }

    // ---------------------------------------------------------------
    // Scene Restart
    // ---------------------------------------------------------------

    private IEnumerator RestartScene()
    {
        // Kurz warten dann Szene neu starten
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ---------------------------------------------------------------
    // Utility
    // ---------------------------------------------------------------

    private string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }

    // ---------------------------------------------------------------
    // Public — von anderen Scripts aufrufbar
    // ---------------------------------------------------------------

    /// <summary>Timer pausieren (z.B. wenn Cutscene läuft).</summary>
    public void PauseTimer()  => isRunning = false;

    /// <summary>Timer fortsetzen.</summary>
    public void ResumeTimer() => isRunning = true;

    /// <summary>Timer zurücksetzen.</summary>
    public void ResetTimer()
    {
        timeRemaining = startTime;
        isRunning     = true;
    }
}
