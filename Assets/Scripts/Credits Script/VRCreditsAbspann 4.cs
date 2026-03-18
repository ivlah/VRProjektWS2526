using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class VRCreditsAbspann : MonoBehaviour
{
    [Header("VR Setup")]
    public Transform xrOrigin;

    [Header("Fonts")]
    public TMP_FontAsset titelFont;
    public TMP_FontAsset namenFont;

    [Header("Schriftgrößen")]
    public float titelFontSize = 14f;
    public float kategorieFontSize = 10f;
    public float namenFontSize = 8f;

    [Header("Farben")]
    public Color kategorieColor = new Color(1f, 0.85f, 0.4f);
    public Color namenColor = Color.white;

    [Header("Credits Text (im Inspector editierbar)")]
    [TextArea(20, 40)]
    public string creditsText =
        "A Doll's House\n" +
        "\n" +
        "#Story\n" +
        "Harnoor Multani\n" +
        "Dodi Tschiko-Mulu\n" +
        "\n" +
        "#Programming\n" +
        "Maral Nassiri\n" +
        "Ilian Vlahovic\n" +
        "Erik\n" +
        "Oona Yu-Mi Song\n" +
        "\n" +
        "#3D Art & Design\n" +
        "Yannick Croes\n" +
        "Dodi Tschiko-Mulu\n" +
        "Harnoor Multani\n" +
        "\n" +
        "#Music & Sound\n" +
        "Oona Yu-Mi Song\n" +
        "\n" +
        "#Special Thanks to\n" +
        "Vincet and Salo\n" +
        "for Voice Acting and Sound Design\n" +
        "\n" +
        "#Thanks for playing our Game";

    [Header("Scroll")]
    public float scrollSpeed = 30f;
    public float canvasDistance = 4f;
    public float startDelay = 0.5f;

    [Header("Sterne")]
    public int starCount = 5000;
    public float starFieldRadius = 80f;
    public float starSizeMin = 0.02f;
    public float starSizeMax = 0.08f;
    public Color starColor = new Color(0.9f, 0.92f, 1f, 1f);
    public float twinkleSpeed = 1.5f;

    [Header("Kamerafahrt")]
    public float cameraFlySpeed = 2f;
    public float cameraRotationSpeed = 1f;

    [Header("Musik")]
    public AudioClip abspannMusik;
    [Range(0f, 1f)]
    public float musikLautstaerke = 0.8f;
    public float musikFadeIn = 2f;
    public float musikFadeOut = 3f;

    [Header("Fade")]
    public float fadeInDuration = 2f;

    [Header("Szene nach Abspann")]
    public string mainMenuScene = "MainMenu";

    private ParticleSystem sternPartikel;
    private Canvas creditsCanvas;
    private RectTransform creditsContainer;
    private AudioSource audioSource;
    private Camera mainCam;
    private float scrollOffset;
    private float totalHeight;
    private bool laeuft = false;
    private bool beendet = false;
    private float fadeTimer = 0f;
    private CanvasGroup canvasGroup;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null) return;

        ErstelleSternenfeld();
        ErstelleCredits();
        ErstelleMusik();

        laeuft = true;
        StartCoroutine(FadeInMusik());
    }

    void Update()
    {
        if (!laeuft || beendet) return;

        if (fadeTimer < fadeInDuration)
        {
            fadeTimer += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Clamp01(fadeTimer / fadeInDuration);
        }

        scrollOffset += scrollSpeed * Time.deltaTime;
        if (creditsContainer != null)
            creditsContainer.anchoredPosition = new Vector2(0, scrollOffset);

        Transform flyTarget = xrOrigin != null ? xrOrigin : mainCam.transform;
        flyTarget.position += mainCam.transform.forward * cameraFlySpeed * Time.deltaTime;
        flyTarget.Rotate(Vector3.forward, cameraRotationSpeed * Time.deltaTime, Space.Self);

        if (sternPartikel != null)
            sternPartikel.transform.position = mainCam.transform.position;

        if (creditsCanvas != null)
        {
            creditsCanvas.transform.position = mainCam.transform.position + mainCam.transform.forward * canvasDistance;
            creditsCanvas.transform.rotation = mainCam.transform.rotation;
        }

        if (audioSource != null && abspannMusik != null && !audioSource.isPlaying && fadeTimer > musikFadeIn + 1f)
        {
            beendet = true;
            StartCoroutine(AbspannEnde());
        }
    }

    void ErstelleSternenfeld()
    {
        GameObject obj = new GameObject("Sternenfeld");
        obj.transform.SetParent(transform);
        obj.transform.position = mainCam.transform.position;

        sternPartikel = obj.AddComponent<ParticleSystem>();

        var emission = sternPartikel.emission;
        emission.enabled = false;

        var main = sternPartikel.main;
        main.maxParticles = starCount;
        main.startLifetime = Mathf.Infinity;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(starSizeMin, starSizeMax);
        main.startColor = starColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.loop = false;

        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_Color", starColor);

        var shape = sternPartikel.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = starFieldRadius;

        var noise = sternPartikel.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = twinkleSpeed;
        noise.scrollSpeed = 0.5f;
        noise.sizeAmount = new ParticleSystem.MinMaxCurve(0.4f);

        sternPartikel.Emit(starCount);
    }

    // Credits werden aus dem creditsText-Feld geparst.
    // Zeilen mit # am Anfang = Kategorie (Titel-Font)
    // Erste Zeile = Haupttitel
    // Leerzeilen = Abstand
    // Alles andere = Name (Namen-Font)
    void ErstelleCredits()
    {
        GameObject canvasObj = new GameObject("CreditsCanvas");
        canvasObj.transform.SetParent(transform);

        creditsCanvas = canvasObj.AddComponent<Canvas>();
        creditsCanvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(800, 1200);
        canvasRect.localScale = Vector3.one * 0.003f;

        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        GameObject container = new GameObject("Container");
        container.transform.SetParent(canvasObj.transform, false);

        creditsContainer = container.AddComponent<RectTransform>();
        creditsContainer.anchorMin = new Vector2(0.5f, 0);
        creditsContainer.anchorMax = new Vector2(0.5f, 0);
        creditsContainer.pivot = new Vector2(0.5f, 1);
        creditsContainer.sizeDelta = new Vector2(700, 0);

        float y = 0f;
        string[] zeilen = creditsText.Split('\n');
        bool ersteLinie = true;

        for (int i = 0; i < zeilen.Length; i++)
        {
            string zeile = zeilen[i].Trim();

            if (string.IsNullOrEmpty(zeile))
            {
                y -= 40f;
                continue;
            }

            if (ersteLinie)
            {
                y = ErzeugeText(container.transform, zeile, titelFont, titelFontSize, kategorieColor, y, 80f);
                y -= 30f;
                ersteLinie = false;
            }
            else if (zeile.StartsWith("#"))
            {
                string titel = zeile.Substring(1).Trim();
                y = ErzeugeText(container.transform, titel, titelFont, kategorieFontSize, kategorieColor, y, 45f);
            }
            else
            {
                y = ErzeugeText(container.transform, zeile, namenFont != null ? namenFont : titelFont, namenFontSize, namenColor, y, 35f);
            }
        }

        totalHeight = Mathf.Abs(y);
        creditsContainer.sizeDelta = new Vector2(700, totalHeight);

        // Startposition: Text beginnt knapp unterhalb der Sichtmitte
        scrollOffset = -200f;
        creditsContainer.anchoredPosition = new Vector2(0, scrollOffset);
    }

    float ErzeugeText(Transform parent, string text, TMP_FontAsset font, float size, Color color, float yPos, float height)
    {
        GameObject obj = new GameObject("C_" + text);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, yPos);
        rect.sizeDelta = new Vector2(0, height);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = font;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;

        return yPos - height;
    }

    void ErstelleMusik()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = abspannMusik;
        audioSource.volume = 0f;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;

        if (abspannMusik != null)
            audioSource.Play();
    }

    IEnumerator FadeInMusik()
    {
        float timer = 0f;
        while (timer < musikFadeIn)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, musikLautstaerke, timer / musikFadeIn);
            yield return null;
        }
        audioSource.volume = musikLautstaerke;
    }

    IEnumerator AbspannEnde()
    {
        float timer = 0f;
        while (timer < musikFadeOut)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / musikFadeOut);

            if (canvasGroup != null)
                canvasGroup.alpha = 1f - t;

            audioSource.volume = Mathf.Lerp(musikLautstaerke, 0f, t);
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(mainMenuScene);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, starFieldRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + Vector3.forward * canvasDistance, new Vector3(2.4f, 3.6f, 0.01f));
    }
}
