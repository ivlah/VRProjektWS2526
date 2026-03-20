using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class OptionsPanelController : MonoBehaviour
{
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
    [SerializeField] private MainMenuManager mainMenuManager;

    [Header("─── Graphics: Brightness ───────────────────────")]
    [SerializeField] private Slider sliderBrightness;
    [SerializeField] private TextMeshProUGUI labelBrightness;
    [SerializeField] private float brightnessMin = 0.3f;
    [SerializeField] private float brightnessMax = 2.0f;
    private const string PREF_BRIGHTNESS = "ADH_Brightness";

    [Header("─── Audio: Volume ──────────────────────────────")]
    [SerializeField] private Slider sliderVolume;
    [SerializeField] private TextMeshProUGUI labelVolume;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string mixerParam = "MasterVolume";
    private const string PREF_VOLUME = "ADH_Volume";

    [Header("─── Guidance: Scroll Content ───────────────────")]
    [SerializeField] private TextMeshProUGUI textGuidance;

    private void Awake()
    {
        BindTabButtons();
        BindBackButton();
        SetupGraphicsSlider();
        SetupAudioSlider();
        SetupGuidanceText();
        ShowTab(panelGraphics);
    }

    private void OnEnable()
    {
        LoadSettings();
    }

    // ---------------------------------------------------------------
    // PUBLIC — für XR Simple Interactable Events im Inspector
    // ---------------------------------------------------------------

    public void OpenGraphicsTab() => ShowTab(panelGraphics);
    public void OpenAudioTab()    => ShowTab(panelAudio);
    public void OpenGuidanceTab() => ShowTab(panelGuidance);

    public void OnBack()
    {
        if (mainMenuManager != null)
            mainMenuManager.OnCloseOptions();
        else
            Debug.LogError("[Options] mainMenuManager is not assigned!");
    }

    // ---------------------------------------------------------------
    // Tab Navigation
    // ---------------------------------------------------------------

    private void BindTabButtons()
    {
        if (tabGraphics != null)
            tabGraphics.onClick.AddListener(OpenGraphicsTab);
        else
            Debug.LogError("[Options] tabGraphics is not assigned!");

        if (tabAudio != null)
            tabAudio.onClick.AddListener(OpenAudioTab);
        else
            Debug.LogError("[Options] tabAudio is not assigned!");

        if (tabGuidance != null)
            tabGuidance.onClick.AddListener(OpenGuidanceTab);
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
        if (panelAudio    != null) panelAudio.SetActive(false);
        if (panelGuidance != null) panelGuidance.SetActive(false);

        if (activePanel != null)
            activePanel.SetActive(true);
    }

    // ---------------------------------------------------------------
    // Graphics — Brightness
    // ---------------------------------------------------------------

    private void SetupGraphicsSlider()
    {
        if (sliderBrightness == null) return;

        sliderBrightness.minValue = brightnessMin;
        sliderBrightness.maxValue = brightnessMax;

        float saved = PlayerPrefs.GetFloat(PREF_BRIGHTNESS, 1.0f);
        sliderBrightness.value = saved;
        ApplyBrightness(saved);
        UpdateBrightnessLabel(saved);

        sliderBrightness.onValueChanged.AddListener(OnBrightnessChanged);
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
    }

    private void UpdateBrightnessLabel(float value)
    {
        if (labelBrightness == null) return;
        float normalized = Mathf.InverseLerp(brightnessMin, brightnessMax, value);
        labelBrightness.text = $"Brightness: {Mathf.RoundToInt(normalized * 100f)}%";
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
        UpdateVolumeLabel(saved);

        sliderVolume.onValueChanged.AddListener(OnVolumeChanged);
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
        if (audioMixer == null) return;
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        audioMixer.SetFloat(mixerParam, db);
    }

    private void UpdateVolumeLabel(float value)
    {
        if (labelVolume == null) return;
        labelVolume.text = $"Volume: {Mathf.RoundToInt(value * 100f)}%";
    }

    // ---------------------------------------------------------------
    // Guidance Text
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
Grip   (Thumb-Button)            →  Grab Objects
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
- Fully charge the headset before playing.
- Recommended playtime: max. 45 minutes without a break.
- If the lenses fog up: remove the headset briefly and let it air out.
";
    }

    // ---------------------------------------------------------------
    // Settings laden
    // ---------------------------------------------------------------

    private void LoadSettings()
    {
        if (sliderBrightness != null)
        {
            float b = PlayerPrefs.GetFloat(PREF_BRIGHTNESS, 1.0f);
            sliderBrightness.value = b;
            ApplyBrightness(b);
            UpdateBrightnessLabel(b);
        }

        if (sliderVolume != null)
        {
            float v = PlayerPrefs.GetFloat(PREF_VOLUME, 0.8f);
            sliderVolume.value = v;
            ApplyVolume(v);
            UpdateVolumeLabel(v);
        }
    }
}