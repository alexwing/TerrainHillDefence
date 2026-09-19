using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace HillDefence.EditorScripts
{
    [InitializeOnLoad]
    public class MissingScriptCleaner
    {
        static MissingScriptCleaner()
        {
            EditorApplication.delayCall += CleanUp;
        }

        static void CleanUp()
        {
            if (SessionState.GetBool("MissingScriptCleaned_v1", false))
                return;

            SessionState.SetBool("MissingScriptCleaned_v1", true);

            int totalRemoved = 0;
            
            // 1. Clean Scene
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            foreach (var go in rootObjects)
            {
                totalRemoved += CleanGameObject(go);
            }

            // 2. Clean Prefabs
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in prefabPaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    int removed = CleanGameObject(prefab);
                    if (removed > 0)
                    {
                        totalRemoved += removed;
                        PrefabUtility.SavePrefabAsset(prefab);
                        Debug.Log($"Cleaned {removed} missing scripts from prefab: {path}");
                    }
                }
            }

            if (totalRemoved > 0)
            {
                Debug.Log($"[MissingScriptCleaner] Successfully removed {totalRemoved} missing scripts from the project!");
            }
        }

        static int CleanGameObject(GameObject go)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            foreach (Transform child in go.transform)
            {
                removed += CleanGameObject(child.gameObject);
            }
            return removed;
        }
    }
}
