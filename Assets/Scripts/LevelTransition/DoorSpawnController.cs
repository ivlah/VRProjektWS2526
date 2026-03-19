using UnityEngine;
using Unity.XR.CoreUtils;

/// <summary>
/// A Doll's House — Door Spawn Controller
///
/// Spawnt den Spieler an der Eingangstür wenn er aus dem vorherigen Raum kommt.
///
/// SETUP in Room2, Room3, Credits — an der Eingangstür:
///   1. Leeres GameObject vor der Eingangstür → "SpawnPoint"
///   2. Rotation so setzen dass Spieler mit Blick in den Raum steht
///   3. Dieses Script dranhängen
///   4. XR Origin reinziehen (oder wird automatisch gefunden)
/// </summary>
public class DoorSpawnController : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;

    private void Start()
    {
        if (xrOrigin == null)
            xrOrigin = FindObjectOfType<XROrigin>();

        if (SceneTransitionData.SpawnAtDoor)
        {
            if (xrOrigin != null)
            {
                xrOrigin.transform.position = transform.position;
                xrOrigin.transform.rotation = transform.rotation;
                Debug.Log($"[DoorSpawn] Gespawnt bei: {transform.position}");
            }
            SceneTransitionData.SpawnAtDoor = false;
        }
    }
}
