using UnityEngine;

/// <summary>
/// A Doll's House — Sunrise Skybox
/// Attach to any GameObject in your Menu scene.
/// Nutzt Unity's built-in "Skybox/Procedural" Shader.
/// Kein externes Asset erforderlich.
/// </summary>
[ExecuteAlways]
public class SunriseSkybox : MonoBehaviour
{
    [Header("Sky Farben")]
    [SerializeField] private Color skyHorizon = new Color(0.85f, 0.38f, 0.15f, 1f);
    [SerializeField] private Color skyGround  = new Color(0.12f, 0.09f, 0.06f, 1f);
    [SerializeField] private Color skyTop     = new Color(0.05f, 0.05f, 0.18f, 1f);

    [Header("Sonne")]
    [SerializeField] [Range(0f, 1f)]  private float sunSize     = 0.04f;
    [SerializeField] [Range(0f, 10f)] private float sunSoftness = 0.5f;
    [SerializeField] [Range(0f, 8f)]  private float exposure    = 1.3f;

    [Header("Ambient Light")]
    [SerializeField] private Color ambientSky     = new Color(0.55f, 0.35f, 0.22f, 1f);
    [SerializeField] private Color ambientEquator = new Color(0.30f, 0.22f, 0.18f, 1f);
    [SerializeField] private Color ambientGround  = new Color(0.10f, 0.08f, 0.06f, 1f);

    [Header("Directional Light (Sonne)")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Color sunLightColor    = new Color(1.0f, 0.78f, 0.45f, 1f);
    [SerializeField] [Range(0f, 3f)] private float sunLightIntensity = 1.1f;
    [SerializeField] private Vector3 sunRotation    = new Vector3(12f, -30f, 0f);

    private Material skyMat;

    private void OnEnable()   => Apply();
    private void OnValidate() => Apply();

    private void Apply()
    {
        if (skyMat == null)
        {
            Shader s = Shader.Find("Skybox/Procedural");
            if (s == null) { Debug.LogError("[SunriseSkybox] Shader 'Skybox/Procedural' nicht gefunden."); return; }
            skyMat = new Material(s);
        }

        skyMat.SetColor("_SkyTint",    skyHorizon);
        skyMat.SetColor("_GroundColor", skyGround);
        skyMat.SetFloat("_Exposure",   exposure);
        skyMat.SetFloat("_SunSize",    sunSize);
        skyMat.SetFloat("_SunSizeConvergence", sunSoftness);
        skyMat.SetColor("_AtmosphereThickness", new Color(skyTop.r, skyTop.g, skyTop.b, 1f));

        RenderSettings.skybox = skyMat;
        RenderSettings.ambientMode         = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = ambientSky;
        RenderSettings.ambientEquatorColor = ambientEquator;
        RenderSettings.ambientGroundColor  = ambientGround;

        if (sunLight != null)
        {
            sunLight.color     = sunLightColor;
            sunLight.intensity = sunLightIntensity;
            sunLight.transform.rotation = Quaternion.Euler(sunRotation);
        }

        DynamicGI.UpdateEnvironment();
    }
}
