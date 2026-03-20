using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

/// <summary>
/// A Doll's House — Options Panel Controller
///
/// Three tabs:
///   1. Graphics   → Brightness / Gamma
///   2. Audio      → Master Volume
///   3. Guidance   → Meta Quest 3 user instructions
///
/// Setup:
///   • Attach this script to the OptionsPanel root GameObject
///   • Assign all fields in the Inspector (sliders, tabs, text fields)
///   • Connect an AudioMixer with exposed parameter "MasterVolume"
///   • Post-processing / gamma control is optional
/// </summary>
public class OptionsPanelController : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Inspector — Navigation
    // ---------------------------------------------------------------

    [Header("─── Tab Buttons ────────────────────────────────")]
    [SerializeField] private Button tabGraphics;
    [SerializeField] private Button tabAudio;
    [SerializeField] private Button tabGuidance;
    [SerializeField] private Button btnBack;

    [Header("─── Tab Panels ─────────────────────────────────")]
    [SerializeField] private GameObject panelGraphics;
    [SerializeField] private GameObject panelAudio;
    [SerializeField] private GameObject panelGuidance;

    [Header("─── Main Menu Reference ────────────────────────")]
    [Tooltip("MainMenuManager reference used to call OnCloseOptions().")]
    [SerializeField] private MainMenuManager mainMenuManager;

    // ---------------------------------------------------------------
    // Inspector — Graphics Tab
    // ---------------------------------------------------------------

    [Header("─── Graphics: Brightness ───────────────────────")]
    [SerializeField] private Slider sliderBrightness;
    [SerializeField] private TextMeshProUGUI labelBrightness;

    [Tooltip("Minimum brightness/gamma value (0.3 = very dark).")]
    [SerializeField] private float brightnessMin = 0.3f;

    [Tooltip("Maximum brightness/gamma value (2.0 = very bright).")]
    [SerializeField] private float brightnessMax = 2.0f;

    private const string PREF_BRIGHTNESS = "ADH_Brightness";

    // ---------------------------------------------------------------
    // Inspector — Audio Tab
    // ---------------------------------------------------------------

    [Header("─── Audio: Volume ──────────────────────────────")]
    [SerializeField] private Slider sliderVolume;
    [SerializeField] private TextMeshProUGUI labelVolume;

    [Tooltip("AudioMixer with exposed parameter 'MasterVolume'.")]
    [SerializeField] private AudioMixer audioMixer;

    [Tooltip("Name of the exposed AudioMixer parameter.")]
    [SerializeField] private string mixerParam = "MasterVolume";

    private const string PREF_VOLUME = "ADH_Volume";

    // ---------------------------------------------------------------
    // Inspector — Guidance Tab
    // ---------------------------------------------------------------

    [Header("─── Guidance: Scroll Content ───────────────────")]
    [SerializeField] private TextMeshProUGUI textGuidance;

    // ---------------------------------------------------------------
    // Unity Lifecycle
    // ---------------------------------------------------------------

    private void Awake()
    {
        BindTabButtons();
        BindBackButton();
        SetupGraphicsSlider();
        SetupAudioSlider();
        SetupGuidanceText();

        // Open the Graphics tab by default
        ShowTab(panelGraphics);
    }

    private void OnEnable()
    {
        // Load saved values whenever the options panel is opened
        LoadSettings();
    }

    // ---------------------------------------------------------------
    // Tab Navigation
    // ---------------------------------------------------------------

    private void BindTabButtons()
    {
        if (tabGraphics != null)
            tabGraphics.onClick.AddListener(() => ShowTab(panelGraphics));
        else
            Debug.LogError("[Options] tabGraphics is not assigned!");

        if (tabAudio != null)
            tabAudio.onClick.AddListener(() => ShowTab(panelAudio));
        else
            Debug.LogError("[Options] tabAudio is not assigned!");

        if (tabGuidance != null)
            tabGuidance.onClick.AddListener(() => ShowTab(panelGuidance));
        else
            Debug.LogError("[Options] tabGuidance is not assigned!");
    }

    private void BindBackButton()
    {
        if (btnBack != null)
            btnBack.onClick.AddListener(OnBack);
        else
            Debug.LogError("[Options] btnBack is not assigned!");
    }

    private void ShowTab(GameObject activePanel)
    {
        if (panelGraphics != null) panelGraphics.SetActive(false);
        if (panelAudio != null) panelAudio.SetActive(false);
        if (panelGuidance != null) panelGuidance.SetActive(false);

        if (activePanel != null)
            activePanel.SetActive(true);
    }

    // ---------------------------------------------------------------
    // Graphics — Brightness / Gamma
    // ---------------------------------------------------------------

    private void SetupGraphicsSlider()
    {
        if (sliderBrightness == null) return;

        sliderBrightness.minValue = brightnessMin;
        sliderBrightness.maxValue = brightnessMax;

        float saved = PlayerPrefs.GetFloat(PREF_BRIGHTNESS, 1.0f);
        sliderBrightness.value = saved;
        ApplyBrightness(saved);

        sliderBrightness.onValueChanged.AddListener(OnBrightnessChanged);
        UpdateBrightnessLabel(saved);
    }

    private void OnBrightnessChanged(float value)
    {
        ApplyBrightness(value);
        UpdateBrightnessLabel(value);
        PlayerPrefs.SetFloat(PREF_BRIGHTNESS, value);
        PlayerPrefs.Save();
    }

    private void ApplyBrightness(float value)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Screen.brightness = Mathf.InverseLerp(brightnessMin, brightnessMax, value);
#endif

        Shader.SetGlobalFloat("_Gamma", value);

        // Optional:
        // If you use post-processing, you can hook your brightness exposure here.
    }

    private void UpdateBrightnessLabel(float value)
    {
        if (labelBrightness != null)
        {
            float normalized = Mathf.InverseLerp(brightnessMin, brightnessMax, value);
            labelBrightness.text = $"Brightness: {Mathf.RoundToInt(normalized * 100f)}%";
        }
    }

    // ---------------------------------------------------------------
    // Audio — Volume
    // ---------------------------------------------------------------

    private void SetupAudioSlider()
    {
        if (sliderVolume == null) return;

        sliderVolume.minValue = 0.0001f;
        sliderVolume.maxValue = 1f;

        float saved = PlayerPrefs.GetFloat(PREF_VOLUME, 0.8f);
        sliderVolume.value = saved;
        ApplyVolume(saved);

        sliderVolume.onValueChanged.AddListener(OnVolumeChanged);
        UpdateVolumeLabel(saved);
    }

    private void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        UpdateVolumeLabel(value);
        PlayerPrefs.SetFloat(PREF_VOLUME, value);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(float value)
    {
        if (audioMixer != null)
        {
            float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
            audioMixer.SetFloat(mixerParam, db);
        }
        else
        {
            AudioListener.volume = value;
        }
    }

    private void UpdateVolumeLabel(float value)
    {
        if (labelVolume != null)
            labelVolume.text = $"Volume: {Mathf.RoundToInt(value * 100f)}%";
    }

    // ---------------------------------------------------------------
    // Guidance — Meta Quest 3 User Guide
    // ---------------------------------------------------------------

    private void SetupGuidanceText()
    {
        if (textGuidance == null) return;

        textGuidance.text =
@"<b>Meta Quest 3 — User Guide</b>

<b>Controllers</b>
Right Controller   →  Interact / Select
Left Controller    →  Open Menu / Back

<b>Movement in Game</b>
Left Stick         →  Move / Walk
Right Stick        →  Look Around / Turn
Grip    (the button at your thumb)           →  Grab Objects
Trigger            →  Select / Point

<b>VR Comfort</b>
If you feel discomfort, take a break.
Keep your play area clear of obstacles.
Adjust the IPD (interpupillary distance) on the headset:
→ Use the slider on the underside of the headset.

<b>Guardian Boundary</b>
Set up the Guardian boundary when starting for the first time.
This protects you from leaving your play area.

<b>Hand Tracking (optional)</b>
Put down the controllers to enable hand tracking.
Point gesture → Select
Thumb + index finger together → Click

<b>Pause Game</b>
Quest button (left controller) → System menu

<b>Tips</b>
• Fully charge the headset before playing.
• Recommended playtime: max. 45 minutes without a break.
• If the lenses fog up: remove the headset briefly and let it air out.";
    }

    // ---------------------------------------------------------------
    // Load Settings
    // ---------------------------------------------------------------

    private void LoadSettings()
    {
        if (sliderBrightness != null)
        {
            float brightness = PlayerPrefs.GetFloat(PREF_BRIGHTNESS, 1.0f);
            sliderBrightness.value = brightness;
            ApplyBrightness(brightness);
            UpdateBrightnessLabel(brightness);
        }

        if (sliderVolume != null)
        {
            float volume = PlayerPrefs.GetFloat(PREF_VOLUME, 0.8f);
            sliderVolume.value = volume;
            ApplyVolume(volume);
            UpdateVolumeLabel(volume);
        }
    }

    // ---------------------------------------------------------------
    // Back Button
    // ---------------------------------------------------------------

    private void OnBack()
    {
        if (mainMenuManager != null)
            mainMenuManager.OnCloseOptions();
        else
            gameObject.SetActive(false);
    }
}