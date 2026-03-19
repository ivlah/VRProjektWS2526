using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;

/// <summary>
/// NORA SUBTITLE SYSTEM
/// ====================
/// Cinematic bottom-screen subtitles like TV series.
/// 
/// SETUP:
/// 1. Canvas → Screen Space - Overlay
/// 2. TextMeshProUGUI child → Anchor Bottom-Center, Pos Y ~60px,
///    Width ~80% of screen, Alignment Center, Rich Text ON
/// 3. Optional: Semi-transparent dark Image behind text → drag into Background Panel
/// 4. Attach this script, drag TMP element into "Subtitle Text"
/// 5. Drag your two TMP Font Assets into "Normal Font" and "Highlight Font"
/// 6. Adjust startTime and duration per line in the Inspector
/// 7. Call PlaySubtitles() or enable Auto Play
/// </summary>
public class SubtitleSystem : MonoBehaviour
{
    [Header("═══ UI ═══")]
    [Tooltip("Your TextMeshProUGUI element at the bottom of the screen")]
    public TextMeshProUGUI subtitleText;

    [Tooltip("Optional: semi-transparent dark panel behind text")]
    public UnityEngine.UI.Image backgroundPanel;

    [Header("═══ Fonts ═══")]
    [Tooltip("Font for normal text — drag your TMP Font Asset here")]
    public TMP_FontAsset normalFont;

    [Tooltip("Font for #highlighted words — drag your TMP Font Asset here")]
    public TMP_FontAsset highlightFont;

    [Header("═══ Colors ═══")]
    public Color normalColor = Color.white;

    [Tooltip("Color for #marked words")]
    public Color highlightColor = new Color(0.95f, 0.1f, 0.1f, 1f);

    [Header("═══ Highlight Style ═══")]
    public bool highlightBold = true;
    public bool highlightItalic = false;

    [Header("═══ Transitions ═══")]
    [Range(0f, 1f)] public float fadeInDuration = 0.2f;
    [Range(0f, 1f)] public float fadeOutDuration = 0.2f;

    [Header("═══ Playback ═══")]
    public bool autoPlay = false;

    [Header("═══ Subtitle Lines ═══")]
    [Tooltip("Nora's lines — adjust startTime and duration per entry")]
    public List<SubtitleEntry> entries = new List<SubtitleEntry>();

    private Coroutine _playback;
    private CanvasGroup _canvasGroup;

    // ──────────────────────────────────────────

    [Serializable]
    public class SubtitleEntry
    {
        [Tooltip("Label for your overview (not displayed in game)")]
        public string label = "";

        [TextArea(2, 5)]
        [Tooltip("Subtitle text. Prefix words with # to highlight them.")]
        public string text = "";

        [Tooltip("When this line appears (seconds from sequence start)")]
        public float startTime = 0f;

        [Tooltip("How long this line stays visible (seconds)")]
        public float duration = 3f;
    }

    // ──────────────────────────────────────────
    //  LIFECYCLE
    // ──────────────────────────────────────────

    private void Awake()
    {
        if (subtitleText != null)
        {
            _canvasGroup = subtitleText.GetComponentInParent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = subtitleText.gameObject.AddComponent<CanvasGroup>();
        }

        if (subtitleText != null && normalFont != null)
            subtitleText.font = normalFont;
    }

    private void Start()
    {
        Hide();
        if (autoPlay) PlaySubtitles();
    }

    // ──────────────────────────────────────────
    //  PUBLIC API
    // ──────────────────────────────────────────

    public void PlaySubtitles()
    {
        StopSubtitles();
        _playback = StartCoroutine(RunSequence());
    }

    public void StopSubtitles()
    {
        if (_playback != null)
        {
            StopAllCoroutines();
            _playback = null;
        }
        Hide();
    }

    public void ShowLine(string text, float duration)
    {
        StartCoroutine(DisplayLine(text, duration));
    }

    // ──────────────────────────────────────────
    //  SEQUENCE
    // ──────────────────────────────────────────

    private IEnumerator RunSequence()
    {
        if (entries.Count == 0) yield break;

        List<SubtitleEntry> sorted = new List<SubtitleEntry>(entries);
        sorted.Sort((a, b) => a.startTime.CompareTo(b.startTime));

        float clock = 0f;
        int idx = 0;

        while (idx < sorted.Count)
        {
            if (clock >= sorted[idx].startTime)
            {
                StartCoroutine(DisplayLine(sorted[idx].text, sorted[idx].duration));
                idx++;
            }
            clock += Time.deltaTime;
            yield return null;
        }

        SubtitleEntry last = sorted[sorted.Count - 1];
        float wait = (last.startTime + last.duration + fadeOutDuration) - clock;
        if (wait > 0f) yield return new WaitForSeconds(wait);

        _playback = null;
    }

    // ──────────────────────────────────────────
    //  DISPLAY
    // ──────────────────────────────────────────

    private IEnumerator DisplayLine(string rawText, float duration)
    {
        subtitleText.text = FormatText(rawText);
        subtitleText.gameObject.SetActive(true);
        if (backgroundPanel != null) backgroundPanel.gameObject.SetActive(true);

        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSeconds(duration);
        yield return Fade(1f, 0f, fadeOutDuration);

        Hide();
    }

    // ──────────────────────────────────────────
    //  TEXT FORMATTING — #word → highlight font + color
    // ──────────────────────────────────────────

    private string FormatText(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";

        string hHex = ColorUtility.ToHtmlStringRGB(highlightColor);
        string nHex = ColorUtility.ToHtmlStringRGB(normalColor);

        string result = Regex.Replace(raw, @"#(\w[\w'-]*)", match =>
        {
            string word = match.Groups[1].Value;
            string open = "";
            string close = "";

            if (highlightFont != null)
            {
                open += $"<font=\"{highlightFont.name}\">";
                close = "</font>" + close;
            }

            open += $"<color=#{hHex}>";
            close = "</color>" + close;

            if (highlightBold)   { open += "<b>"; close = "</b>" + close; }
            if (highlightItalic) { open += "<i>"; close = "</i>" + close; }

            return open + word + close;
        });

        return $"<color=#{nHex}>{result}</color>";
    }

    // ──────────────────────────────────────────
    //  FADE
    // ──────────────────────────────────────────

    private IEnumerator Fade(float from, float to, float dur)
    {
        if (_canvasGroup == null) yield break;
        if (dur <= 0f) { _canvasGroup.alpha = to; yield break; }

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        _canvasGroup.alpha = to;
    }

    private void Hide()
    {
        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.gameObject.SetActive(false);
        }
        if (backgroundPanel != null)
            backgroundPanel.gameObject.SetActive(false);
        if (_canvasGroup != null)
            _canvasGroup.alpha = 0f;
    }

    // ──────────────────────────────────────────
    //  PRE-FILLED LINES — Reset() runs when component is first added
    // ──────────────────────────────────────────

    private void Reset()
    {
        entries = new List<SubtitleEntry>
        {
            L(01, "Ah!"),
            L(02, "Yes, I know. Let me go! Let me get out!"),
            L(03, "It is true. I have #loved you above everything else in the world."),
            L(04, "#Torvald!"),
            L(05, "Let me go. You shall not suffer for my sake."),
            L(06, "Yes, now I am beginning to understand thoroughly."),
            L(07, "I will go, it is the best for us."),
            L(08, "I am #leaving!"),
            L(09, "You have never loved me. You have only thought it pleasant to be in love with me."),
            L(10, "I have been your doll-wife, just as at home I was papa's doll-child; and here the children have been my dolls."),
            L(11, "I thought it great fun when you played with me, just as they thought it great fun when I played with them. That is what our marriage has been, Torvald."),
            L(12, "Whose lessons? Mine, or the children's?"),
            L(13, "Alas, Torvald, you are not the man to educate me into being a proper wife for you."),
            L(14, "I must #stand quite alone."),
            L(15, "I am going away from here now, at once."),
            L(16, "It is no use forbidding me anything any longer. I will take with me what belongs to #myself."),
            L(17, "I must try and get some sense, Torvald."),
            L(18, "I only know that it is necessary for me."),
            L(19, "My most #sacred #duties?"),
            L(20, "I have other duties just as sacred."),
            L(21, "#Good #bye, Torvald."),
        };
    }

    private SubtitleEntry L(int num, string text)
    {
        return new SubtitleEntry
        {
            label = $"Line {num:D2}",
            text = text,
            startTime = 0f,
            duration = 3f,
        };
    }

#if UNITY_EDITOR
    [ContextMenu("▶ Play Subtitles")]
    private void EditorPlay() => PlaySubtitles();

    [ContextMenu("■ Stop Subtitles")]
    private void EditorStop() => StopSubtitles();

    [ContextMenu("↻ Reset Lines to Default")]
    private void EditorResetLines() => Reset();
#endif
}
