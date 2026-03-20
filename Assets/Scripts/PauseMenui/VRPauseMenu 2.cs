using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class VRPauseMenu : MonoBehaviour
{
    [Header("═══ VR Setup ═══")]
    public Transform xrOrigin;

    [Header("═══ Settings ═══")]
    public string mainMenuScene = "MainMenu";

    [Header("═══ Menu Appearance ═══")]
    public float menuDistance = 1.5f;
    public float menuScale = 0.001f;

    [Header("═══ Input (Optional) ═══")]
    public InputActionReference menuButtonAction;

    private GameObject menuCanvas;
    private Camera mainCam;
    private bool isPaused = false;
    private bool menuCreated = false;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null && xrOrigin != null)
            mainCam = xrOrigin.GetComponentInChildren<Camera>();

        if (menuButtonAction != null)
            menuButtonAction.action.Enable();
    }

    void OnDestroy()
    {
        // nichts zu deregistrieren da wir per Update pollen
    }

    void Update()
    {
        // Escape für Desktop-Test
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("[PauseMenu] Escape gedrückt");
            TogglePause();
        }

        // VR Button — per Frame gepolt statt Callback
        if (menuButtonAction != null &&
            menuButtonAction.action.WasPressedThisFrame())
        {
            Debug.Log("[PauseMenu] VR Button gedrückt");
            TogglePause();
        }

        // Menu vor Kamera halten
        if (isPaused && menuCanvas != null && mainCam != null)
        {
            Vector3 forward = mainCam.transform.forward;
            forward.y = 0;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            menuCanvas.transform.position = mainCam.transform.position +
                forward * menuDistance + Vector3.up * -0.1f;
            menuCanvas.transform.rotation = Quaternion.LookRotation(
                menuCanvas.transform.position - mainCam.transform.position, Vector3.up);
        }
    }

    public void TogglePause()
    {
        Debug.Log("[PauseMenu] TogglePause aufgerufen, isPaused war: " + isPaused);
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    void PauseGame()
    {
        isPaused = true;
        // KEIN Time.timeScale = 0 — blockiert Input in VR!

        if (!menuCreated) CreateMenu();
        menuCanvas.SetActive(true);
        Debug.Log("[PauseMenu] Spiel pausiert, Menu aktiv");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (menuCanvas != null) menuCanvas.SetActive(false);
        Debug.Log("[PauseMenu] Spiel fortgesetzt");
    }

    public void BackToMainMenu()
    {
        AutoSaveSystem.Save(xrOrigin);
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(mainMenuScene);
    }

    void CreateMenu()
    {
        menuCreated = true;

        menuCanvas = new GameObject("PauseMenuCanvas");
        Canvas canvas = menuCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = menuCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(500, 350);
        canvasRect.localScale = Vector3.one * menuScale;

        menuCanvas.AddComponent<CanvasGroup>();
        menuCanvas.AddComponent<GraphicRaycaster>();

        // Hintergrund
        GameObject panel = new GameObject("Background");
        panel.transform.SetParent(menuCanvas.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.85f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Titel
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "PAUSED";
        title.fontSize = 42;
        title.color = Color.white;
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.7f);
        titleRect.anchorMax = new Vector2(1, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        CreateButton(panel.transform, "Weiter spielen",
            new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.6f), () => ResumeGame());

        CreateButton(panel.transform, "Zurück zum Hauptmenü",
            new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.3f), () => BackToMainMenu());

        // Initial vor Kamera positionieren
        if (mainCam != null)
        {
            Vector3 forward = mainCam.transform.forward;
            forward.y = 0;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            menuCanvas.transform.position = mainCam.transform.position + forward * menuDistance;
            menuCanvas.transform.rotation = Quaternion.LookRotation(
                menuCanvas.transform.position - mainCam.transform.position, Vector3.up);
        }
    }

    void CreateButton(Transform parent, string label,
        Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(label);
        btnObj.transform.SetParent(parent, false);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        btn.colors = colors;

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = 28;
        txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.Center;
        RectTransform txtRect = textObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;
    }
}