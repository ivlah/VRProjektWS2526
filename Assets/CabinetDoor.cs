using System.Collections;
using UnityEngine;

namespace NavKeypad
{
    public class CabinetDoor : MonoBehaviour
    {
        [Header("Door Settings")]
        [SerializeField] private float openAngle = 110f;      // Wie weit die Tür aufgeht (Grad)
        [SerializeField] private float animationSpeed = 2f;   // Wie schnell sie aufgeht
        [SerializeField] private bool openToLeft = true;      // Öffnungsrichtung

        [Header("Sound (optional)")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioSource audioSource;

        private bool isOpen = false;
        private bool isAnimating = false;
        private Quaternion closedRotation;
        private Quaternion openRotation;

        private void Start()
        {
            closedRotation = transform.localRotation;

            float direction = openToLeft ? -1f : 1f;
            openRotation = Quaternion.Euler(
                transform.localEulerAngles.x,
                transform.localEulerAngles.y + (openAngle * direction),
                transform.localEulerAngles.z
            );
        }

        // Wird per Raycast vom Controller aufgerufen
        public void ToggleDoor()
        {
            if (isAnimating) return;
            Debug.Log("ToggleDoor aufgerufen!"); 
            isOpen = !isOpen;
            StartCoroutine(AnimateDoor(isOpen ? openRotation : closedRotation));

            if (audioSource != null)
            {
                AudioClip clip = isOpen ? openSound : closeSound;
                if (clip != null) audioSource.PlayOneShot(clip);
            }
        }

        private IEnumerator AnimateDoor(Quaternion targetRotation)
        {
            isAnimating = true;
            Quaternion startRotation = transform.localRotation;
            float elapsed = 0f;

            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * animationSpeed;
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsed);
                yield return null;
            }

            transform.localRotation = targetRotation;
            isAnimating = false;
        }
    }
}