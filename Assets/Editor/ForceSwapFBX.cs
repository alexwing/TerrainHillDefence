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
            if (SessionState.GetBool("ForceSwapFBX5", false)) return;
            SessionState.SetBool("ForceSwapFBX5", true);

            string tankPath = "Assets/Resources/Tank.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tankPath) != null)
            {
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
                
                Transform oldVis = contentsRoot.transform.Find("TankVisual");
                if (oldVis != null)
                {
                    GameObject.DestroyImmediate(oldVis.gameObject, true);
                }

                GameObject tankModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/TankSketchfab.fbx");
                GameObject newVisual = (GameObject)PrefabUtility.InstantiatePrefab(tankModel);
                newVisual.transform.SetParent(contentsRoot.transform, false);
                newVisual.transform.localPosition = Vector3.zero;
                
                // Rotamos 90 grados para arreglar el chasis. 
                // En la foto, el tanque miraba hacia el lado (eje X en vez de Z). -90 o 90 deberia arreglarlo.
                newVisual.transform.localRotation = Quaternion.Euler(0, -90, 0);
                
                // Hacemos el tanque mas grande
                newVisual.transform.localScale = Vector3.one * 1.5f;
                newVisual.name = "TankVisual";

                TeamTank tt = contentsRoot.GetComponent<TeamTank>();
                if (tt != null)
                {
                    Transform turretMesh = newVisual.transform.Find("Turret");
                    if (turretMesh != null)
                    {
                        // Creamos un pivote para la torreta
                        GameObject turretPivot = new GameObject("TurretPivot");
                        turretPivot.transform.SetParent(newVisual.transform, false);
                        turretPivot.transform.localPosition = turretMesh.localPosition;
                        
                        // Metemos la malla de la torreta dentro del pivote y le corregimos la rotacion
                        turretMesh.SetParent(turretPivot.transform, true);
                        turretMesh.localRotation = Quaternion.Euler(0, -90, 0); 
                        turretMesh.localPosition = Vector3.zero;
                        
                        tt.tower = turretPivot;
                    }
                    else
                    {
                        tt.tower = newVisual; 
                    }
                    
                    MeshRenderer mr = newVisual.GetComponentInChildren<MeshRenderer>();
                    if (mr != null) tt.towerMaterial = mr;

                    Transform shootPos = newVisual.transform.Find("ShootPos");
                    if (shootPos == null)
                    {
                        GameObject sp = new GameObject("ShootPos");
                        sp.transform.SetParent(tt.tower.transform, false);
                        
                        // Ponemos el shootPos delante de la torreta (hacia +Z que es el forward del pivote)
                        sp.transform.localPosition = new Vector3(0, 0.5f, 2.5f);
                        shootPos = sp.transform;
                    }
                    tt.shootInitPosition = shootPos.gameObject;
                }

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                Debug.Log("Swapped, fixed rotation with TurretPivot and increased scale!");
            }
        }
    }
}
