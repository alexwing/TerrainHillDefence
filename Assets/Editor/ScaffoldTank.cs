using UnityEngine;
using UnityEditor;

namespace HillDefence.EditorScripts
{
    public static class ScaffoldTank
    {
        [MenuItem("Tools/Scaffold Tank")]
        [InitializeOnLoadMethod]
        public static void DoIt()
        {
            if (SessionState.GetBool("TankScaffolded", false)) return;
            SessionState.SetBool("TankScaffolded", true);

            string towerPath = "" ;
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab Tower");
            foreach (string guid in prefabPaths) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("Tower.prefab")) towerPath = path;
            }

            if (string.IsNullOrEmpty(towerPath)) return;

            string tankPath = "Assets/Prefabs/Tank.prefab";
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(tankPath))
            {
                AssetDatabase.CopyAsset(towerPath, tankPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
                TeamTower tt = contentsRoot.GetComponent<TeamTower>();
                if (tt != null)
                {
                    GameObject tankModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Tank.obj");
                    if (tt.tower != null) {
                        GameObject.DestroyImmediate(tt.tower, true);
                    }

                    GameObject newVisual = (GameObject)PrefabUtility.InstantiatePrefab(tankModel);
                    newVisual.transform.SetParent(contentsRoot.transform, false);
                    newVisual.transform.localPosition = Vector3.zero;
                    newVisual.transform.localRotation = Quaternion.identity;
                    newVisual.transform.localScale = Vector3.one * 0.35f;
                    newVisual.name = "TankVisual";

                    tt.tower = newVisual;
                    MeshRenderer mr = newVisual.GetComponentInChildren<MeshRenderer>();
                    if (mr != null) {
                        tt.towerMaterial = mr;
                        mr.sharedMaterial = new Material(Shader.Find("Legacy Shaders/Diffuse"));
                    }

                    PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                    Debug.Log("[ScaffoldTank] Created Tank.prefab!");
                }
                PrefabUtility.UnloadPrefabContents(contentsRoot);
            }
        }
    }
}
