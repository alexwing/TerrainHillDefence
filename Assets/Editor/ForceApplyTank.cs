using UnityEngine;
using UnityEditor;

namespace HillDefence.EditorScripts
{
    public static class ForceApplyTank
    {
        [MenuItem("Tools/Force Apply Tank")]
        [InitializeOnLoadMethod]
        public static void DoIt()
        {
            if (SessionState.GetBool("TankForced3", false)) return;
            SessionState.SetBool("TankForced3", true);

            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab Tower");
            foreach (string guid in prefabPaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith("Tower.prefab")) continue;

                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(path);
                TeamTower tt = contentsRoot.GetComponent<TeamTower>();
                
                if (tt != null)
                {
                    Debug.Log($"[ForceApplyTank] Modifying {path}");
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
                            Material newMat = new Material(Shader.Find("Legacy Shaders/Diffuse"));
                            mr.sharedMaterial = newMat;
                        }

                        PrefabUtility.SaveAsPrefabAsset(contentsRoot, path);
                        Debug.Log("[ForceApplyTank] Successfully saved prefab!");
                    }
                }
                PrefabUtility.UnloadPrefabContents(contentsRoot);
            }
        }
    }
}
