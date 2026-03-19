using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;

/// <summary>
/// A Doll's House — Subtitle System (v3)
///
/// ════════════════════════════════════════════════════════
/// Untertitel erscheinen IMMER unten im Blickfeld — wie im Kino.
/// Funktioniert in VR ohne XR Camera Referenz.
///
/// HASHTAG SYSTEM:
///   #Wort → wird in Special Font + Farbe angezeigt
///   Beispiel: "I have #loved you" → "loved" in Goldfarbe
///
/// BINDESTRICHE in Text Feld:
///   Trenne Passagen mit — (langer Bindestrich) oder -
///   Die Pausen zwischen Passagen über "Delay Before" einstellen
///
/// ALLES IM INSPECTOR ÄNDERBAR:
///   • Text, Speaker, Timing → direkt im Inspector
///   • Fonts, Farben, Größen → im Inspector
///   • Kein Code anfassen nötig!
/// ════════════════════════════════════════════════════════
/// </summary>
public class SubtitleSystem : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Subtitle Line
    // ---------------------------------------------------------------

    [System.Serializable]
    public class SubtitleLine
    {
        [Tooltip("Pause BEVOR diese Zeile erscheint (Sekunden).")]
        public float delayBefore = 1f;

        [Tooltip("Sprechername — leer lassen für keine Anzeige.")]
        public string speaker = "";

        [TextArea(2, 6)]
        [Tooltip("Text. #Wort = Special Font.\nBindestriche nur zur Übersicht — haben keine Funktion im Text selbst.")]
        public string text = "";

        [Tooltip("Wie lange diese Zeile sichtbar bleibt (Sekunden).")]
        public float duration = 3f;

        [Tooltip("Optional: Audioclip der gleichzeitig spielt.")]
        public AudioClip audio;
    }

    // ---------------------------------------------------------------
    // Inspector — Fonts
    // ---------------------------------------------------------------

    [Header("════ Fonts ════════════════════════")]
    [Tooltip("Normale Schrift für den Untertitel-Text.")]
    [SerializeField] private TMP_FontAsset normalFont;

    [Tooltip("Schrift für #markierte Wörter.")]
    [SerializeField] private TMP_FontAsset specialFont;

    [Tooltip("Schrift für den Sprechernamen.")]
    [SerializeField] private TMP_FontAsset speakerFont;

    // ---------------------------------------------------------------
    // Inspector — Farben & Größen
    // ---------------------------------------------------------------

    [Header("════ Farben ════════════════════════")]
    [SerializeField] private Color normalColor   = Color.white;
    [SerializeField] private Color specialColor  = new Color(1f, 0.85f, 0.30f, 1f);
    [SerializeField] private Color speakerColor  = new Color(1f, 0.80f, 0.40f, 1f);
    [SerializeField] private Color bgColor       = new Color(0f, 0f, 0f, 0.60f);

    [Header("════ Schriftgrößen ══════════════════")]
    [Range(10f, 40f)] [SerializeField] private float textSize   = 20f;
    [Range(8f,  30f)] [SerializeField] private float speakerSize = 16f;

    // ---------------------------------------------------------------
    // Inspector — Untertitel
    // ---------------------------------------------------------------

    [Header("════ Steuerung ══════════════════════")]
    [Tooltip("Automatisch beim Start der Szene abspielen.")]
    [SerializeField] private bool playOnStart = true;

    [Header("════ Untertitel Zeilen ═══════════════")]
    [SerializeField] private SubtitleLine[] subtitles = new SubtitleLine[]
    {
        // ── Alle Zeilen aus dem Text — Timing anpassbar ──────────
        new SubtitleLine { delayBefore=2f,   speaker="Nora",    text="Ah!",                                                                                                          duration=1.5f },
        new SubtitleLine { delayBefore=0.5f, speaker="Nora",    text="Yes, I know. Let me go! Let me get out!",                                                                      duration=3.5f },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="It is true. I have #loved you above everything else in the world.",                                            duration=4f   },
        new SubtitleLine { delayBefore=0.5f, speaker="Nora",    text="#Torvald!",                                                                                                    duration=1.5f },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="Let me go. You shall not suffer for my sake.",                                                                 duration=3.5f },
        new SubtitleLine { delayBefore=1f,   speaker="Torvald", text="Yes, now I am beginning to understand thoroughly.",                                                            duration=3.5f },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="Let me go. You shall not suffer for my sake.",                                                                 duration=3.5f },
        new SubtitleLine { delayBefore=0.8f, speaker="Nora",    text="I will go, it is the best for us.",                                                                            duration=3f   },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="I am #leaving!",                                                                                              duration=2f   },
        new SubtitleLine { delayBefore=1.5f, speaker="Nora",    text="You have never loved me. You have only thought it pleasant to be in love with me.",                            duration=5f   },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="I have been your doll-wife, just as at home I was papa's doll-child;\nand here the children have been my dolls.",duration=6f },
        new SubtitleLine { delayBefore=0.5f, speaker="Nora",    text="I thought it great fun when you played with me, just as they thought it great fun when I played with them.\nThat is what our marriage has been, Torvald.", duration=7f },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="Whose lessons? Mine, or the children's?",                                                                      duration=3.5f },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="Alas, Torvald, you are not the man to educate me into being a proper wife for you.",                           duration=4.5f },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="I must #stand quite alone.",                                                                                   duration=3f   },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="I am going away from here now, at once.",                                                                      duration=3.5f },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="It is no use forbidding me anything any longer.\nI will take with me what belongs to #myself.",                duration=5f   },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="I must try and get some sense, Torvald.",                                                                      duration=3.5f },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="I only know that it is necessary for me.",                                                                     duration=3f   },
        new SubtitleLine { delayBefore=1f,   speaker="Nora",    text="My most #sacred #duties?",                                                                                    duration=3f   },
        new SubtitleLine { delayBefore=0.8f, speaker="Nora",    text="I have other duties just as sacred.",                                                                          duration=3f   },
        new SubtitleLine { delayBefore=1.5f, speaker="Nora",    text="#Good #bye, Torvald.",                                                                                         duration=4f   },
    };

    // ---------------------------------------------------------------
    // Private
    // ---------------------------------------------------------------

    private Canvas              uiCanvas;
    private Image               bgImage;
    private TextMeshProUGUI     speakerTMP;
    private TextMeshProUGUI     subtitleTMP;
    private AudioSource         audioSrc;

    // ---------------------------------------------------------------
    // Start
    // ---------------------------------------------------------------

    private void Start()
    {
        BuildUI();
        if (playOnStart && subtitles != null && subtitles.Length > 0)
            StartCoroutine(PlaySequence());
    }

    // ---------------------------------------------------------------
    // UI — Screen Space Overlay (immer unten im Bild)
    // ---------------------------------------------------------------

    private void BuildUI()
    {
        GameObject cGO = new GameObject("SubtitleCanvas");
        cGO.transform.SetParent(transform, false);

        uiCanvas = cGO.AddComponent<Canvas>();
        uiCanvas.renderMode   = RenderMode.ScreenSpaceCamera;
        uiCanvas.worldCamera  = Camera.main;
        uiCanvas.planeDistance = 1f;
        uiCanvas.sortingOrder = 100;

        CanvasScaler scaler         = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight   = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // Hintergrund — unten zentriert
        GameObject bgGO  = new GameObject("BG");
        bgGO.transform.SetParent(cGO.transform, false);
        bgImage          = bgGO.AddComponent<Image>();
        bgImage.color    = bgColor;
        RectTransform bgRt = bgGO.GetComponent<RectTransform>();
        bgRt.anchorMin   = new Vector2(0.1f, 0f);
        bgRt.anchorMax   = new Vector2(0.9f, 0.18f);
        bgRt.offsetMin   = new Vector2(0f, 10f);
        bgRt.offsetMax   = new Vector2(0f, -10f);

        // Sprechername
        GameObject spkGO  = new GameObject("Speaker");
        spkGO.transform.SetParent(cGO.transform, false);
        speakerTMP        = spkGO.AddComponent<TextMeshProUGUI>();
        speakerTMP.fontSize  = speakerSize;
        speakerTMP.color     = speakerColor;
        speakerTMP.fontStyle = FontStyles.Bold;
        speakerTMP.alignment = TextAlignmentOptions.Center;
        speakerTMP.text      = "";
        if (speakerFont != null) speakerTMP.font = speakerFont;
        RectTransform spkRt  = spkGO.GetComponent<RectTransform>();
        spkRt.anchorMin      = new Vector2(0.1f, 0.10f);
        spkRt.anchorMax      = new Vector2(0.9f, 0.16f);
        spkRt.offsetMin      = spkRt.offsetMax = Vector2.zero;

        // Subtitle Text
        GameObject txtGO  = new GameObject("Text");
        txtGO.transform.SetParent(cGO.transform, false);
        subtitleTMP        = txtGO.AddComponent<TextMeshProUGUI>();
        subtitleTMP.fontSize          = textSize;
        subtitleTMP.color             = normalColor;
        subtitleTMP.alignment         = TextAlignmentOptions.Center;
        subtitleTMP.enableWordWrapping = true;
        subtitleTMP.text              = "";
        if (normalFont != null) subtitleTMP.font = normalFont;
        RectTransform txtRt = txtGO.GetComponent<RectTransform>();
        txtRt.anchorMin     = new Vector2(0.1f, 0.01f);
        txtRt.anchorMax     = new Vector2(0.9f, 0.10f);
        txtRt.offsetMin     = txtRt.offsetMax = Vector2.zero;

        // Audio
        audioSrc = cGO.AddComponent<AudioSource>();
        audioSrc.spatialBlend = 0f;
        audioSrc.playOnAwake  = false;

        SetVisible(false);
    }

    // ---------------------------------------------------------------
    // Hashtag Parser
    // ---------------------------------------------------------------

    private string ParseHashtags(string input)
    {
        string hex = ColorUtility.ToHtmlStringRGB(specialColor);

        if (specialFont != null)
        {
            return Regex.Replace(input, @"#(\S+)",
                m => $"<color=#{hex}><font=\"{specialFont.name}\">{m.Groups[1].Value}</font></color>");
        }
        else
        {
            return Regex.Replace(input, @"#(\S+)",
                m => $"<color=#{hex}>{m.Groups[1].Value}</color>");
        }
    }

    // ---------------------------------------------------------------
    // Sequence
    // ---------------------------------------------------------------

    private IEnumerator PlaySequence()
    {
        foreach (SubtitleLine line in subtitles)
        {
            yield return new WaitForSeconds(line.delayBefore);

            speakerTMP.text  = string.IsNullOrEmpty(line.speaker) ? "" : $"{line.speaker}:";
            subtitleTMP.text = ParseHashtags(line.text);

            if (line.audio != null)
            {
                audioSrc.clip = line.audio;
                audioSrc.Play();
            }

            SetVisible(true);
            yield return StartCoroutine(Fade(0f, 1f, 0.2f));
            yield return new WaitForSeconds(line.duration);
            yield return StartCoroutine(Fade(1f, 0f, 0.2f));
            SetVisible(false);
        }
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    private IEnumerator Fade(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / dur);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        if (subtitleTMP != null) { var c = subtitleTMP.color; c.a = a; subtitleTMP.color = c; }
        if (speakerTMP  != null) { var c = speakerTMP.color;  c.a = a; speakerTMP.color  = c; }
        if (bgImage     != null) { var c = bgImage.color; c.a = Mathf.Min(a, bgColor.a); bgImage.color = c; }
    }

    private void SetVisible(bool v)
    {
        if (uiCanvas != null) uiCanvas.gameObject.SetActive(v);
    }

    // ---------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------

    public void RestartSequence()
    {
        StopAllCoroutines();
        StartCoroutine(PlaySequence());
    }

    public void StopSubtitles()
    {
        StopAllCoroutines();
        SetVisible(false);
    }
}
