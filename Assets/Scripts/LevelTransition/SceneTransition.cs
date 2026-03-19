using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// A Doll's House — Scene Transition (v3)
///
/// Blendet aus wenn Spieler durch Tür geht → lädt nächste Szene.
/// Der Chapter-Titel wird vom ChapterIntro Script in der neuen Szene angezeigt.
///
/// SETUP pro Ausgangstür:
///   1. Leeres GameObject hinter der Tür → "DoorTransition"
///   2. Box Collider → Is Trigger ✓
///   3. Dieses Script dranhängen
///   4. Next Scene Name eintragen
/// </summary>
public class SceneTransition : MonoBehaviour
{
    [Header("Nächste Szene")]
    [Tooltip("Exakter Szenenname — muss in Build Settings stehen!")]
    [SerializeField] private string nextSceneName = "Room2";

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.8f;

    // ---------------------------------------------------------------

    private bool isTransitioning = false;
    private Canvas fadeCanvas;
    private Image  fadeImage;

    // ---------------------------------------------------------------

    private void Awake()
    {
        // Einfaches schwarzes Overlay für Ausblende beim Verlassen
        GameObject cGO  = new GameObject("FadeCanvas_Exit");
        DontDestroyOnLoad(cGO);
        fadeCanvas      = cGO.AddComponent<Canvas>();
        fadeCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 998;
        cGO.AddComponent<CanvasScaler>();
        cGO.AddComponent<GraphicRaycaster>();

        GameObject imgGO = new GameObject("Overlay");
        imgGO.transform.SetParent(cGO.transform, false);
        fadeImage        = imgGO.AddComponent<Image>();
        fadeImage.color  = new Color(0f, 0f, 0f, 0f);
        RectTransform rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin     = Vector2.zero;
        rt.anchorMax     = Vector2.one;
        rt.offsetMin     = rt.offsetMax = Vector2.zero;
    }

    // ---------------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (isTransitioning) return;

        if (other.CompareTag("Player")                 ||
            other.name.ToLower().Contains("xr")        ||
            other.name.ToLower().Contains("origin")    ||
            other.name.ToLower().Contains("rig")       ||
            other.name.ToLower().Contains("camera"))
        {
            StartCoroutine(TransitionRoutine());
        }
    }

    // ---------------------------------------------------------------

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;

        // AutoSave
        AutoSaveSystem.Save();

        // Schwarzblende rein
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // Szene laden — ChapterIntro in der neuen Szene übernimmt ab hier
        SceneTransitionData.SpawnAtDoor = true;
        SceneManager.LoadScene(nextSceneName);

        // FadeCanvas zerstören — ChapterIntro startet mit eigenem schwarzen Screen
        Destroy(fadeCanvas.gameObject);
        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0f, 0f, 0f,
                Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        fadeImage.color = new Color(0f, 0f, 0f, to);
    }
}

/// <summary>Flag für DoorSpawnController.</summary>
public static class SceneTransitionData
{
    public static bool SpawnAtDoor = false;
}
