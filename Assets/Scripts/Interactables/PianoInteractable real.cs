using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// A Doll's House — Piano Interactable (v4)
/// Spielt wenn Trigger-Button gedrückt wird während man auf das Klavier zeigt.
/// Nutzt direkten XR Input — kein Collider Trigger nötig.
///
/// SETUP:
///   1. Box Collider auf dem Klavier → Is Trigger ✓
///   2. Audio Source auf dem Klavier
///   3. Dieses Script dranhängen
///   4. Piano Clip reinziehen
///   5. Background Music Source → MusicController Audio Source reinziehen
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PianoInteractable : MonoBehaviour
{
    [Header("Klavier Sound")]
    [SerializeField] private AudioClip pianoClip;

    [Range(0f, 1f)]
    [SerializeField] private float pianoVolume = 1f;

    [Header("Hintergrundmusik")]
    [SerializeField] private AudioSource backgroundMusicSource;

    // ---------------------------------------------------------------

    private AudioSource pianoAudio;
    private bool isPlaying = false;
    private bool controllerInside = false;

    // XR Input Devices
    private InputDevice rightController;
    private InputDevice leftController;
    private bool wasPressed = false;

    // ---------------------------------------------------------------

    private void Start()
    {
        pianoAudio             = GetComponent<AudioSource>();
        pianoAudio.clip        = pianoClip;
        pianoAudio.playOnAwake = false;
        pianoAudio.loop        = false;
        pianoAudio.volume      = pianoVolume;

        if (backgroundMusicSource == null)
        {
            GameObject musicObj = GameObject.Find("MusicController");
            if (musicObj != null)
                backgroundMusicSource = musicObj.GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (isPlaying) return;

        // Controller bei Bedarf neu suchen
        if (!rightController.isValid)
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (!leftController.isValid)
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        // Trigger ODER Grip auf beiden Controllern prüfen
        bool pressed = IsPressed(rightController) || IsPressed(leftController);

        // Nur beim ersten Drücken auslösen (kein Dauerfeuer)
        if (pressed && !wasPressed && controllerInside)
            StartCoroutine(PlayAndResume());

        wasPressed = pressed;
    }

    private bool IsPressed(InputDevice device)
    {
        if (!device.isValid) return false;

        // Trigger Button
        device.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger);
        // Grip Button
        device.TryGetFeatureValue(CommonUsages.gripButton, out bool grip);
        // Primary Button (A/X)
        device.TryGetFeatureValue(CommonUsages.primaryButton, out bool primary);
        // Secondary Button (B/Y)
        device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondary);

        return trigger || grip || primary || secondary;
    }

    // ---------------------------------------------------------------
    // Collider Trigger
    // ---------------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        controllerInside = true;
        Debug.Log($"[Piano] Controller erkannt: {other.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        controllerInside = false;
    }

    // ---------------------------------------------------------------
    // Pause → Play → Resume
    // ---------------------------------------------------------------

    private System.Collections.IEnumerator PlayAndResume()
    {
        isPlaying = true;

        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
            backgroundMusicSource.Pause();

        pianoAudio.Play();
        Debug.Log($"[Piano] ♪ Spiele '{pianoClip.name}'");

        yield return new WaitForSeconds(pianoClip.length);

        if (backgroundMusicSource != null)
            backgroundMusicSource.UnPause();

        Debug.Log("[Piano] Hintergrundmusik fortgesetzt.");
        isPlaying = false;
    }
}
