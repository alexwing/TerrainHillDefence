using UnityEngine;
using UnityEditor;
using System.IO;

namespace HillDefence.EditorScripts
{
    public static class FixTankMaterialHard
    {
        [MenuItem("Tools/Fix Tank Material Hard")]
        [InitializeOnLoadMethod]
        public static void DoIt()
        {
            if (SessionState.GetBool("FixTankMaterialHard1", false)) return;
            SessionState.SetBool("FixTankMaterialHard1", true);

            string tankPath = "Assets/Resources/Tank.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tankPath) != null)
            {
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
                TeamTank tt = contentsRoot.GetComponent<TeamTank>();
                
                Material teamMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TankTeamMaterial.mat");
                Material barrelMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TankBarrelMaterial.mat");
                
                MeshRenderer[] allRenderers = contentsRoot.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mr in allRenderers)
                {
                    if (mr.gameObject.name.Contains("Barrel"))
                    {
                        mr.sharedMaterial = barrelMat;
                    }
                    else
                    {
                        mr.sharedMaterial = teamMat;
                        if (tt != null && mr.gameObject.name.Contains("Hull"))
                        {
                            tt.towerMaterial = mr; // ensure color applies here
                        }
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                Debug.Log("Fixed Tank Materials HARD!");
            }
        }
    }
}
