using UnityEngine;
using UnityEditor;

namespace HillDefence.EditorScripts
{
    public static class ForceSwapFBX
    {
        [MenuItem("Tools/Force Swap FBX")]
        [InitializeOnLoadMethod]
        public static void DoIt()
        {
            if (SessionState.GetBool("ForceSwapFBX7", false)) return;
            SessionState.SetBool("ForceSwapFBX7", true);

            string tankPath = "Assets/Resources/Tank.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tankPath) != null)
            {
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
                
                // Destroy old visual
                Transform oldVis = contentsRoot.transform.Find("TankVisual");
                if (oldVis != null)
                {
                    GameObject.DestroyImmediate(oldVis.gameObject, true);
                }

                GameObject tankModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/TankSketchfab.fbx");
                GameObject newVisual = (GameObject)PrefabUtility.InstantiatePrefab(tankModel);
                newVisual.transform.SetParent(contentsRoot.transform, false);
                
                // NO HACKS! Direct import scale and zero rotation!
                newVisual.transform.localPosition = Vector3.zero;
                newVisual.transform.localRotation = Quaternion.identity;
                newVisual.transform.localScale = Vector3.one * 0.75f;
                newVisual.name = "TankVisual";

                TeamTank tt = contentsRoot.GetComponent<TeamTank>();
                if (tt != null)
                {
                    // Find the turret (now perfectly structured in Blender)
                    Transform turretMesh = newVisual.transform.Find("Turret");
                    if (turretMesh != null)
                    {
                        tt.tower = turretMesh.gameObject;
                    }
                    else
                    {
                        tt.tower = newVisual; 
                    }
                    
                    // Assign material (from Hull)
                    Transform hullMesh = newVisual.transform.Find("Hull");
                    if (hullMesh != null)
                    {
                        MeshRenderer mr = hullMesh.GetComponent<MeshRenderer>();
                        if (mr != null) tt.towerMaterial = mr;
                    }
                    
                    if (tt.towerMaterial == null)
                    {
                        MeshRenderer mr = newVisual.GetComponentInChildren<MeshRenderer>();
                        if (mr != null) tt.towerMaterial = mr;
                    }

                    Transform shootPos = newVisual.transform.Find("ShootPos");
                    if (shootPos == null)
                    {
                        GameObject sp = new GameObject("ShootPos");
                        sp.transform.SetParent(tt.tower.transform, false);
                        
                        // ShootPos at the tip of the barrel
                        // Since the tank is nicely oriented to Z, we just move it +Z and +Y
                        sp.transform.localPosition = new Vector3(0, 0.4f, 4.0f);
                        shootPos = sp.transform;
                    }
                    tt.shootInitPosition = shootPos.gameObject;
                }

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                Debug.Log("Swapped with PERFECT Blender FBX!");
            }
        }
    }
}
