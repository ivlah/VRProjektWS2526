using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ChapterIntro : MonoBehaviour
{
    [Header("Chapter Texte")]
    [SerializeField] private string chapterTitle    = "Chapter 1";
    [SerializeField] private string chapterSubtitle = "My Love";

    [Header("Timing")]
    [Range(3f, 7f)]
    [SerializeField] private float displayDuration = 5f;

    [Range(0.3f, 1.5f)]
    [SerializeField] private float fadeDuration = 0.8f;

    private Canvas introCanvas;
    private Image blackOverlay;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI subtitleText;
    private Camera headCam;

    private void Awake()
    {
        headCam = Camera.main;
        if (headCam == null) headCam = FindObjectOfType<Camera>();
        BuildUI();
    }

    private void Start()
    {
        StartCoroutine(PlayIntro());
    }

    private void BuildUI()
    {
        GameObject cGO = new GameObject("ChapterIntroCanvas");

        introCanvas = cGO.AddComponent<Canvas>();
        introCanvas.renderMode    = RenderMode.ScreenSpaceCamera;
        introCanvas.worldCamera   = headCam;
        introCanvas.planeDistance = 0.5f;
        introCanvas.sortingOrder  = 999;

        cGO.AddComponent<CanvasScaler>();
        cGO.AddComponent<GraphicRaycaster>();

        GameObject overlayGO = new GameObject("BlackOverlay");
        overlayGO.transform.SetParent(cGO.transform, false);
        blackOverlay = overlayGO.AddComponent<Image>();
        blackOverlay.color = new Color(0f, 0f, 0f, 1f);
        RectTransform oRt  = overlayGO.GetComponent<RectTransform>();
        oRt.anchorMin      = Vector2.zero;
        oRt.anchorMax      = Vector2.one;
        oRt.offsetMin      = oRt.offsetMax = Vector2.zero;

        GameObject titleGO = new GameObject("ChapterTitle");
        titleGO.transform.SetParent(cGO.transform, false);
        titleText           = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text      = chapterTitle;
        titleText.fontSize  = 72f;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color     = new Color(1f, 0.95f, 0.85f, 0f);
        RectTransform tRt   = titleGO.GetComponent<RectTransform>();
        tRt.anchorMin       = new Vector2(0.05f, 0.52f);
        tRt.anchorMax       = new Vector2(0.95f, 0.70f);
        tRt.offsetMin       = tRt.offsetMax = Vector2.zero;

        GameObject subGO = new GameObject("ChapterSubtitle");
        subGO.transform.SetParent(cGO.transform, false);
        subtitleText           = subGO.AddComponent<TextMeshProUGUI>();
        subtitleText.text      = chapterSubtitle;
        subtitleText.fontSize  = 42f;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.fontStyle = FontStyles.Italic;
        subtitleText.color     = new Color(0.75f, 0.70f, 0.60f, 0f);
        RectTransform sRt      = subGO.GetComponent<RectTransform>();
        sRt.anchorMin          = new Vector2(0.05f, 0.36f);
        sRt.anchorMax          = new Vector2(0.95f, 0.52f);
        sRt.offsetMin          = sRt.offsetMax = Vector2.zero;
    }

    private IEnumerator PlayIntro()
    {
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(FadeTexts(0f, 1f, fadeDuration));
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(FadeTexts(1f, 0f, fadeDuration));
        yield return StartCoroutine(FadeOverlay(1f, 0f, fadeDuration));

        Destroy(introCanvas.gameObject);
        Destroy(gameObject);
    }

    private IEnumerator FadeOverlay(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            blackOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, t / duration));
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
            float a            = Mathf.Lerp(from, to, t / duration);
            titleText.color    = new Color(1f,    0.95f, 0.85f, a);
            subtitleText.color = new Color(0.75f, 0.70f, 0.60f, a);
            yield return null;
        }
    }
}