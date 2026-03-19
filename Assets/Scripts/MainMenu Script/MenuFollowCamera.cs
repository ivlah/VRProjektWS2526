using UnityEngine;

/// <summary>
/// A Doll's House — Menu Follow Camera
///
/// Platziert das Main Menu immer vor dem Spieler.
/// Das Menu folgt sanft der Blickrichtung (Lazy Follow)
/// damit es nicht zu aufdringlich wirkt.
///
/// SETUP:
///   1. Dieses Script auf das "MainMenu_Root" GameObject
///   2. XR Camera reinziehen (XR Origin → Camera Offset → Main Camera)
///   3. Fertig — Menu erscheint beim Start automatisch vor dem Spieler
/// </summary>
public class MenuFollowCamera : MonoBehaviour
{
    [Header("Kamera Referenz")]
    [Tooltip("Main Camera vom XR Origin → Camera Offset → Main Camera")]
    [SerializeField] private Transform xrCamera;

    [Header("Position vor dem Spieler")]
    [Tooltip("Wie weit das Menu vor dem Spieler schwebt (Meter).")]
    [SerializeField] private float distanceFromPlayer = 2f;

    [Tooltip("Höhe relativ zur Kamera (0 = Augenhöhe).")]
    [SerializeField] private float heightOffset = -0.1f;

    [Header("Sanftes Folgen")]
    [Tooltip("Wie schnell das Menu der Blickrichtung folgt. 0 = gar nicht, 1 = sofort.")]
    [Range(0f, 1f)]
    [SerializeField] private float followSpeed = 0.05f;

    [Tooltip("Erst folgen wenn Spieler X Grad weggeschaut hat.")]
    [SerializeField] private float followDeadzone = 30f;

    // ---------------------------------------------------------------

    private bool initialized = false;

    private void Start()
    {
        // Kamera automatisch suchen falls nicht zugewiesen
        if (xrCamera == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
                xrCamera = cam.transform;
            else
            {
                Debug.LogError("[MenuFollow] Keine Main Camera gefunden!");
                return;
            }
        }

        // Menu sofort vor dem Spieler platzieren beim Start
        SnapToCamera();
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || xrCamera == null) return;

        // Winkel zwischen Menu und aktueller Blickrichtung berechnen
        Vector3 toMenu = (transform.position - xrCamera.position).normalized;
        float angle    = Vector3.Angle(xrCamera.forward, toMenu);

        // Nur folgen wenn Spieler zu weit weggeschaut hat
        if (angle > followDeadzone)
        {
            Vector3 targetPos = GetTargetPosition();
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed);
        }

        // Menu immer zum Spieler ausrichten
        Vector3 lookDir   = transform.position - xrCamera.position;
        lookDir.y         = 0f; // nicht kippen
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }

    // ---------------------------------------------------------------

    private void SnapToCamera()
    {
        transform.position = GetTargetPosition();

        // Menu schaut den Spieler an
        Vector3 lookDir = transform.position - xrCamera.position;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 forward    = xrCamera.forward;
        forward.y          = 0f;
        forward.Normalize();

        return xrCamera.position
               + forward * distanceFromPlayer
               + Vector3.up * heightOffset;
    }
}
