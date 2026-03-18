using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

/// <summary>
/// A Doll's House — Options Panel Controller
///
/// Drei Tabs:
///   1. Grafik     → Helligkeit / Gamma
///   2. Audio      → Master-Lautstärke
///   3. Anleitung  → Meta Quest 3 Bedienungshinweise
///
/// Setup:
///   • Dieses Script an das OptionsPanel Root GameObject hängen
///   • Alle Felder im Inspector zuweisen (Sliders, Tabs, TextFields)
///   • AudioMixer mit Exposed Parameter "MasterVolume" verbinden
///   • PostProcessing Volume für Gamma/Helligkeit optional
/// </summary>
public class OptionsPanelController : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Inspector — Navigation
    // ---------------------------------------------------------------

    [Header("─── Tab Buttons ────────────────────────────────")]
    [SerializeField] private Button tabGrafik;
    [SerializeField] private Button tabAudio;
    [SerializeField] private Button tabAnleitung;
    [SerializeField] private Button btnBack;

    [Header("─── Tab Panels ─────────────────────────────────")]
    [SerializeField] private GameObject panelGrafik;
    [SerializeField] private GameObject panelAudio;
    [SerializeField] private GameObject panelAnleitung;

    [Header("─── Hauptmenü Referenz ──────────────────────────")]
    [Tooltip("MainMenuManager um OnCloseOptions aufzurufen.")]
    [SerializeField] private MainMenuManager mainMenuManager;

    // ---------------------------------------------------------------
    // Inspector — Grafik Tab
    // ---------------------------------------------------------------

    [Header("─── Grafik: Helligkeit ─────────────────────────")]
    [SerializeField] private Slider sliderHelligkeit;
    [SerializeField] private TextMeshProUGUI labelHelligkeit;

    [Tooltip("Minimale Gamma-Helligkeit (0.3 = sehr dunkel).")]
    [SerializeField] private float helligkeitMin = 0.3f;
    [Tooltip("Maximale Gamma-Helligkeit (2.0 = sehr hell).")]
    [SerializeField] private float helligkeitMax = 2.0f;

    private const string PREF_HELLIGKEIT = "ADH_Helligkeit";

    // ---------------------------------------------------------------
    // Inspector — Audio Tab
    // ---------------------------------------------------------------

    [Header("─── Audio: Lautstärke ──────────────────────────")]
    [SerializeField] private Slider sliderLautstaerke;
    [SerializeField] private TextMeshProUGUI labelLautstaerke;

    [Tooltip("AudioMixer mit Exposed Parameter 'MasterVolume'.")]
    [SerializeField] private AudioMixer audioMixer;
    [Tooltip("Name des Exposed Parameters im AudioMixer.")]
    [SerializeField] private string mixerParam = "MasterVolume";

    private const string PREF_LAUTSTAERKE = "ADH_Lautstaerke";

    // ---------------------------------------------------------------
    // Inspector — Anleitung Tab
    // ---------------------------------------------------------------

    [Header("─── Anleitung: Scroll Content ──────────────────")]
    [SerializeField] private TextMeshProUGUI textAnleitung;

    // ---------------------------------------------------------------
    // Unity Lifecycle
    // ---------------------------------------------------------------

    private void Awake()
    {
        BindTabButtons();
        BindBackButton();
        SetupGrafikSlider();
        SetupAudioSlider();
        SetupAnleitungText();

        // Standardmäßig Grafik-Tab öffnen
        ShowTab(panelGrafik);
    }

    private void OnEnable()
    {
        // Gespeicherte Werte beim Öffnen laden
        LoadSettings();
    }

    // ---------------------------------------------------------------
    // Tab Navigation
    // ---------------------------------------------------------------

    private void BindTabButtons()
    {
        if (tabGrafik    != null) tabGrafik.onClick.AddListener(()    => ShowTab(panelGrafik));
        if (tabAudio     != null) tabAudio.onClick.AddListener(()     => ShowTab(panelAudio));
        if (tabAnleitung != null) tabAnleitung.onClick.AddListener(() => ShowTab(panelAnleitung));
    }

    private void BindBackButton()
    {
        if (btnBack != null)
            btnBack.onClick.AddListener(OnBack);
        else
            Debug.LogError("[Options] btnBack nicht zugewiesen!");
    }

    private void ShowTab(GameObject activePanel)
    {
        if (panelGrafik    != null) panelGrafik.SetActive(false);
        if (panelAudio     != null) panelAudio.SetActive(false);
        if (panelAnleitung != null) panelAnleitung.SetActive(false);

        if (activePanel != null)
            activePanel.SetActive(true);
    }

    // ---------------------------------------------------------------
    // Grafik — Helligkeit / Gamma
    // ---------------------------------------------------------------

    private void SetupGrafikSlider()
    {
        if (sliderHelligkeit == null) return;

        sliderHelligkeit.minValue = helligkeitMin;
        sliderHelligkeit.maxValue = helligkeitMax;

        float saved = PlayerPrefs.GetFloat(PREF_HELLIGKEIT, 1.0f);
        sliderHelligkeit.value = saved;
        ApplyHelligkeit(saved);

        sliderHelligkeit.onValueChanged.AddListener(OnHelligkeitChanged);
        UpdateHelligkeitLabel(saved);
    }

    private void OnHelligkeitChanged(float value)
    {
        ApplyHelligkeit(value);
        UpdateHelligkeitLabel(value);
        PlayerPrefs.SetFloat(PREF_HELLIGKEIT, value);
        PlayerPrefs.Save();
    }

    private void ApplyHelligkeit(float value)
    {
        // Methode 1: Unity Screen Brightness (nur Android/Quest)
#if UNITY_ANDROID && !UNITY_EDITOR
        Screen.brightness = Mathf.InverseLerp(helligkeitMin, helligkeitMax, value);
#endif

        // Methode 2: Gamma über Shader Global (funktioniert überall)
        // Setzt den globalen "_Gamma" Wert den du in custom Shadern nutzen kannst.
        Shader.SetGlobalFloat("_Gamma", value);

        // Methode 3: Falls du Unity Post Processing (URP) nutzt:
        // Uncomment wenn PostProcessingVolume vorhanden:
        // if (colorAdjustments != null)
        //     colorAdjustments.postExposure.value = Mathf.Lerp(-2f, 2f,
        //         Mathf.InverseLerp(helligkeitMin, helligkeitMax, value));
    }

    private void UpdateHelligkeitLabel(float value)
    {
        if (labelHelligkeit != null)
            labelHelligkeit.text = $"Helligkeit: {Mathf.RoundToInt(value * 100f / helligkeitMax)}%";
    }

    // ---------------------------------------------------------------
    // Audio — Lautstärke
    // ---------------------------------------------------------------

    private void SetupAudioSlider()
    {
        if (sliderLautstaerke == null) return;

        sliderLautstaerke.minValue = 0.0001f;  // 0 würde log10 kaputt machen
        sliderLautstaerke.maxValue = 1f;

        float saved = PlayerPrefs.GetFloat(PREF_LAUTSTAERKE, 0.8f);
        sliderLautstaerke.value = saved;
        ApplyLautstaerke(saved);

        sliderLautstaerke.onValueChanged.AddListener(OnLautstaerkeChanged);
        UpdateLautstaerkeLabel(saved);
    }

    private void OnLautstaerkeChanged(float value)
    {
        ApplyLautstaerke(value);
        UpdateLautstaerkeLabel(value);
        PlayerPrefs.SetFloat(PREF_LAUTSTAERKE, value);
        PlayerPrefs.Save();
    }

    private void ApplyLautstaerke(float value)
    {
        if (audioMixer != null)
        {
            // AudioMixer erwartet Dezibel: 0..1 → -80..0 dB
            float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
            audioMixer.SetFloat(mixerParam, db);
        }
        else
        {
            // Fallback ohne AudioMixer
            AudioListener.volume = value;
        }
    }

    private void UpdateLautstaerkeLabel(float value)
    {
        if (labelLautstaerke != null)
            labelLautstaerke.text = $"Lautstärke: {Mathf.RoundToInt(value * 100f)}%";
    }

    // ---------------------------------------------------------------
    // Anleitung — Meta Quest 3 Bedienung
    // ---------------------------------------------------------------

    private void SetupAnleitungText()
    {
        if (textAnleitung == null) return;

        textAnleitung.text =
@"<b>Meta Quest 3 — Bedienungsanleitung</b>

<b>Controller</b>
  Rechter Controller  →  Interagieren / Auswählen
  Linker  Controller  →  Menü öffnen / Zurück

<b>Bewegung im Spiel</b>
  Linker Stick         →  Laufen / Bewegen
  Rechter Stick        →  Umsehen / Drehen
  A-Taste              →  Interagieren
  B-Taste              →  Zurück / Abbrechen
  Grip (Griff)         →  Gegenstände greifen
  Trigger              →  Auswählen / Zeigen

<b>VR Komfort</b>
  Bei Unwohlsein eine Pause einlegen.
  Spielbereich frei von Hindernissen halten.
  IPD (Augenabstand) am Headset einstellen:
    → Schieberegler an der Unterseite des Headsets.

<b>Guardian Boundary</b>
  Beim ersten Start die Guardian-Grenze einrichten.
  Diese schützt dich vor dem Verlassen des Spielbereichs.

<b>Hand Tracking (optional)</b>
  Controller ablegen um Hand Tracking zu aktivieren.
  Zeigegeste → Auswählen
  Daumen + Zeigefinger zusammen → Klicken

<b>Passthrough</b>
  Doppelklick auf den Quest-Button (links am Headset)
  aktiviert den Passthrough-Modus — du siehst die echte Welt.

<b>Spiel pausieren</b>
  Quest-Button (linker Controller) → Systemmenü

<b>Tipps</b>
  • Headset-Akku vor dem Spielen vollständig aufladen.
  • Empfohlene Spielzeit: max. 45 Minuten ohne Pause.
  • Bei Beschlagen der Linsen: kurz Headset ablegen und lüften.";
    }

    // ---------------------------------------------------------------
    // Einstellungen laden
    // ---------------------------------------------------------------

    private void LoadSettings()
    {
        if (sliderHelligkeit != null)
        {
            float h = PlayerPrefs.GetFloat(PREF_HELLIGKEIT, 1.0f);
            sliderHelligkeit.value = h;
            ApplyHelligkeit(h);
            UpdateHelligkeitLabel(h);
        }

        if (sliderLautstaerke != null)
        {
            float v = PlayerPrefs.GetFloat(PREF_LAUTSTAERKE, 0.8f);
            sliderLautstaerke.value = v;
            ApplyLautstaerke(v);
            UpdateLautstaerkeLabel(v);
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
            gameObject.SetActive(false); // Fallback
    }
}
