using UnityEngine;

/// <summary>
/// A Doll's House — Main Menu Skybox
///
/// Erstellt einen schönen Sonnenaufgang-Himmel für das Main Menu.
/// Keine externe Assets nötig.
///
/// SETUP:
///   1. Leeres GameObject → "SkyboxController"
///   2. Dieses Script dranhängen
///   3. Deine Directional Light reinziehen
/// </summary>
[ExecuteAlways]
public class MainMenuSkybox : MonoBehaviour
{
    [Header("Directional Light (Sonne)")]
    [SerializeField] private Light sunLight;

    [Header("Himmel Farben — Sonnenaufgang")]
    [SerializeField] private Color horizonColor  = new Color(0.98f, 0.60f, 0.25f, 1f);
    [SerializeField] private Color skyColor      = new Color(0.15f, 0.20f, 0.45f, 1f);
    [SerializeField] private Color groundColor   = new Color(0.08f, 0.06f, 0.04f, 1f);

    [Header("Atmosphäre")]
    [Range(0f, 5f)] [SerializeField] private float exposure     = 1.4f;
    [Range(0f, 1f)] [SerializeField] private float sunSize      = 0.05f;
    [Range(1f, 10f)][SerializeField] private float sunSoftness  = 2f;

    [Header("Licht")]
    [SerializeField] private Color  sunLightColor     = new Color(1f, 0.85f, 0.60f, 1f);
    [Range(0f, 2f)] [SerializeField] private float sunIntensity = 0.9f;
    [SerializeField] private Vector3 sunRotation      = new Vector3(15f, -20f, 0f);

    [Header("Ambient")]
    [SerializeField] private Color ambientSky     = new Color(0.50f, 0.35f, 0.25f, 1f);
    [SerializeField] private Color ambientEquator = new Color(0.25f, 0.18f, 0.12f, 1f);
    [SerializeField] private Color ambientGround  = new Color(0.08f, 0.06f, 0.04f, 1f);

    private Material skyMat;

    private void OnEnable()   => Apply();
    private void OnValidate() => Apply();

    private void Apply()
    {
        Shader s = Shader.Find("Skybox/Procedural");
        if (s == null)
        {
            Debug.LogError("[Skybox] 'Skybox/Procedural' nicht gefunden.");
            return;
        }

        if (skyMat == null)
            skyMat = new Material(s);

        // Himmel
        skyMat.SetColor("_SkyTint",    horizonColor);
        skyMat.SetColor("_GroundColor", groundColor);
        skyMat.SetFloat("_Exposure",   exposure);
        skyMat.SetFloat("_SunSize",    sunSize);
        skyMat.SetFloat("_SunSizeConvergence", sunSoftness);
        skyMat.SetColor("_AtmosphereThickness",
            new Color(skyColor.r, skyColor.g, skyColor.b, 1f));

        RenderSettings.skybox = skyMat;

        // Ambient
        RenderSettings.ambientMode         = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = ambientSky;
        RenderSettings.ambientEquatorColor = ambientEquator;
        RenderSettings.ambientGroundColor  = ambientGround;

        // Sonne
        if (sunLight != null)
        {
            sunLight.color     = sunLightColor;
            sunLight.intensity = sunIntensity;
            sunLight.transform.rotation = Quaternion.Euler(sunRotation);
        }

        DynamicGI.UpdateEnvironment();
    }
}
