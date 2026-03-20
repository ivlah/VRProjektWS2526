using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class PianoInteractable : MonoBehaviour
{
    [Header("Klavier Sound")]
    [SerializeField] private AudioClip pianoClip;

    [Range(0f, 1f)]
    [SerializeField] private float pianoVolume = 1f;

    [Header("Hintergrundmusik")]
    [SerializeField] private AudioSource backgroundMusicSource;

    private AudioSource pianoAudio;
    private bool isPlaying = false;

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

        UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnSelected);
        interactable.activated.AddListener(OnActivated);
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        Debug.Log("[Piano] OnSelected gefeuert");
        if (!isPlaying)
            StartCoroutine(PlayAndResume());
    }

    private void OnActivated(ActivateEventArgs args)
    {
        Debug.Log("[Piano] OnActivated gefeuert");
        if (!isPlaying)
            StartCoroutine(PlayAndResume());
    }

    private void OnDestroy()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSelected);
            interactable.activated.RemoveListener(OnActivated);
        }
    }

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