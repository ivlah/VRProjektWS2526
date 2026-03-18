using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A Doll's House — AutoSave System
///
/// Speichert automatisch die aktuelle Szene (und optional Position/Rotation).
/// Statics: kann von überall aufgerufen werden ohne Referenz.
///
/// Wie AutoSave auslösen (aus jedem anderen Script):
///     AutoSaveSystem.Save();
///
/// Beim Szenenübergang automatisch speichern:
///     Dieses Script an ein GameObject in der Spielszene hängen und
///     "Auto Save On Scene Load" aktivieren.
/// </summary>
public class AutoSaveSystem : MonoBehaviour
{
    // ---------------------------------------------------------------
    // PlayerPrefs Keys
    // ---------------------------------------------------------------

    private const string KEY_SAVE_EXISTS   = "ADH_SaveExists";
    private const string KEY_SCENE_NAME    = "ADH_SavedScene";
    private const string KEY_POS_X        = "ADH_PosX";
    private const string KEY_POS_Y        = "ADH_PosY";
    private const string KEY_POS_Z        = "ADH_PosZ";
    private const string KEY_ROT_Y        = "ADH_RotY";

    // ---------------------------------------------------------------
    // Inspector
    // ---------------------------------------------------------------

    [Header("Auto Save Einstellungen")]
    [Tooltip("Speichert automatisch wenn diese Szene geladen wird.")]
    [SerializeField] private bool autoSaveOnSceneLoad = true;

    [Tooltip("Optional: Transform des Spielers für Positionsspeicherung.")]
    [SerializeField] private Transform playerTransform;

    [Header("Debug")]
    [SerializeField] private bool logSaveEvents = true;

    // ---------------------------------------------------------------
    // Unity Lifecycle
    // ---------------------------------------------------------------

    private void Start()
    {
        if (autoSaveOnSceneLoad)
            Save(playerTransform);
    }

    // ---------------------------------------------------------------
    // Public Static API
    // ---------------------------------------------------------------

    /// <summary>
    /// Speichert den aktuellen Spielstand (aktive Szene + optionale Position).
    /// Aufruf: AutoSaveSystem.Save();  oder  AutoSaveSystem.Save(playerTransform);
    /// </summary>
    public static void Save(Transform player = null)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        PlayerPrefs.SetInt(KEY_SAVE_EXISTS, 1);
        PlayerPrefs.SetString(KEY_SCENE_NAME, currentScene);

        if (player != null)
        {
            Vector3 pos = player.position;
            PlayerPrefs.SetFloat(KEY_POS_X, pos.x);
            PlayerPrefs.SetFloat(KEY_POS_Y, pos.y);
            PlayerPrefs.SetFloat(KEY_POS_Z, pos.z);
            PlayerPrefs.SetFloat(KEY_ROT_Y, player.eulerAngles.y);
        }

        PlayerPrefs.Save();
        Debug.Log($"[AutoSave] Gespeichert: Szene='{currentScene}'" +
                  (player != null ? $" | Pos={player.position}" : ""));
    }

    /// <summary>Gibt true zurück wenn ein AutoSave existiert.</summary>
    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(KEY_SAVE_EXISTS, 0) == 1;
    }

    /// <summary>Gibt den Szenennamen des letzten Speicherstands zurück. Leer wenn keiner.</summary>
    public static string GetSavedScene()
    {
        if (!HasSave()) return string.Empty;
        return PlayerPrefs.GetString(KEY_SCENE_NAME, string.Empty);
    }

    /// <summary>
    /// Gibt die gespeicherte Spielerposition zurück.
    /// Vector3.zero wenn keine Positionsdaten gespeichert.
    /// </summary>
    public static Vector3 GetSavedPosition()
    {
        if (!HasSave()) return Vector3.zero;
        return new Vector3(
            PlayerPrefs.GetFloat(KEY_POS_X, 0f),
            PlayerPrefs.GetFloat(KEY_POS_Y, 0f),
            PlayerPrefs.GetFloat(KEY_POS_Z, 0f)
        );
    }

    /// <summary>Gibt die gespeicherte Y-Rotation zurück.</summary>
    public static float GetSavedRotationY()
    {
        return PlayerPrefs.GetFloat(KEY_ROT_Y, 0f);
    }

    /// <summary>
    /// Löscht den gesamten Speicherstand (wird bei "Start New Game" aufgerufen).
    /// </summary>
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(KEY_SAVE_EXISTS);
        PlayerPrefs.DeleteKey(KEY_SCENE_NAME);
        PlayerPrefs.DeleteKey(KEY_POS_X);
        PlayerPrefs.DeleteKey(KEY_POS_Y);
        PlayerPrefs.DeleteKey(KEY_POS_Z);
        PlayerPrefs.DeleteKey(KEY_ROT_Y);
        PlayerPrefs.Save();
        Debug.Log("[AutoSave] Speicherstand gelöscht.");
    }

    // ---------------------------------------------------------------
    // Position wiederherstellen (optional)
    // ---------------------------------------------------------------

    /// <summary>
    /// Stellt die gespeicherte Spielerposition wieder her.
    /// Aufruf in der Spielszene nach dem Laden:
    ///     AutoSaveSystem.RestorePosition(xrOriginTransform);
    /// </summary>
    public static void RestorePosition(Transform target)
    {
        if (target == null || !HasSave()) return;

        Vector3 savedPos = GetSavedPosition();
        float savedRotY  = GetSavedRotationY();

        if (savedPos == Vector3.zero) return; // keine Positionsdaten vorhanden

        target.position = savedPos;
        target.rotation = Quaternion.Euler(0f, savedRotY, 0f);

        Debug.Log($"[AutoSave] Position wiederhergestellt: {savedPos}");
    }
}
