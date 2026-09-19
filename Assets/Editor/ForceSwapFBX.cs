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
            if (SessionState.GetBool("ForceSwapFBX3", false)) return;
            SessionState.SetBool("ForceSwapFBX3", true);

            string tankPath = "Assets/Resources/Tank.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tankPath) != null)
            {
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
                
                // Destroy ANY existing TankVisual (the old obj)
                Transform oldVis = contentsRoot.transform.Find("TankVisual");
                if (oldVis != null)
                {
                    GameObject.DestroyImmediate(oldVis.gameObject, true);
                }

                // Instantiate new FBX
                GameObject tankModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/TankSketchfab.fbx");
                GameObject newVisual = (GameObject)PrefabUtility.InstantiatePrefab(tankModel);
                newVisual.transform.SetParent(contentsRoot.transform, false);
                newVisual.transform.localPosition = Vector3.zero;
                newVisual.transform.localRotation = Quaternion.identity;
                
                // Scale it appropriately (the Sketchfab T90 is 10m long, scale 0.5f makes it 5m)
                newVisual.transform.localScale = Vector3.one * 0.5f;
                newVisual.name = "TankVisual";

                TeamTank tt = contentsRoot.GetComponent<TeamTank>();
                if (tt != null)
                {
                    Transform turretTransform = newVisual.transform.Find("Turret");
                    if (turretTransform != null)
                    {
                        tt.tower = turretTransform.gameObject;
                    }
                    else
                    {
                        tt.tower = newVisual; // fallback
                    }
                    
                    // Assign material (from Hull or Turret)
                    MeshRenderer mr = newVisual.GetComponentInChildren<MeshRenderer>();
                    if (mr != null) tt.towerMaterial = mr;

                    // Recreate ShootPos
                    Transform shootPos = newVisual.transform.Find("ShootPos");
                    if (shootPos == null)
                    {
                        GameObject sp = new GameObject("ShootPos");
                        sp.transform.SetParent(turretTransform != null ? turretTransform : newVisual.transform, false);
                        
                        // If it's on the turret, we just move it forward (along Z) and up (along Y)
                        sp.transform.localPosition = new Vector3(0, 0f, 4.0f);
                        shootPos = sp.transform;
                    }
                    tt.shootInitPosition = shootPos.gameObject;
                }

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                Debug.Log("Swapped to the new separated Turret FBX model!");
            }
        }
    }
}
