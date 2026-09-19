using UnityEngine;
using UnityEditor;

namespace HillDefence.EditorScripts
{
    public static class FixTankResources
    {
        [MenuItem("Tools/Fix Tank")]
        [InitializeOnLoadMethod]
        public static void DoIt()
        {
            if (SessionState.GetBool("TankFixed", false)) return;
            SessionState.SetBool("TankFixed", true);

            if (!AssetDatabase.IsValidFolder("Assets/Resources")) {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string towerPath = "" ;
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab Tower");
            foreach (string guid in prefabPaths) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("Tower.prefab")) towerPath = path;
            }

            if (string.IsNullOrEmpty(towerPath)) return;

            string tankPath = "Assets/Resources/Tank.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tankPath) == null) {
                AssetDatabase.CopyAsset(towerPath, tankPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
            TeamTower tt = contentsRoot.GetComponent<TeamTower>();
            if (tt != null)
            {
                GameObject tankModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Tank.obj");
                if (tt.tower != null && tt.tower.name != "TankVisual") {
                    GameObject.DestroyImmediate(tt.tower, true);
                }

                if (contentsRoot.transform.Find("TankVisual") == null) {
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
                    Debug.Log("[FixTank] Successfully created Tank.prefab in Resources!");
                }
            }
            PrefabUtility.UnloadPrefabContents(contentsRoot);
        }
    }
}
