using UnityEngine;
using Unity.XR.CoreUtils;

/// <summary>
/// A Doll's House — Room Loader
///
/// Dieses Script erkennt ob die Szene per "Continue" geladen wurde
/// und stellt dann die gespeicherte Spielerposition wieder her.
///
/// SETUP PRO SZENE (Room1, Room2, Room3):
///   1. Dasselbe GameObject wie AutoSaveController nehmen
///      ODER ein neues "RoomLoader" GameObject erstellen
///   2. Dieses Script dranhängen
///   3. XR Origin ins "XR Origin" Feld ziehen
///
/// Reihenfolge auf dem GameObject spielt keine Rolle —
/// RoomLoader wartet automatisch bis AutoSave fertig ist.
/// </summary>
public class RoomLoader : MonoBehaviour
{
    [Header("XR Origin")]
    [Tooltip("XR Origin GameObject — Spielerposition wird hierauf angewendet.")]
    [SerializeField] private XROrigin xrOrigin;

    [Header("Optionen")]
    [Tooltip("Position nur wiederherstellen wenn via Continue geladen.")]
    [SerializeField] private bool onlyRestoreOnContinue = true;

    [Tooltip("Debug Output in der Console.")]
    [SerializeField] private bool debugLog = true;

    // ---------------------------------------------------------------
    // Static Flag — wird vom MainMenuManager gesetzt
    // ---------------------------------------------------------------

    /// <summary>
    /// Wird vom MainMenuManager.OnContinue() auf true gesetzt
    /// bevor die Szene geladen wird.
    /// Bleibt über Szenenwechsel erhalten (static).
    /// </summary>
    public static bool LoadedViaContinue = false;

    // ---------------------------------------------------------------

    private void Start()
    {
        // XR Origin automatisch suchen wenn nicht zugewiesen
        if (xrOrigin == null)
            xrOrigin = FindObjectOfType<XROrigin>();

        if (xrOrigin == null)
        {
            Debug.LogError("[RoomLoader] XR Origin nicht gefunden! Bitte im Inspector zuweisen.");
            return;
        }

        bool shouldRestore = onlyRestoreOnContinue ? LoadedViaContinue : AutoSaveSystem.HasSave();

        if (shouldRestore)
        {
            RestorePosition();
            LoadedViaContinue = false; // Flag zurücksetzen
        }
        else
        {
            if (debugLog)
                Debug.Log("[RoomLoader] Neue Szene — Position nicht wiederhergestellt.");
        }
    }

    // ---------------------------------------------------------------

    private void RestorePosition()
    {
        Vector3 savedPos = AutoSaveSystem.GetSavedPosition();
        float   savedRot = AutoSaveSystem.GetSavedRotationY();

        // Vector3.zero bedeutet: keine Positionsdaten gespeichert → überspringen
        if (savedPos == Vector3.zero)
        {
            if (debugLog)
                Debug.Log("[RoomLoader] Keine gespeicherte Position — Spieler bleibt am Spawn.");
            return;
        }

        // XR Origin verschieben (bewegt den ganzen Rig inkl. Kamera)
        xrOrigin.transform.position = savedPos;
        xrOrigin.transform.rotation = Quaternion.Euler(0f, savedRot, 0f);

        if (debugLog)
            Debug.Log($"[RoomLoader] ✓ Position wiederhergestellt → {savedPos} | Rot Y: {savedRot}°");
    }
}
