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
            if (SessionState.GetBool("ForceSwapFBX6", false)) return;
            SessionState.SetBool("ForceSwapFBX6", true);

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
                
                // Rotacion del chasis: el modelo original de Sketchfab mira hacia +X.
                // Lo rotamos -90 en Y para que mire hacia +Z (Forward de Unity).
                newVisual.transform.localRotation = Quaternion.Euler(0, -90, 0);
                
                // Escala: el usuario dijo 'el doble de grande', y antes estaba en 1.5f. 
                // Lo bajamos a la mitad: 0.75f.
                newVisual.transform.localScale = Vector3.one * 0.75f;
                newVisual.name = "TankVisual";

                TeamTank tt = contentsRoot.GetComponent<TeamTank>();
                if (tt != null)
                {
                    Transform turretMesh = newVisual.transform.Find("Turret");
                    if (turretMesh != null)
                    {
                        GameObject turretPivot = new GameObject("TurretPivot");
                        turretPivot.transform.SetParent(newVisual.transform, false); // Hereda el rot de newVisual
                        turretPivot.transform.localPosition = turretMesh.localPosition;
                        
                        // Metemos el mesh dentro del pivote. false para que su localPosition sea 0 respecto al pivote.
                        turretMesh.SetParent(turretPivot.transform, false);
                        // El pivot cuando apunte al enemigo pondra su +Z hacia el enemigo. 
                        // Como el mesh original mira a +X, lo rotamos -90 localmente para alinear su +X con el +Z del pivote.
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
                        
                        // Posicion frente a la torreta (+Z del pivote) y un poco elevado (+Y).
                        sp.transform.localPosition = new Vector3(0, 0.4f, 4.0f);
                        shootPos = sp.transform;
                    }
                    tt.shootInitPosition = shootPos.gameObject;
                }

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                Debug.Log("Swapped, fixed rotation perfectly and adjusted scale to 0.75f!");
            }
        }
    }
}
