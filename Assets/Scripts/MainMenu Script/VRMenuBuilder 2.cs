using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// A Doll's House — VR Menu Builder (v2)
/// Generiert MainMenu + OptionsPanel komplett in der Szene.
///
/// Ausführen über: Tools > A Doll's House > Build VR Main Menu
/// </summary>
public class VRMenuBuilder : EditorWindow
{
    [MenuItem("Tools/A Doll's House/Build VR Main Menu")]
    public static void BuildMenu()
    {
        // -------------------------------------------------------
        // Root
        // -------------------------------------------------------
        GameObject root = new GameObject("MainMenu_Root");
        Undo.RegisterCreatedObjectUndo(root, "Create VR Main Menu");
        root.transform.position = new Vector3(0f, 1.6f, 2f);

        // -------------------------------------------------------
        // Hauptmenü Canvas
        // -------------------------------------------------------
        GameObject mainMenuGO = BuildWorldCanvas("MainMenu_Canvas", root.transform,
            width: 80f, height: 100f, scale: 0.01f, offset: Vector3.zero);

        // Hintergrund
        CreateImage("BG", mainMenuGO.transform, new Color(0.03f, 0.03f, 0.09f, 0.72f), true);

        // Titel
        CreateTMP("Title", mainMenuGO.transform, "A Doll's House",
            fontSize: 9f, color: new Color(0.95f, 0.88f, 0.72f, 1f), bold: true,
            anchorMin: new Vector2(0.1f, 0.78f), anchorMax: new Vector2(0.9f, 0.95f));

        // Untertitel
        CreateTMP("Subtitle", mainMenuGO.transform, "— A Story Unfolds —",
            fontSize: 3.5f, color: new Color(0.75f, 0.68f, 0.55f, 0.8f), bold: false,
            anchorMin: new Vector2(0.1f, 0.70f), anchorMax: new Vector2(0.9f, 0.79f));

        // Trennlinie
        CreateImage("Divider", mainMenuGO.transform, new Color(1f, 1f, 1f, 0.18f), false,
            anchorMin: new Vector2(0.1f, 0.685f), anchorMax: new Vector2(0.9f, 0.688f));

        // Button Container
        GameObject btnContainer = CreateLayoutContainer("Buttons", mainMenuGO.transform,
            anchorMin: new Vector2(0.05f, 0.10f), anchorMax: new Vector2(0.95f, 0.67f));

        string[] labels = { "Start New Game", "Continue", "Options", "Quit" };
        string[] names  = { "Btn_StartNewGame", "Btn_Continue", "Btn_Options", "Btn_Quit" };
        for (int i = 0; i < labels.Length; i++)
            CreateMenuButton(names[i], labels[i], btnContainer.transform);

        // Footer
        CreateTMP("Footer", mainMenuGO.transform, "© 2025  A Doll's House",
            fontSize: 2.2f, color: new Color(1f, 1f, 1f, 0.22f), bold: false,
            anchorMin: new Vector2(0.05f, 0.01f), anchorMax: new Vector2(0.95f, 0.08f));

        // -------------------------------------------------------
        // Options Panel Canvas (daneben, leicht versetzt)
        // -------------------------------------------------------
        GameObject optionsGO = BuildWorldCanvas("OptionsPanel_Canvas", root.transform,
            width: 90f, height: 110f, scale: 0.01f, offset: new Vector3(0f, 0f, 0f));
        optionsGO.SetActive(false); // Startet geschlossen

        BuildOptionsPanel(optionsGO);

        // -------------------------------------------------------
        // MainMenuManager hinzufügen
        // -------------------------------------------------------
        MainMenuManager manager = root.AddComponent<MainMenuManager>();

        // -------------------------------------------------------
        // Hinweis im Log
        // -------------------------------------------------------
        Debug.Log(
            "[VRMenuBuilder] Done!\n" +
            "Nächste Schritte:\n" +
            "  1. Im MainMenuManager Inspector: Buttons + OptionsPanel_Canvas zuweisen\n" +
            "  2. Im OptionsPanelController Inspector: Sliders, Tabs, MainMenuManager zuweisen\n" +
            "  3. 'Room1' in Build Settings eintragen\n" +
            "  4. AutoSaveSystem Script in die Room1-Szene legen"
        );

        Selection.activeGameObject = root;
        EditorUtility.SetDirty(root);
    }

    // ---------------------------------------------------------------
    // Options Panel Aufbau
    // ---------------------------------------------------------------

    private static void BuildOptionsPanel(GameObject parent)
    {
        // Hintergrund
        CreateImage("BG", parent.transform, new Color(0.04f, 0.04f, 0.12f, 0.85f), true);

        // Titel
        CreateTMP("Title", parent.transform, "Optionen",
            fontSize: 7f, color: new Color(0.95f, 0.88f, 0.72f, 1f), bold: true,
            anchorMin: new Vector2(0.05f, 0.88f), anchorMax: new Vector2(0.95f, 0.98f));

        // Tab Buttons (Grafik | Audio | Anleitung)
        GameObject tabBar = CreateLayoutContainer("TabBar", parent.transform,
            anchorMin: new Vector2(0.03f, 0.79f), anchorMax: new Vector2(0.97f, 0.88f),
            vertical: false);
        string[] tabs = { "Grafik", "Audio", "Anleitung" };
        string[] tabNames = { "Tab_Grafik", "Tab_Audio", "Tab_Anleitung" };
        for (int i = 0; i < tabs.Length; i++)
            CreateSmallButton(tabNames[i], tabs[i], tabBar.transform, height: 8f);

        // Trennlinie
        CreateImage("TabDivider", parent.transform, new Color(1f, 1f, 1f, 0.15f), false,
            anchorMin: new Vector2(0.03f, 0.783f), anchorMax: new Vector2(0.97f, 0.786f));

        // ── Panel: Grafik ────────────────────────────────────────
        GameObject panelGrafik = CreatePanel("Panel_Grafik", parent.transform,
            anchorMin: new Vector2(0.03f, 0.12f), anchorMax: new Vector2(0.97f, 0.78f));

        CreateTMP("LabelHelligkeit", panelGrafik.transform, "Helligkeit: 100%",
            fontSize: 4f, color: new Color(0.9f, 0.85f, 0.75f, 1f), bold: false,
            anchorMin: new Vector2(0.05f, 0.80f), anchorMax: new Vector2(0.95f, 0.95f));

        CreateSlider("Slider_Helligkeit", panelGrafik.transform,
            anchorMin: new Vector2(0.05f, 0.60f), anchorMax: new Vector2(0.95f, 0.80f));

        CreateTMP("HinweisGrafik", panelGrafik.transform,
            "Passt die Bildschirmhelligkeit\nund den Gamma-Wert an.",
            fontSize: 3.2f, color: new Color(0.7f, 0.65f, 0.55f, 0.8f), bold: false,
            anchorMin: new Vector2(0.05f, 0.10f), anchorMax: new Vector2(0.95f, 0.55f));

        // ── Panel: Audio ─────────────────────────────────────────
        GameObject panelAudio = CreatePanel("Panel_Audio", parent.transform,
            anchorMin: new Vector2(0.03f, 0.12f), anchorMax: new Vector2(0.97f, 0.78f));
        panelAudio.SetActive(false);

        CreateTMP("LabelLautstaerke", panelAudio.transform, "Lautstärke: 80%",
            fontSize: 4f, color: new Color(0.9f, 0.85f, 0.75f, 1f), bold: false,
            anchorMin: new Vector2(0.05f, 0.80f), anchorMax: new Vector2(0.95f, 0.95f));

        CreateSlider("Slider_Lautstaerke", panelAudio.transform,
            anchorMin: new Vector2(0.05f, 0.60f), anchorMax: new Vector2(0.95f, 0.80f));

        CreateTMP("HinweisAudio", panelAudio.transform,
            "Master-Lautstärke für alle\nSounds im Spiel.",
            fontSize: 3.2f, color: new Color(0.7f, 0.65f, 0.55f, 0.8f), bold: false,
            anchorMin: new Vector2(0.05f, 0.10f), anchorMax: new Vector2(0.95f, 0.55f));

        // ── Panel: Anleitung ─────────────────────────────────────
        GameObject panelAnleitung = CreatePanel("Panel_Anleitung", parent.transform,
            anchorMin: new Vector2(0.03f, 0.12f), anchorMax: new Vector2(0.97f, 0.78f));
        panelAnleitung.SetActive(false);

        // Scrollable Text
        GameObject scrollView = CreateUIObject("ScrollView", panelAnleitung.transform);
        scrollView.AddComponent<ScrollRect>();
        SetAnchors(scrollView.GetComponent<RectTransform>(),
            new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f));

        GameObject scrollContent = CreateUIObject("Content", scrollView.transform);
        ContentSizeFitter csf = scrollContent.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI anleitungText = scrollContent.AddComponent<TextMeshProUGUI>();
        anleitungText.fontSize = 3.0f;
        anleitungText.color = new Color(0.85f, 0.80f, 0.70f, 1f);
        anleitungText.text = "Anleitung wird per Script befüllt (OptionsPanelController).";

        ScrollRect sr = scrollView.GetComponent<ScrollRect>();
        sr.content = scrollContent.GetComponent<RectTransform>();
        sr.vertical = true;
        sr.horizontal = false;

        // ── Back Button ──────────────────────────────────────────
        CreateSmallButton("Btn_Back", "← Zurück", parent.transform,
            height: 9f,
            anchorMin: new Vector2(0.25f, 0.01f), anchorMax: new Vector2(0.75f, 0.10f));

        // ── OptionsPanelController ───────────────────────────────
        OptionsPanelController ctrl = parent.AddComponent<OptionsPanelController>();
        Debug.Log("[VRMenuBuilder] OptionsPanelController hinzugefügt — bitte Felder im Inspector zuweisen.");
    }

    // ---------------------------------------------------------------
    // Helper: Canvas
    // ---------------------------------------------------------------

    private static GameObject BuildWorldCanvas(string name, Transform parent,
        float width, float height, float scale, Vector3 offset)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = offset;

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        go.AddComponent<GraphicRaycaster>();

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);
        rt.localScale = Vector3.one * scale;

        return go;
    }

    // ---------------------------------------------------------------
    // Helper: UI Elements
    // ---------------------------------------------------------------

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static void CreateImage(string name, Transform parent, Color color,
        bool stretch,
        Vector2? anchorMin = null, Vector2? anchorMax = null)
    {
        GameObject go = CreateUIObject(name, parent);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();

        if (stretch)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
        else if (anchorMin.HasValue && anchorMax.HasValue)
        {
            SetAnchors(rt, anchorMin.Value, anchorMax.Value);
        }
    }

    private static void CreateTMP(string name, Transform parent, string text,
        float fontSize, Color color, bool bold,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = CreateUIObject(name, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax);
    }

    private static GameObject CreateLayoutContainer(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, bool vertical = true)
    {
        GameObject go = CreateUIObject(name, parent);
        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax);

        if (vertical)
        {
            VerticalLayoutGroup vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(8, 8, 0, 0);
        }
        else
        {
            HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(4, 4, 0, 0);
        }
        return go;
    }

    private static GameObject CreatePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = CreateUIObject(name, parent);
        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax);
        return go;
    }

    private static void CreateSlider(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = CreateUIObject(name, parent);
        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax);
        go.AddComponent<Slider>();
        // Slider-Visuals werden automatisch von Unity generiert
        // oder du nutzt das Default Slider Prefab per Drag & Drop
    }

    private static void CreateMenuButton(string name, string label, Transform parent)
    {
        GameObject go = CreateUIObject(name, parent);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 11f);

        Image img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.06f);

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.55f);
        outline.effectDistance = new Vector2(1f, -1f);

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = new Color(1f, 1f, 1f, 0.06f);
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.20f);
        cb.pressedColor     = new Color(1f, 1f, 1f, 0.32f);
        cb.selectedColor    = new Color(1f, 1f, 1f, 0.12f);
        cb.disabledColor    = new Color(0.5f, 0.5f, 0.5f, 0.15f);
        cb.fadeDuration     = 0.1f;
        btn.colors          = cb;
        btn.targetGraphic   = img;

        go.AddComponent<CanvasGroup>(); // für Alpha-Fade bei Continue

        GameObject labelGo = CreateUIObject("Label", go.transform);
        TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 4.5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = new Color(0.95f, 0.92f, 0.85f, 1f);

        RectTransform lr = labelGo.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero;
        lr.anchorMax = Vector2.one;
        lr.offsetMin = new Vector2(4f, 1f);
        lr.offsetMax = new Vector2(-4f, -1f);
    }

    private static void CreateSmallButton(string name, string label, Transform parent,
        float height = 8f,
        Vector2? anchorMin = null, Vector2? anchorMax = null)
    {
        GameObject go = CreateUIObject(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, height);

        if (anchorMin.HasValue && anchorMax.HasValue)
            SetAnchors(rt, anchorMin.Value, anchorMax.Value);

        Image img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.06f);

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.40f);
        outline.effectDistance = new Vector2(0.8f, -0.8f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        GameObject labelGo = CreateUIObject("Label", go.transform);
        TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 3.8f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.90f, 0.86f, 0.78f, 1f);

        RectTransform lr = labelGo.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero;
        lr.anchorMax = Vector2.one;
        lr.offsetMin = new Vector2(2f, 0f);
        lr.offsetMax = new Vector2(-2f, 0f);
    }
}
#endif
