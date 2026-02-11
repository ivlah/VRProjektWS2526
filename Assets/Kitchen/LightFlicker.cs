using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light targetLight;
    public RoleCardManager manager;
    
    public float minIntensity = 0.2f;
    public float maxIntensity = 1.5f;
    public float minFlickerSpeed = 0.02f;
    public float maxFlickerSpeed = 0.3f;
    
    private float baseIntensity;
    private float timer;
    private bool isOff = false;
    
    void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();
        
        baseIntensity = targetLight.intensity;
    }
    
    void Update()
    {
        timer -= Time.deltaTime;
        
        if (timer <= 0)
        {
            // Je weniger Zeit, desto intensiver
            float urgency = 1f;
            if (manager != null)
            {
                urgency = 1f - (manager.timeRemaining / manager.timeLimit);
                urgency = Mathf.Clamp(urgency, 0f, 1f);
            }
            
            // Zufällig komplett aus?
            if (Random.value < urgency * 0.3f)
            {
                isOff = !isOff;
                targetLight.intensity = isOff ? 0f : baseIntensity;
            }
            else
            {
                // Zufällige Intensität - mehr Variation bei wenig Zeit
                float range = maxIntensity - minIntensity;
                float intensity = Random.Range(minIntensity, minIntensity + range * (0.5f + urgency * 0.5f));
                
                // Manchmal kurze helle Spikes
                if (Random.value < 0.1f)
                {
                    intensity = maxIntensity * Random.Range(1f, 1.5f);
                }
                
                targetLight.intensity = baseIntensity * intensity;
            }
            
            // Schnellerer Flicker bei wenig Zeit
            float speed = Mathf.Lerp(maxFlickerSpeed, minFlickerSpeed, urgency);
            timer = Random.Range(speed * 0.5f, speed * 1.5f);
        }
    }
}