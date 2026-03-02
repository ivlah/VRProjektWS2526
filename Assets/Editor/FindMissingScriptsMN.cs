#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class FindMissingScriptsMN
{
    [MenuItem("Tools/Room1/Find Missing Scripts (Scene)")]
    public static void FindMissingInScene()
    {
        int missingCount = 0;
        int gameObjectCount = 0;

        var all = Object.FindObjectsOfType<GameObject>(true);
        foreach (var go in all)
        {
            gameObjectCount++;

            var components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    missingCount++;
                    Debug.LogWarning($"[MissingScript] GameObject='{GetFullPath(go)}' has a missing script component.", go);
                }
            }
        }

        Debug.Log($"[FindMissingScriptsMN] Scanned {gameObjectCount} GameObjects. Missing components found: {missingCount}");
    }

    [MenuItem("Tools/Room1/Remove Missing Scripts (Selected)")]
    public static void RemoveMissingOnSelected()
    {
        var selection = Selection.gameObjects;
        if (selection == null || selection.Length == 0)
        {
            Debug.LogWarning("[FindMissingScriptsMN] No GameObjects selected.");
            return;
        }

        int removed = 0;

        foreach (var go in selection)
        {
            var comps = go.GetComponents<Component>();
            for (int i = comps.Length - 1; i >= 0; i--)
            {
                if (comps[i] == null)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    removed++;
                    break;
                }
            }
        }

        Debug.Log($"[FindMissingScriptsMN] Removed missing scripts on selected objects. Affected objects: {removed}");
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
#endif
