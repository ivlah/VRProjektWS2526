using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeIn : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.8f;

    private void Start() => StartCoroutine(DoFade());

    private IEnumerator DoFade()
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        // Canvas im World Space vor der Kamera
        GameObject cGO = new GameObject("FadeIn");
        Canvas c = cGO.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.sortingOrder = 999;

        // Direkt vor der Kamera positionieren
        cGO.transform.position = cam.transform.position + cam.transform.forward * 0.5f;
        cGO.transform.rotation = cam.transform.rotation;
        cGO.transform.localScale = Vector3.one * 0.001f;

        RectTransform cRect = cGO.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(4000, 4000);

        GameObject imgGO = new GameObject("Overlay");
        imgGO.transform.SetParent(cGO.transform, false);
        Image img = imgGO.AddComponent<Image>();
        img.color = Color.black;
        RectTransform rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // Ausblenden
        float t = 0f;
        while (t < fadeDuration)
        {
            // Canvas immer vor der Kamera halten
            cGO.transform.position = cam.transform.position + cam.transform.forward * 0.5f;
            cGO.transform.rotation = cam.transform.rotation;

            t += Time.deltaTime;
            img.color = new Color(0f, 0f, 0f, 1f - (t / fadeDuration));
            yield return null;
        }

        Destroy(cGO);
    }
}