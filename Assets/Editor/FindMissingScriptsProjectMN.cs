#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class FindMissingScriptsProjectMN
{
    [MenuItem("Tools/Room1/Find Missing Scripts (Project Prefabs)")]
    public static void FindMissingInProjectPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int prefabCount = 0;
        int missingCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            prefabCount++;

            var allGos = prefab.GetComponentsInChildren<Transform>(true);
            foreach (var t in allGos)
            {
                var comps = t.gameObject.GetComponents<Component>();
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i] == null)
                    {
                        missingCount++;
                        Debug.LogWarning($"[MissingScript-Prefab] Prefab='{path}' GameObject='{GetFullPath(t.gameObject)}' has a missing script.", prefab);
                    }
                }
            }
        }

        Debug.Log($"[FindMissingScriptsProjectMN] Scanned {prefabCount} prefabs. Missing components found: {missingCount}");
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