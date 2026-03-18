using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// A Doll's House — VR Main Menu Manager (v2)
///
/// • Start New Game  → löscht AutoSave, lädt "Room1"
/// • Continue        → lädt die zuletzt gespeicherte Szene (AutoSaveSystem)
/// • Options         → öffnet OptionsPanel, blendet Hauptmenü aus
/// • Quit            → beendet die App sofort (Quest 3 + Editor)
///
/// Attach to: MainMenu Root GameObject (World Space Canvas Parent)
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Inspector
    // ---------------------------------------------------------------

    [Header("Szenen")]
    [Tooltip("Szenenname für Room 1. Muss in Build Settings eingetragen sein.")]
    [SerializeField] private string room1SceneName = "Room1";

    [Header("Canvas")]
    [Tooltip("Der Canvas des Hauptmenüs (wird ausgeblendet wenn Options offen ist).")]
    [SerializeField] private Canvas mainMenuCanvas;

    [Header("Buttons")]
    [SerializeField] private Button btnStartNewGame;
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnOptions;
    [SerializeField] private Button btnQuit;

    [Header("Options Panel")]
    [Tooltip("Das OptionsPanel GameObject. Wird per Script ein-/ausgeblendet.")]
    [SerializeField] private GameObject optionsPanel;

    // ---------------------------------------------------------------
    // Unity Lifecycle
    // ---------------------------------------------------------------

    private void Awake()
    {
        ValidateCanvas();
        BindButtons();
        RefreshContinueButton();

        // Sicherstellen dass Options beim Start geschlossen ist
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    // ---------------------------------------------------------------
    // Initialisierung
    // ---------------------------------------------------------------

    private void ValidateCanvas()
    {
        if (mainMenuCanvas == null)
            mainMenuCanvas = GetComponentInChildren<Canvas>();

        if (mainMenuCanvas != null && mainMenuCanvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogWarning("[MainMenu] Canvas RenderMode wird auf WorldSpace korrigiert.");
            mainMenuCanvas.renderMode = RenderMode.WorldSpace;
        }
    }

    private void BindButtons()
    {
        Bind(btnStartNewGame, OnStartNewGame, nameof(btnStartNewGame));
        Bind(btnContinue,     OnContinue,     nameof(btnContinue));
        Bind(btnOptions,      OnOptions,      nameof(btnOptions));
        Bind(btnQuit,         OnQuit,         nameof(btnQuit));
    }

    private static void Bind(Button btn, UnityEngine.Events.UnityAction action, string label)
    {
        if (btn != null)
            btn.onClick.AddListener(action);
        else
            Debug.LogError($"[MainMenu] Button '{label}' ist nicht im Inspector zugewiesen!");
    }

    /// <summary>
    /// Continue-Button nur anklickbar wenn ein AutoSave existiert.
    /// </summary>
    public void RefreshContinueButton()
    {
        if (btnContinue == null) return;

        bool hasSave = AutoSaveSystem.HasSave();
        btnContinue.interactable = hasSave;

        CanvasGroup cg = btnContinue.GetComponent<CanvasGroup>();
        if (cg != null)
            cg.alpha = hasSave ? 1f : 0.4f;
    }

    // ---------------------------------------------------------------
    // Button Handler
    // ---------------------------------------------------------------

    /// <summary>Start New Game: AutoSave löschen → Room1 laden.</summary>
    public void OnStartNewGame()
    {
        Debug.Log("[MainMenu] → Start New Game (Room1)");
        AutoSaveSystem.DeleteSave();
        LoadScene(room1SceneName);
    }

    /// <summary>Continue: gespeicherte Szene aus dem AutoSave laden.</summary>
    public void OnContinue()
    {
        string savedScene = AutoSaveSystem.GetSavedScene();

        if (string.IsNullOrEmpty(savedScene))
        {
            Debug.LogWarning("[MainMenu] Kein AutoSave gefunden — Continue ignoriert.");
            return;
        }

        Debug.Log($"[MainMenu] → Continue: Lade '{savedScene}'");
        LoadScene(savedScene);
    }

    /// <summary>Options: Hauptmenü ausblenden, Options Panel öffnen.</summary>
    public void OnOptions()
    {
        if (optionsPanel == null)
        {
            Debug.LogError("[MainMenu] optionsPanel ist nicht zugewiesen!");
            return;
        }

        bool nowOpen = !optionsPanel.activeSelf;
        optionsPanel.SetActive(nowOpen);

        if (mainMenuCanvas != null)
            mainMenuCanvas.gameObject.SetActive(!nowOpen);

        Debug.Log($"[MainMenu] Options Panel: {(nowOpen ? "geöffnet" : "geschlossen")}");
    }

    /// <summary>
    /// Schließt das Options Panel und zeigt das Hauptmenü wieder.
    /// Aufruf durch den Back-Button im OptionsPanel.
    /// </summary>
    public void OnCloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (mainMenuCanvas != null)
            mainMenuCanvas.gameObject.SetActive(true);
    }

    /// <summary>Quit: App sofort beenden — funktioniert auf Meta Quest 3.</summary>
    public void OnQuit()
    {
        Debug.Log("[MainMenu] → Quit");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(0);
#endif
    }

    // ---------------------------------------------------------------
    // Utility
    // ---------------------------------------------------------------

    private static void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[MainMenu] Szenenname ist leer!");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}
