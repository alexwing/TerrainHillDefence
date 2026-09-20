using UnityEngine;
using UnityEditor;
using System.IO;

namespace HillDefence.EditorScripts
{
    [InitializeOnLoad]
    public static class RebuildTank4X
    {
        static RebuildTank4X()
        {
            Execute();
        }

        [MenuItem("Tools/Rebuild Tank 4X")]
        [InitializeOnLoadMethod]
        public static void Execute()
        {
            if (SessionState.GetBool("RebuildTank4X_v2", false)) return;
            SessionState.SetBool("RebuildTank4X_v2", true);

            string tankPath = "Assets/Resources/Tank.prefab";
            string fbxPath = "Assets/Models/TankSketchfab.fbx";
            string teamMatPath = "Assets/Materials/TankTeamMaterial.mat";
            string barrelMatPath = "Assets/Materials/TankBarrelMaterial.mat";

            GameObject fbxModel = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxModel == null)
            {
                Debug.LogError("[RebuildTank4X] TankSketchfab.fbx not found at " + fbxPath);
                return;
            }

            Material teamMat = AssetDatabase.LoadAssetAtPath<Material>(teamMatPath);
            Material barrelMat = AssetDatabase.LoadAssetAtPath<Material>(barrelMatPath);

            // Clean or create Tank.prefab
            GameObject root;
            bool exists = File.Exists(tankPath);
            if (exists)
            {
                root = PrefabUtility.LoadPrefabContents(tankPath);
            }
            else
            {
                root = new GameObject("Tank");
            }

            // Remove all existing children from root
            int childCount = root.transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(root.transform.GetChild(i).gameObject, true);
            }

            // Set root transform
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            // Add or configure BoxCollider
            BoxCollider bc = root.GetComponent<BoxCollider>();
            if (bc == null) bc = root.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            // 4x larger size: length ~46, width ~30, height ~14
            bc.size = new Vector3(30f, 15f, 46f);
            bc.center = new Vector3(0f, 7.5f, 0f);

            // Add or configure TeamTank
            TeamTank tt = root.GetComponent<TeamTank>();
            if (tt == null) tt = root.AddComponent<TeamTank>();

            // Remove TeamTower if it was separately on root
            TeamTower oldTower = root.GetComponent<TeamTower>();
            if (oldTower != null && oldTower != tt)
            {
                GameObject.DestroyImmediate(oldTower, true);
            }

            // Instantiate FBX visual as child
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbxModel, root.transform);
            visual.name = "TankVisual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            // 4x scale (previous was 1.5x, 1.5 * 4 = 6.0f)
            visual.transform.localScale = Vector3.one * 6.0f;

            // Find parts inside TankVisual
            Transform hullT = visual.transform.Find("Hull");
            Transform turretT = null;
            Transform barrelT = null;

            if (hullT != null)
            {
                turretT = hullT.Find("Turret");
            }
            if (turretT == null)
            {
                turretT = visual.transform.Find("Turret");
            }
            if (turretT != null)
            {
                barrelT = turretT.Find("Barrel");
            }

            // Fallback searches if hierarchy varies
            if (hullT == null) hullT = visual.transform;
            if (turretT == null) turretT = visual.transform;

            // Setup Materials
            if (hullT != null)
            {
                MeshRenderer mr = hullT.GetComponent<MeshRenderer>();
                if (mr != null && teamMat != null)
                {
                    mr.sharedMaterial = teamMat;
                    tt.towerMaterial = mr;
                }
            }

            if (turretT != null)
            {
                MeshRenderer mr = turretT.GetComponent<MeshRenderer>();
                if (mr != null && teamMat != null)
                {
                    mr.sharedMaterial = teamMat;
                }
            }

            if (barrelT != null)
            {
                MeshRenderer mr = barrelT.GetComponent<MeshRenderer>();
                if (mr != null && barrelMat != null)
                {
                    mr.sharedMaterial = barrelMat;
                }
            }

            // Setup ShootPos on Turret
            Transform shootPosT = turretT.Find("ShootPos");
            if (shootPosT == null)
            {
                GameObject sp = new GameObject("ShootPos");
                sp.transform.SetParent(turretT, false);
                // Tip of the muzzle in turret local space
                sp.transform.localPosition = new Vector3(0f, 0.52f, 6.8f);
                sp.transform.localRotation = Quaternion.identity;
                shootPosT = sp.transform;
            }
            else
            {
                shootPosT.localPosition = new Vector3(0f, 0.52f, 6.8f);
                shootPosT.localRotation = Quaternion.identity;
            }

            // Hook up TeamTank references
            tt.tower = turretT.gameObject;
            tt.shootInitPosition = shootPosT.gameObject;
            tt.healthBarPrefab = Resources.Load<GameObject>("healthLayout");
            tt.npcInfo.npcType = NpcType.tank;

            // Save Prefab
            PrefabUtility.SaveAsPrefabAsset(root, tankPath);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log("[RebuildTank4X] Successfully assembled Tank.prefab 4X with attached cannon!");

            // Also verify UI.prefab
            string uiPath = "Assets/Prefabs/UI.prefab";
            if (File.Exists(uiPath))
            {
                GameObject uiRoot = PrefabUtility.LoadPrefabContents(uiPath);
                UIController uic = uiRoot.GetComponent<UIController>();
                if (uic != null)
                {
                    GameObject tankAsset = AssetDatabase.LoadAssetAtPath<GameObject>(tankPath);
                    if (uic.tankPrefab != tankAsset)
                    {
                        uic.tankPrefab = tankAsset;
                        PrefabUtility.SaveAsPrefabAsset(uiRoot, uiPath);
                        Debug.Log("[RebuildTank4X] Assigned tankPrefab in UI.prefab");
                    }
                }
                PrefabUtility.UnloadPrefabContents(uiRoot);
            }
        }
    }
}
