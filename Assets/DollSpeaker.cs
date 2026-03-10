using System.Collections;
using UnityEngine;

namespace NavKeypad
{
    public class DollSpeaker : MonoBehaviour
    {
        [Header("Audio Clips")]
        [Tooltip("Die Audioklips die diese Puppe abspielen kann")]
        [SerializeField] private AudioClip[] speechClips;

        [Header("Component References")]
        [SerializeField] private AudioSource audioSource;

        [Header("Settings")]
        [SerializeField] private float cooldownBetweenLines = 5f; // Sekunden zwischen Aussagen

        private int currentClipIndex = 0;
        private float lastSpokenTime = -999f;

        // Wird von außen aufgerufen (z.B. vom Brief oder Keypad)
        public void Speak()
        {
            if (speechClips == null || speechClips.Length == 0) return;
            if (Time.time - lastSpokenTime < cooldownBetweenLines) return;
            if (audioSource.isPlaying) return;

            audioSource.clip = speechClips[currentClipIndex];
            audioSource.Play();

            lastSpokenTime = Time.time;

            // Nächsten Satz für nächstes Mal vorbereiten
            currentClipIndex = (currentClipIndex + 1) % speechClips.Length;
        }
    }
}