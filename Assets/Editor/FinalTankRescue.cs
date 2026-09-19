using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace HillDefence.EditorScripts
{
    public static class FinalTankRescue
    {
        [MenuItem("Tools/Final Tank Rescue")]
        [InitializeOnLoadMethod]
        public static void DoIt()
        {
            if (SessionState.GetBool("FinalTankRescue1", false)) return;
            SessionState.SetBool("FinalTankRescue1", true);

            string tankPath = "Assets/Resources/Tank.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tankPath) != null)
            {
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
                
                // 1. Clean up everything visual
                List<GameObject> toDestroy = new List<GameObject>();
                foreach (Transform child in contentsRoot.transform)
                {
                    if (child.name != "TankVisual" && child.name != "TurretPivot" && child.name != "VisualWrapper") 
                        continue;
                    toDestroy.Add(child.gameObject);
                }
                foreach (var obj in toDestroy) GameObject.DestroyImmediate(obj, true);

                // 2. Load the FBX fresh
                GameObject tankModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/TankSketchfab.fbx");
                GameObject fbxInst = (GameObject)PrefabUtility.InstantiatePrefab(tankModel);
                
                // 3. Create a clean VisualWrapper
                GameObject visualWrapper = new GameObject("VisualWrapper");
                visualWrapper.transform.SetParent(contentsRoot.transform, false);
                visualWrapper.transform.localScale = Vector3.one * 0.75f;
                
                // 4. Put FBX inside Wrapper
                fbxInst.transform.SetParent(visualWrapper.transform, false);
                fbxInst.name = "FBX_Root";
                
                // Since the tank is modeled along X, we rotate the FBX by -90 on Y so X aligns with Z (forward)
                fbxInst.transform.localRotation = Quaternion.Euler(0, -90, 0);

                TeamTank tt = contentsRoot.GetComponent<TeamTank>();
                
                // 5. Setup Materials
                Material stdMat = new Material(Shader.Find("Standard"));
                Material barrelMat = new Material(Shader.Find("Standard"));
                barrelMat.color = Color.gray;
                
                Transform hullMesh = fbxInst.transform.Find("Hull");
                Transform turretMesh = hullMesh != null ? hullMesh.Find("Turret") : fbxInst.transform.Find("Turret");
                Transform barrelMesh = turretMesh != null ? turretMesh.Find("Barrel") : null;
                
                if (hullMesh != null)
                {
                    MeshRenderer mr = hullMesh.GetComponent<MeshRenderer>();
                    if (mr != null) 
                    {
                        mr.sharedMaterial = stdMat;
                        if (tt != null) tt.towerMaterial = mr;
                    }
                }
                
                if (turretMesh != null)
                {
                    MeshRenderer mr = turretMesh.GetComponent<MeshRenderer>();
                    if (mr != null) mr.sharedMaterial = stdMat;
                }
                
                if (barrelMesh != null)
                {
                    MeshRenderer mr = barrelMesh.GetComponent<MeshRenderer>();
                    if (mr != null) mr.sharedMaterial = barrelMat;
                }

                // 6. Fix Turret Rotation Logic
                if (tt != null)
                {
                    if (turretMesh != null)
                    {
                        // We need a Pivot that has identity rotation, placed exactly at the turret's world position
                        GameObject turretPivot = new GameObject("TurretPivot");
                        turretPivot.transform.SetParent(visualWrapper.transform, false);
                        turretPivot.transform.position = turretMesh.position;
                        
                        // Move the turretMesh inside this new Pivot
                        turretMesh.SetParent(turretPivot.transform, true); // Keep world transform exactly as imported!
                        
                        tt.tower = turretPivot;
                        
                        // Setup ShootPos
                        Transform shootPos = turretPivot.transform.Find("ShootPos");
                        if (shootPos == null)
                        {
                            GameObject sp = new GameObject("ShootPos");
                            sp.transform.SetParent(turretPivot.transform, false);
                            // Set it forward (Z) relative to the pivot, slightly up
                            sp.transform.localPosition = new Vector3(0, 0.4f, 4.0f);
                            shootPos = sp.transform;
                        }
                        tt.shootInitPosition = shootPos.gameObject;
                    }
                    else
                    {
                        tt.tower = visualWrapper;
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                Debug.Log("Final Tank Rescue Complete! All transforms preserved from FBX, wrapper rotated, pivot isolated.");
            }
        }
    }
}
