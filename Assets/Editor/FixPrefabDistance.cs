using UnityEngine;
using UnityEditor;

namespace HillDefence.EditorScripts
{
    [InitializeOnLoad]
    public class FixPrefabDistance
    {
        static FixPrefabDistance()
        {
            EditorApplication.delayCall += FixPrefabs;
        }

        static void FixPrefabs()
        {
            if (SessionState.GetBool("PrefabDistanceFixed", false))
                return;

            SessionState.SetBool("PrefabDistanceFixed", true);

            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab");
            int fixedCount = 0;
            foreach (string guid in prefabPaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    HealthLayout hl = prefab.GetComponentInChildren<HealthLayout>(true);
                    if (hl != null && hl.maxVisibleDistance != 500f)
                    {
                        hl.maxVisibleDistance = 500f;
                        EditorUtility.SetDirty(hl);
                        PrefabUtility.SavePrefabAsset(prefab);
                        fixedCount++;
                        Debug.Log($"Fixed maxVisibleDistance on {path}");
                    }
                }
            }
            if (fixedCount > 0)
            {
                Debug.Log($"[FixPrefabDistance] Fixed {fixedCount} prefabs to have maxVisibleDistance = 500f.");
            }
        }
    }
}
