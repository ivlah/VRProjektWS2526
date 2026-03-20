using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// A Doll's House — VR Menu Builder (v2)
/// Generates Main Menu + Options Panel directly in the scene.
///
/// Run via: Tools > A Doll's House > Build VR Main Menu
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
        // Main Menu Canvas
        // -------------------------------------------------------
        GameObject mainMenuGO = BuildWorldCanvas("MainMenu_Canvas", root.transform,
            width: 80f, height: 100f, scale: 0.01f, offset: Vector3.zero);

        CreateImage("BG", mainMenuGO.transform, new Color(0.03f, 0.03f, 0.09f, 0.72f), true);

        CreateTMP("Title", mainMenuGO.transform, "A Doll's House",
            fontSize: 9f, color: new Color(0.95f, 0.88f, 0.72f, 1f), bold: true,
            anchorMin: new Vector2(0.1f, 0.78f), anchorMax: new Vector2(0.9f, 0.95f));

        CreateTMP("Subtitle", mainMenuGO.transform, "— A Story Unfolds —",
            fontSize: 3.5f, color: new Color(0.75f, 0.68f, 0.55f, 0.8f), bold: false,
            anchorMin: new Vector2(0.1f, 0.70f), anchorMax: new Vector2(0.9f, 0.79f));

        CreateImage("Divider", mainMenuGO.transform, new Color(1f, 1f, 1f, 0.18f), false,
            anchorMin: new Vector2(0.1f, 0.685f), anchorMax: new Vector2(0.9f, 0.688f));

        GameObject btnContainer = CreateLayoutContainer("Buttons", mainMenuGO.transform,
            anchorMin: new Vector2(0.05f, 0.10f), anchorMax: new Vector2(0.95f, 0.67f));

        string[] labels = { "Start New Game", "Continue", "Options", "Quit" };
        string[] names = { "Btn_StartNewGame", "Btn_Continue", "Btn_Options", "Btn_Quit" };
        for (int i = 0; i < labels.Length; i++)
            CreateMenuButton(names[i], labels[i], btnContainer.transform);

        CreateTMP("Footer", mainMenuGO.transform, "© 2025  A Doll's House",
            fontSize: 2.2f, color: new Color(1f, 1f, 1f, 0.22f), bold: false,
            anchorMin: new Vector2(0.05f, 0.01f), anchorMax: new Vector2(0.95f, 0.08f));

        // -------------------------------------------------------
        // Options Panel Canvas
        // -------------------------------------------------------
        GameObject optionsGO = BuildWorldCanvas("OptionsPanel_Canvas", root.transform,
            width: 90f, height: 110f, scale: 0.01f, offset: new Vector3(0f, 0f, 0f));
        optionsGO.SetActive(false);

        BuildOptionsPanel(optionsGO);

        // -------------------------------------------------------
        // Add MainMenuManager
        // -------------------------------------------------------
        root.AddComponent<MainMenuManager>();

        Debug.Log(
            "[VRMenuBuilder] Done!\n" +
            "Next steps:\n" +
            "  1. In MainMenuManager Inspector: assign buttons + OptionsPanel_Canvas\n" +
            "  2. In OptionsPanelController Inspector: assign sliders, tabs, MainMenuManager\n" +
            "  3. Add 'Room1' to Build Settings\n" +
            "  4. Place AutoSaveSystem in the Room1 scene"
        );

        Selection.activeGameObject = root;
        EditorUtility.SetDirty(root);
    }

    // ---------------------------------------------------------------
    // Options Panel
    // ---------------------------------------------------------------

    private static void BuildOptionsPanel(GameObject parent)
    {
        CreateImage("BG", parent.transform, new Color(0.04f, 0.04f, 0.12f, 0.85f), true);

        CreateTMP("Title", parent.transform, "Options",
            fontSize: 7f, color: new Color(0.95f, 0.88f, 0.72f, 1f), bold: true,
            anchorMin: new Vector2(0.05f, 0.88f), anchorMax: new Vector2(0.95f, 0.98f));

        GameObject tabBar = CreateLayoutContainer("TabBar", parent.transform,
            anchorMin: new Vector2(0.03f, 0.79f), anchorMax: new Vector2(0.97f, 0.88f),
            vertical: false);

        string[] tabs = { "Graphics", "Audio", "Guidance" };
        string[] tabNames = { "Tab_Graphics", "Tab_Audio", "Tab_Guidance" };
        for (int i = 0; i < tabs.Length; i++)
            CreateSmallButton(tabNames[i], tabs[i], tabBar.transform, height: 8f);

        CreateImage("TabDivider", parent.transform, new Color(1f, 1f, 1f, 0.15f), false,
            anchorMin: new Vector2(0.03f, 0.783f), anchorMax: new Vector2(0.97f, 0.786f));

        // Graphics Panel
        GameObject panelGraphics = CreatePanel("Panel_Graphics", parent.transform,
            anchorMin: new Vector2(0.03f, 0.12f), anchorMax: new Vector2(0.97f, 0.78f));

        CreateTMP("LabelBrightness", panelGraphics.transform, "Brightness: 100%",
            fontSize: 4f, color: new Color(0.9f, 0.85f, 0.75f, 1f), bold: false,
            anchorMin: new Vector2(0.05f, 0.80f), anchorMax: new Vector2(0.95f, 0.95f));

        CreateSlider("Slider_Brightness", panelGraphics.transform,
            anchorMin: new Vector2(0.05f, 0.60f), anchorMax: new Vector2(0.95f, 0.80f));

        CreateTMP("HintBrightness", panelGraphics.transform,
            "Adjust the visual brightness of the scene.",
            fontSize: 3.2f, color: new Color(0.7f, 0.65f, 0.55f, 0.8f), bold: false,
            anchorMin: new Vector2(0.05f, 0.10f), anchorMax: new Vector2(0.95f, 0.55f));

        // Audio Panel
        GameObject panelAudio = CreatePanel("Panel_Audio", parent.transform,
            anchorMin: new Vector2(0.03f, 0.12f), anchorMax: new Vector2(0.97f, 0.78f));
        panelAudio.SetActive(false);

        CreateTMP("LabelVolume", panelAudio.transform, "Volume: 80%",
            fontSize: 4f, color: new Color(0.9f, 0.85f, 0.75f, 1f), bold: false,
            anchorMin: new Vector2(0.05f, 0.80f), anchorMax: new Vector2(0.95f, 0.95f));

        CreateSlider("Slider_Volume", panelAudio.transform,
            anchorMin: new Vector2(0.05f, 0.60f), anchorMax: new Vector2(0.95f, 0.80f));

        CreateTMP("HintAudio", panelAudio.transform,
            "Master volume for all sounds in the game.",
            fontSize: 3.2f, color: new Color(0.7f, 0.65f, 0.55f, 0.8f), bold: false,
            anchorMin: new Vector2(0.05f, 0.10f), anchorMax: new Vector2(0.95f, 0.55f));

        // Guidance Panel
        GameObject panelGuidance = CreatePanel("Panel_Guidance", parent.transform,
            anchorMin: new Vector2(0.03f, 0.12f), anchorMax: new Vector2(0.97f, 0.78f));
        panelGuidance.SetActive(false);

        GameObject scrollView = CreateUIObject("ScrollView", panelGuidance.transform);
        scrollView.AddComponent<ScrollRect>();
        SetAnchors(scrollView.GetComponent<RectTransform>(),
            new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f));

        GameObject scrollContent = CreateUIObject("Content", scrollView.transform);
        ContentSizeFitter csf = scrollContent.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI guidanceText = scrollContent.AddComponent<TextMeshProUGUI>();
        guidanceText.fontSize = 3.0f;
        guidanceText.color = new Color(0.85f, 0.80f, 0.70f, 1f);
        guidanceText.text = "Guidance will be filled by script (OptionsPanelController).";

        ScrollRect sr = scrollView.GetComponent<ScrollRect>();
        sr.content = scrollContent.GetComponent<RectTransform>();
        sr.vertical = true;
        sr.horizontal = false;

        // Back Button
        CreateSmallButton("Btn_Back", "← Back", parent.transform,
            height: 9f,
            anchorMin: new Vector2(0.25f, 0.01f), anchorMax: new Vector2(0.75f, 0.10f));

        // Add controller
        parent.AddComponent<OptionsPanelController>();
        Debug.Log("[VRMenuBuilder] OptionsPanelController added — assign fields in the Inspector.");
    }

    // ---------------------------------------------------------------
    // Helpers
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

    private static GameObject CreatePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = CreateUIObject(name, parent);
        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax);
        return go;
    }

    private static GameObject CreateLayoutContainer(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, bool vertical = true)
    {
        GameObject go = CreateUIObject(name, parent);
        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax);

        if (vertical)
        {
            VerticalLayoutGroup vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 3f;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
        }
        else
        {
            HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 3f;
            hlg.childControlHeight = true;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = true;
        }

        ContentSizeFitter csf = go.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        return go;
    }

    private static GameObject CreateTMP(string name, Transform parent, string text,
        float fontSize, Color color, bool bold,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = CreateUIObject(name, parent);
        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = true;
        return go;
    }

    private static GameObject CreateMenuButton(string name, string text, Transform parent)
    {
        return CreateSmallButton(name, text, parent, 12f);
    }

    private static GameObject CreateSmallButton(string name, string text, Transform parent,
        float height,
        Vector2? anchorMin = null, Vector2? anchorMax = null)
    {
        GameObject go = CreateUIObject(name, parent);

        if (anchorMin.HasValue && anchorMax.HasValue)
            SetAnchors(go.GetComponent<RectTransform>(), anchorMin.Value, anchorMax.Value);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.08f, 0.08f, 0.14f, 0.92f);

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.08f, 0.08f, 0.14f, 0.92f);
        cb.highlightedColor = new Color(0.18f, 0.18f, 0.28f, 0.98f);
        cb.pressedColor = new Color(0.28f, 0.28f, 0.40f, 1f);
        cb.selectedColor = cb.highlightedColor;
        btn.colors = cb;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
        outline.effectDistance = new Vector2(1f, -1f);

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;

        GameObject label = CreateUIObject("Label", go.transform);
        SetAnchors(label.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

        TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 4f;
        tmp.color = new Color(0.95f, 0.92f, 0.82f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return go;
    }

    private static GameObject CreateSlider(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = CreateUIObject(name, parent);
        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax);

        Slider slider = go.AddComponent<Slider>();

        GameObject bg = CreateUIObject("Background", go.transform);
        SetAnchors(bg.GetComponent<RectTransform>(), new Vector2(0f, 0.35f), new Vector2(1f, 0.65f));
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(1f, 1f, 1f, 0.12f);

        GameObject fillArea = CreateUIObject("Fill Area", go.transform);
        SetAnchors(fillArea.GetComponent<RectTransform>(), new Vector2(0f, 0.35f), new Vector2(1f, 0.65f));

        GameObject fill = CreateUIObject("Fill", fillArea.transform);
        SetAnchors(fill.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.85f, 0.72f, 0.38f, 1f);

        GameObject handleArea = CreateUIObject("Handle Slide Area", go.transform);
        SetAnchors(handleArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

        GameObject handle = CreateUIObject("Handle", handleArea.transform);
        RectTransform handleRT = handle.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(6f, 12f);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(0.98f, 0.95f, 0.88f, 1f);

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;

        return go;
    }
}
#endif