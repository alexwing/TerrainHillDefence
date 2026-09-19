using UnityEngine;
using UnityEditor;

namespace HillDefence.EditorScripts
{
    public static class FixTankMaterial
    {
        [MenuItem("Tools/Fix Tank Material")]
        [InitializeOnLoadMethod]
        public static void DoIt()
        {
            if (SessionState.GetBool("FixTankMaterial1", false)) return;
            SessionState.SetBool("FixTankMaterial1", true);

            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            // Create or load Team Material
            Material teamMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TankTeamMaterial.mat");
            if (teamMat == null)
            {
                teamMat = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(teamMat, "Assets/Materials/TankTeamMaterial.mat");
            }

            // Create or load Barrel Material
            Material barrelMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TankBarrelMaterial.mat");
            if (barrelMat == null)
            {
                barrelMat = new Material(Shader.Find("Standard"));
                barrelMat.color = Color.gray;
                AssetDatabase.CreateAsset(barrelMat, "Assets/Materials/TankBarrelMaterial.mat");
            }

            string tankPath = "Assets/Resources/Tank.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tankPath) != null)
            {
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
                TeamTank tt = contentsRoot.GetComponent<TeamTank>();
                
                Transform hullMesh = contentsRoot.transform.Find("VisualWrapper/FBX_Root/Hull");
                Transform turretMesh = contentsRoot.transform.Find("VisualWrapper/TurretPivot/Turret");
                Transform barrelMesh = turretMesh != null ? turretMesh.Find("Barrel") : null;
                
                if (hullMesh != null)
                {
                    MeshRenderer mr = hullMesh.GetComponent<MeshRenderer>();
                    if (mr != null) 
                    {
                        mr.sharedMaterial = teamMat;
                        if (tt != null) tt.towerMaterial = mr;
                    }
                }
                
                if (turretMesh != null)
                {
                    MeshRenderer mr = turretMesh.GetComponent<MeshRenderer>();
                    if (mr != null) mr.sharedMaterial = teamMat;
                }
                
                if (barrelMesh != null)
                {
                    MeshRenderer mr = barrelMesh.GetComponent<MeshRenderer>();
                    if (mr != null) mr.sharedMaterial = barrelMat;
                }

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                Debug.Log("Fixed Tank Materials with real Asset files!");
            }
        }
    }
}
