using UnityEngine;
using UnityEditor;

namespace HillDefence.EditorScripts
{
    public static class FinalTankFix
    {
        [MenuItem("Tools/Final Tank Fix")]
        [InitializeOnLoadMethod]
        public static void DoIt()
        {
            if (SessionState.GetBool("FinalTankFixed5", false)) return;
            SessionState.SetBool("FinalTankFixed5", true);

            // 1. Fix Tank.prefab
            string tankPath = "Assets/Resources/Tank.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tankPath) != null)
            {
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
                TeamTower tt = contentsRoot.GetComponent<TeamTower>();
                if (tt != null)
                {
                    // Clean up any old GunPlat / Tower visuals
                    foreach (Transform child in contentsRoot.transform)
                    {
                        if (child.name.Contains("Gun") || child.name.Contains("Plat") || child.name.Contains("Tower"))
                        {
                            GameObject.DestroyImmediate(child.gameObject, true);
                        }
                    }

                    // Add TankVisual if missing
                    Transform tankVis = contentsRoot.transform.Find("TankVisual");
                    if (tankVis == null)
                    {
                        GameObject tankModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/TankSketchfab.fbx");
                        GameObject newVisual = (GameObject)PrefabUtility.InstantiatePrefab(tankModel);
                        newVisual.transform.SetParent(contentsRoot.transform, false);
                        newVisual.transform.localPosition = Vector3.zero;
                        newVisual.transform.localRotation = Quaternion.identity;
                        newVisual.transform.localScale = Vector3.one * 0.35f;
                        newVisual.name = "TankVisual";
                        tankVis = newVisual.transform;
                    }

                    tt.tower = tankVis.gameObject;
                    MeshRenderer mr = tankVis.GetComponentInChildren<MeshRenderer>();
                    if (mr != null)
                    {
                        tt.towerMaterial = mr;
                        // using fbx materials
                    }

                    // Add shootInitPosition if missing
                    Transform shootPos = tankVis.Find("ShootPos");
                    if (shootPos == null)
                    {
                        GameObject sp = new GameObject("ShootPos");
                        sp.transform.SetParent(tankVis, false);
                        sp.transform.localPosition = new Vector3(0, 1.5f, 3.0f);
                        shootPos = sp.transform;
                    }
                    tt.shootInitPosition = shootPos.gameObject;
                }
                PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                Debug.Log("Tank.prefab fixed!");
            }

            // 2. Ensure UIController is hooked up
            string uiPath = "Assets/Prefabs/UI.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(uiPath) != null)
            {
                GameObject uiRoot = PrefabUtility.LoadPrefabContents(uiPath);
                UIController uic = uiRoot.GetComponent<UIController>();
                if (uic != null)
                {
                    if (uic.tankPrefab == null)
                    {
                        uic.tankPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Tank.prefab");
                        PrefabUtility.SaveAsPrefabAsset(uiRoot, uiPath);
                        Debug.Log("UI.prefab fixed!");
                    }
                }
                PrefabUtility.UnloadPrefabContents(uiRoot);
            }
        }
    }
}



