using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// VR CREDITS ABSPANN (Rewritten)
/// ===============================
/// Robust version that works reliably on Meta Quest.
/// 
/// Features:
/// - Star Wars style scrolling credits in VR
/// - Star particle field background
/// - Camera fly-through with rotation
/// - Music fade in/out
/// - Fade in from black
/// - Auto return to main menu
/// - Works with # markers for category headers
///
/// SETUP:
/// 1. Attach to any GameObject in your credits scene
/// 2. Drag XR Origin into "Xr Origin" slot
/// 3. Set your fonts, colors, text in the Inspector
/// 4. Make sure Main Camera has Tag "MainCamera"
/// </summary>
public class VRCreditsAbspannFixed : MonoBehaviour
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
        "Erik Strothmann\n" +
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
        "Vincent and Salo\n" +
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

    // Private
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

    // ──────────────────────────────────────────
    //  STARTUP — waits for camera to be ready
    // ──────────────────────────────────────────

    IEnumerator Start()
    {
        // Wait until camera is available (fixes Quest timing issue)
        mainCam = null;
        float waitTime = 0f;
        float maxWait = 5f;

        while (mainCam == null && waitTime < maxWait)
        {
            mainCam = Camera.main;

            if (mainCam == null && xrOrigin != null)
                mainCam = xrOrigin.GetComponentInChildren<Camera>();

            if (mainCam == null)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
        }

        if (mainCam == null)
        {
            Debug.LogError("[VRCreditsAbspann] No camera found after waiting! Credits cannot start.");
            yield break;
        }

        Debug.Log("[VRCreditsAbspann] Camera found: " + mainCam.name);

        // Make sure background is black
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = Color.black;

        // Wait for start delay
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        // Create everything
        ErstelleSternenfeld();
        ErstelleCredits();
        ErstelleMusik();

        laeuft = true;
        StartCoroutine(FadeInMusik());
    }

    // ──────────────────────────────────────────
    //  UPDATE — scroll, fade, fly
    // ──────────────────────────────────────────

    void Update()
    {
        if (!laeuft || beendet) return;

        // Fade in credits
        if (fadeTimer < fadeInDuration)
        {
            fadeTimer += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Clamp01(fadeTimer / fadeInDuration);
        }

        // Scroll credits
        scrollOffset += scrollSpeed * Time.deltaTime;
        if (creditsContainer != null)
            creditsContainer.anchoredPosition = new Vector2(0, scrollOffset);

        // Camera fly-through
        Transform flyTarget = xrOrigin != null ? xrOrigin : mainCam.transform;
        flyTarget.position += mainCam.transform.forward * cameraFlySpeed * Time.deltaTime;
        flyTarget.Rotate(Vector3.forward, cameraRotationSpeed * Time.deltaTime, Space.Self);

        // Keep stars around camera
        if (sternPartikel != null)
            sternPartikel.transform.position = mainCam.transform.position;

        // Position canvas in front of camera
        if (creditsCanvas != null)
        {
            creditsCanvas.transform.position = mainCam.transform.position +
                mainCam.transform.forward * canvasDistance;
            creditsCanvas.transform.rotation = mainCam.transform.rotation;
        }

        // Check if credits finished (scrolled past all text)
        if (scrollOffset > totalHeight + 500f)
        {
            if (!beendet)
            {
                beendet = true;
                StartCoroutine(AbspannEnde());
            }
        }

        // Also end when music stops (if music is set)
        if (audioSource != null && abspannMusik != null && !audioSource.isPlaying &&
            fadeTimer > musikFadeIn + 1f)
        {
            if (!beendet)
            {
                beendet = true;
                StartCoroutine(AbspannEnde());
            }
        }
    }

    // ──────────────────────────────────────────
    //  STAR FIELD
    // ──────────────────────────────────────────

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

    // ──────────────────────────────────────────
    //  CREDITS CANVAS
    // ──────────────────────────────────────────

    void ErstelleCredits()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("CreditsCanvas");
        canvasObj.transform.SetParent(transform);

        creditsCanvas = canvasObj.AddComponent<Canvas>();
        creditsCanvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(800, 1200);
        canvasRect.localScale = Vector3.one * 0.003f;

        // Canvas group for fade — START AT 0
        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Container that scrolls
        GameObject container = new GameObject("Container");
        container.transform.SetParent(canvasObj.transform, false);

        creditsContainer = container.AddComponent<RectTransform>();
        creditsContainer.anchorMin = new Vector2(0.5f, 0f);
        creditsContainer.anchorMax = new Vector2(0.5f, 0f);
        creditsContainer.pivot = new Vector2(0.5f, 1f);
        creditsContainer.anchoredPosition = new Vector2(0, -600);
        creditsContainer.sizeDelta = new Vector2(800, 0);

        // Parse credits text and create TMP elements
        string[] lines = creditsText.Split('\n');
        float yPos = 0f;
        bool isFirstLine = true;

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                yPos -= 40f; // spacing for empty lines
                continue;
            }

            GameObject textObj = new GameObject("CreditLine");
            textObj.transform.SetParent(container.transform, false);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0.5f, 1);
            textRect.sizeDelta = new Vector2(0, 0);

            if (isFirstLine)
            {
                // Main title
                tmp.text = line;
                tmp.font = titelFont;
                tmp.fontSize = titelFontSize;
                tmp.color = kategorieColor;
                isFirstLine = false;
                yPos -= 20f;
            }
            else if (line.StartsWith("#"))
            {
                // Category header
                tmp.text = line.Substring(1); // remove #
                tmp.font = titelFont;
                tmp.fontSize = kategorieFontSize;
                tmp.color = kategorieColor;
                yPos -= 30f; // extra space before category
            }
            else
            {
                // Name
                tmp.text = line;
                tmp.font = namenFont;
                tmp.fontSize = namenFontSize;
                tmp.color = namenColor;
            }

            textRect.anchoredPosition = new Vector2(0, yPos);

            // Auto-size height
            tmp.ForceMeshUpdate();
            float textHeight = tmp.preferredHeight;
            textRect.sizeDelta = new Vector2(0, textHeight);

            yPos -= textHeight + 8f;
        }

        totalHeight = Mathf.Abs(yPos) + 600f;

        // Position canvas in front of camera initially
        creditsCanvas.transform.position = mainCam.transform.position +
            mainCam.transform.forward * canvasDistance;
        creditsCanvas.transform.rotation = mainCam.transform.rotation;
    }

    // ──────────────────────────────────────────
    //  MUSIC
    // ──────────────────────────────────────────

    void ErstelleMusik()
    {
        if (abspannMusik == null) return;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = abspannMusik;
        audioSource.volume = 0f;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 2D music
        audioSource.Play();
    }

    IEnumerator FadeInMusik()
    {
        if (audioSource == null) yield break;

        float t = 0f;
        while (t < musikFadeIn)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, musikLautstaerke, t / musikFadeIn);
            yield return null;
        }
        audioSource.volume = musikLautstaerke;
    }

    IEnumerator FadeOutMusik()
    {
        if (audioSource == null) yield break;

        float startVol = audioSource.volume;
        float t = 0f;
        while (t < musikFadeOut)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, t / musikFadeOut);
            yield return null;
        }
        audioSource.volume = 0f;
    }

    // ──────────────────────────────────────────
    //  END — fade out and return to menu
    // ──────────────────────────────────────────

    IEnumerator AbspannEnde()
    {
        // Fade out music
        StartCoroutine(FadeOutMusik());

        // Fade out credits
        float t = 0f;
        float fadeDur = 2f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

        while (t < fadeDur)
        {
            t += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDur);
            yield return null;
        }

        // Wait a moment
        yield return new WaitForSeconds(1f);

        // Return to main menu
        SceneManager.LoadScene(mainMenuScene);
    }
}
