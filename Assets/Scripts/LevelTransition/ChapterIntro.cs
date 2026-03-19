using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// A Doll's House — Chapter Intro
///
/// Zeigt beim Start der Szene einen schwarzen Screen mit großem Chapter-Titel.
/// Dauer: 5-7 Sekunden, dann Ausblende in die Szene.
///
/// SETUP — in JEDER Szene einmal:
///   1. Leeres GameObject → "ChapterIntro"
///   2. Dieses Script dranhängen
///   3. Texte im Inspector eintragen:
///
///   Room1   → Chapter Title: "Chapter 1"  | Subtitle: "My Love"
///   Room2   → Chapter Title: "Chapter 2"  | Subtitle: "The Letter"
///   Room3   → Chapter Title: "Chapter 3"  | Subtitle: "Run"
/// </summary>
public class ChapterIntro : MonoBehaviour
{
    [Header("Chapter Texte")]
    [SerializeField] private string chapterTitle    = "Chapter 1";
    [SerializeField] private string chapterSubtitle = "My Love";

    [Header("Timing")]
    [Tooltip("Wie lange der schwarze Screen sichtbar bleibt (5-7 Sekunden empfohlen).")]
    [Range(5f, 7f)]
    [SerializeField] private float displayDuration = 6f;

    [Tooltip("Geschwindigkeit der Ein- und Ausblende.")]
    [Range(0.3f, 1.5f)]
    [SerializeField] private float fadeDuration = 0.8f;

    // ---------------------------------------------------------------
    // Private
    // ---------------------------------------------------------------

    private Canvas introCanvas;
    private Image  blackOverlay;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI subtitleText;

    // ---------------------------------------------------------------

    private void Awake()
    {
        BuildUI();
    }

    private void Start()
    {
        StartCoroutine(PlayIntro());
    }

    // ---------------------------------------------------------------
    // UI aufbauen
    // ---------------------------------------------------------------

    private void BuildUI()
    {
        // Canvas
        GameObject cGO = new GameObject("ChapterIntroCanvas");
        introCanvas = cGO.AddComponent<Canvas>();
        introCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        introCanvas.sortingOrder = 999;
        cGO.AddComponent<CanvasScaler>();
        cGO.AddComponent<GraphicRaycaster>();

        // Schwarzes Overlay — startet voll schwarz
        GameObject overlayGO = new GameObject("BlackOverlay");
        overlayGO.transform.SetParent(cGO.transform, false);
        blackOverlay = overlayGO.AddComponent<Image>();
        blackOverlay.color = new Color(0f, 0f, 0f, 1f); // sofort schwarz
        RectTransform oRt  = overlayGO.GetComponent<RectTransform>();
        oRt.anchorMin      = Vector2.zero;
        oRt.anchorMax      = Vector2.one;
        oRt.offsetMin      = oRt.offsetMax = Vector2.zero;

        // Chapter Titel — groß, zentriert, leicht über Mitte
        GameObject titleGO  = new GameObject("ChapterTitle");
        titleGO.transform.SetParent(cGO.transform, false);
        titleText           = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text      = chapterTitle;
        titleText.fontSize  = 72f;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color     = new Color(1f, 0.95f, 0.85f, 0f); // startet unsichtbar
        RectTransform tRt   = titleGO.GetComponent<RectTransform>();
        tRt.anchorMin       = new Vector2(0.05f, 0.50f);
        tRt.anchorMax       = new Vector2(0.95f, 0.68f);
        tRt.offsetMin       = tRt.offsetMax = Vector2.zero;

        // Subtitle — kleiner, kursiv, direkt unter dem Titel
        GameObject subGO    = new GameObject("ChapterSubtitle");
        subGO.transform.SetParent(cGO.transform, false);
        subtitleText        = subGO.AddComponent<TextMeshProUGUI>();
        subtitleText.text   = chapterSubtitle;
        subtitleText.fontSize  = 38f;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.fontStyle = FontStyles.Italic;
        subtitleText.color     = new Color(0.75f, 0.70f, 0.60f, 0f);
        RectTransform sRt      = subGO.GetComponent<RectTransform>();
        sRt.anchorMin          = new Vector2(0.05f, 0.38f);
        sRt.anchorMax          = new Vector2(0.95f, 0.52f);
        sRt.offsetMin          = sRt.offsetMax = Vector2.zero;
    }

    // ---------------------------------------------------------------
    // Intro Sequenz
    // ---------------------------------------------------------------

    private IEnumerator PlayIntro()
    {
        // Szene ist geladen aber noch komplett schwarz
        // Kurz warten damit alles initialisiert ist
        yield return new WaitForSeconds(0.1f);

        // 1. Text einblenden (Overlay bleibt schwarz)
        yield return StartCoroutine(FadeTexts(0f, 1f, fadeDuration));

        // 2. displayDuration Sekunden stehen lassen
        yield return new WaitForSeconds(displayDuration);

        // 3. Text ausblenden
        yield return StartCoroutine(FadeTexts(1f, 0f, fadeDuration));

        // 4. Schwarzes Overlay ausblenden → Szene wird sichtbar
        yield return StartCoroutine(FadeOverlay(1f, 0f, fadeDuration));

        // 5. Canvas entfernen — wird nicht mehr gebraucht
        Destroy(introCanvas.gameObject);
        Destroy(gameObject);
    }

    // ---------------------------------------------------------------
    // Fade Helpers
    // ---------------------------------------------------------------

    private IEnumerator FadeOverlay(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            blackOverlay.color = new Color(0f, 0f, 0f,
                Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        blackOverlay.color = new Color(0f, 0f, 0f, to);
    }

    private IEnumerator FadeTexts(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a        = Mathf.Lerp(from, to, t / duration);
            titleText.color    = new Color(1f,    0.95f, 0.85f, a);
            subtitleText.color = new Color(0.75f, 0.70f, 0.60f, a);
            yield return null;
        }
    }
}
