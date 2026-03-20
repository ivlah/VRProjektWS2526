using UnityEngine;

/// <summary>
/// A Doll's House — Menu Follow Camera
///
/// Keeps the main menu positioned in front of the player.
/// The menu follows the viewing direction smoothly (lazy follow)
/// so it does not feel too aggressive.
///
/// SETUP:
///   1. Attach this script to the "MainMenu_Root" GameObject
///   2. Assign the XR camera (XR Origin → Camera Offset → Main Camera)
///   3. Done — the menu appears in front of the player on start
/// </summary>
public class MenuFollowCamera : MonoBehaviour
{
    [Header("Camera Reference")]
    [Tooltip("Main Camera from XR Origin → Camera Offset → Main Camera")]
    [SerializeField] private Transform xrCamera;

    [Header("Position in Front of Player")]
    [Tooltip("How far the menu floats in front of the player (meters).")]
    [SerializeField] private float distanceFromPlayer = 2f;

    [Tooltip("Vertical offset relative to the camera (0 = eye level).")]
    [SerializeField] private float heightOffset = -0.1f;

    [Header("Smooth Following")]
    [Tooltip("How quickly the menu follows the view direction. 0 = never, 1 = instantly.")]
    [Range(0f, 1f)]
    [SerializeField] private float followSpeed = 0.05f;

    [Tooltip("Only follow if the player looks away more than this angle.")]
    [SerializeField] private float followDeadzone = 30f;

    private bool initialized = false;

    private void Start()
    {
        if (xrCamera == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
                xrCamera = cam.transform;
            else
            {
                Debug.LogError("[MenuFollow] No Main Camera found!");
                return;
            }
        }

        SnapToCamera();
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || xrCamera == null) return;

        Vector3 toMenu = (transform.position - xrCamera.position).normalized;
        float angle = Vector3.Angle(xrCamera.forward, toMenu);

        if (angle > followDeadzone)
        {
            Vector3 targetPos = GetTargetPosition();
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed);
        }

        Vector3 lookDir = transform.position - xrCamera.position;
        lookDir.y = 0f;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }

    private void SnapToCamera()
    {
        transform.position = GetTargetPosition();

        Vector3 lookDir = transform.position - xrCamera.position;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 forward = xrCamera.forward;
        forward.y = 0f;
        forward.Normalize();

        return xrCamera.position
               + forward * distanceFromPlayer
               + Vector3.up * heightOffset;
    }
}