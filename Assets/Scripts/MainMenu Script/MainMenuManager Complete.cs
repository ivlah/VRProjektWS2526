using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// A Doll's House — VR Main Menu Manager (v2)
///
/// • Start New Game  → deletes AutoSave and loads "Room1"
/// • Continue        → loads the most recently saved scene (AutoSaveSystem)
/// • Options         → opens the options panel and hides the main menu
/// • Quit            → exits the app immediately (Quest 3 + Editor)
///
/// Attach to: MainMenu Root GameObject (World Space Canvas Parent)
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Inspector
    // ---------------------------------------------------------------

    [Header("Scenes")]
    [Tooltip("Scene name for Room 1. Must be added to Build Settings.")]
    [SerializeField] private string room1SceneName = "Room1";

    [Header("Canvas")]
    [Tooltip("Main menu canvas (hidden when the options panel is open).")]
    [SerializeField] private Canvas mainMenuCanvas;

    [Header("Buttons")]
    [SerializeField] private Button btnStartNewGame;
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnOptions;
    [SerializeField] private Button btnQuit;

    [Header("Options Panel")]
    [Tooltip("Options panel GameObject that is shown/hidden by script.")]
    [SerializeField] private GameObject optionsPanel;

    // ---------------------------------------------------------------
    // Unity Lifecycle
    // ---------------------------------------------------------------

    private void Awake()
    {
        ValidateCanvas();
        BindButtons();
        RefreshContinueButton();

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    // ---------------------------------------------------------------
    // Initialization
    // ---------------------------------------------------------------

    private void ValidateCanvas()
    {
        if (mainMenuCanvas == null)
            mainMenuCanvas = GetComponentInChildren<Canvas>();

        if (mainMenuCanvas != null && mainMenuCanvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogWarning("[MainMenu] Canvas RenderMode is being corrected to WorldSpace.");
            mainMenuCanvas.renderMode = RenderMode.WorldSpace;
        }
    }

    private void BindButtons()
    {
        Bind(btnStartNewGame, OnStartNewGame, nameof(btnStartNewGame));
        Bind(btnContinue, OnContinue, nameof(btnContinue));
        Bind(btnOptions, OnOptions, nameof(btnOptions));
        Bind(btnQuit, OnQuit, nameof(btnQuit));
    }

    private static void Bind(Button btn, UnityEngine.Events.UnityAction action, string label)
    {
        if (btn != null)
            btn.onClick.AddListener(action);
        else
            Debug.LogError($"[MainMenu] Button '{label}' is not assigned in the Inspector!");
    }

    /// <summary>
    /// Continue button should only be interactable if an autosave exists.
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
    // Button Handlers
    // ---------------------------------------------------------------

    /// <summary>Start New Game: delete AutoSave and load Room1.</summary>
    public void OnStartNewGame()
    {
        Debug.Log("[MainMenu] → Start New Game (Room1)");
        AutoSaveSystem.DeleteSave();
        LoadScene(room1SceneName);
    }

    /// <summary>Continue from the last saved scene.</summary>
    public void OnContinue()
    {
        if (!AutoSaveSystem.HasSave())
        {
            Debug.LogWarning("[MainMenu] No save found — Continue cancelled.");
            RefreshContinueButton();
            return;
        }

        string sceneToLoad = AutoSaveSystem.GetSavedScene();
        Debug.Log($"[MainMenu] → Continue ({sceneToLoad})");
        LoadScene(sceneToLoad);
    }

    /// <summary>Open the options panel and hide the main menu canvas.</summary>
    public void OnOptions()
    {
        Debug.Log("[MainMenu] → Open Options");

        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        if (mainMenuCanvas != null)
            mainMenuCanvas.gameObject.SetActive(false);
    }

    /// <summary>Called by OptionsPanelController when leaving the options panel.</summary>
    public void OnCloseOptions()
    {
        Debug.Log("[MainMenu] ← Close Options");

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (mainMenuCanvas != null)
            mainMenuCanvas.gameObject.SetActive(true);

        RefreshContinueButton();
    }

    /// <summary>Quit app (or stop Play Mode in the editor).</summary>
    public void OnQuit()
    {
        Debug.Log("[MainMenu] → Quit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------------------------------------------------------------
    // Scene Loading Helper
    // ---------------------------------------------------------------

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[MainMenu] Scene name is empty!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[MainMenu] Scene '{sceneName}' is not in Build Settings!");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}