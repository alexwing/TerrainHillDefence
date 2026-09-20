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
            EditorApplication.delayCall += Execute;
        }

        [MenuItem("Tools/Rebuild Tank 4X")]
        public static void Execute()
        {
            if (SessionState.GetBool("RebuildTank4X_v5", false)) return;
            SessionState.SetBool("RebuildTank4X_v5", true);

            string tankPath = "Assets/Resources/Tank.prefab";
            string fbxPath = "Assets/Models/TankSketchfab.fbx";
            string teamMatPath = "Assets/Materials/TankTeamMaterial.mat";
            string barrelMatPath = "Assets/Materials/TankBarrelMaterial.mat";

            AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);
            GameObject tankModel = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (tankModel == null)
            {
                Debug.LogError("[RebuildTank4X] TankSketchfab.fbx not found at " + fbxPath);
                return;
            }

            // Ensure materials
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            Material teamMat = AssetDatabase.LoadAssetAtPath<Material>(teamMatPath);
            if (teamMat == null)
            {
                teamMat = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(teamMat, teamMatPath);
            }

            Material barrelMat = AssetDatabase.LoadAssetAtPath<Material>(barrelMatPath);
            if (barrelMat == null)
            {
                barrelMat = new Material(Shader.Find("Standard"));
                barrelMat.color = Color.gray;
                AssetDatabase.CreateAsset(barrelMat, barrelMatPath);
            }

            // Build fresh in-memory hierarchy
            GameObject root = new GameObject("Tank");
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one * 2.0f;

            BoxCollider bc = root.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(8f, 4f, 12f);
            bc.center = new Vector3(0f, 2f, 0f);

            TeamTank tt = root.AddComponent<TeamTank>();

            // VisualWrapper with 4x scale (original was 0.75, now 3.0)
            GameObject visualWrapper = new GameObject("VisualWrapper");
            visualWrapper.transform.SetParent(root.transform, false);
            visualWrapper.transform.localPosition = Vector3.zero;
            visualWrapper.transform.localRotation = Quaternion.identity;
            visualWrapper.transform.localScale = Vector3.one * 3.0f;

            // Instantiate FBX
            GameObject fbxInst = (GameObject)PrefabUtility.InstantiatePrefab(tankModel);
            fbxInst.name = "FBX_Root";
            fbxInst.transform.SetParent(visualWrapper.transform, false);
            fbxInst.transform.localPosition = Vector3.zero;
            fbxInst.transform.localRotation = Quaternion.identity;

            // Unpack completely
            PrefabUtility.UnpackPrefabInstance(fbxInst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            // Find parts: fbxInst is Hull, child is Turret, grandchild is Barrel
            Transform turret = fbxInst.transform.Find("Turret");
            Transform barrel = turret != null ? turret.Find("Barrel") : null;

            // Apply Materials
            MeshRenderer mrHull = fbxInst.GetComponent<MeshRenderer>();
            if (mrHull != null && teamMat != null)
            {
                mrHull.sharedMaterial = teamMat;
                tt.towerMaterial = mrHull;
            }

            if (turret != null)
            {
                MeshRenderer mrTurret = turret.GetComponent<MeshRenderer>();
                if (mrTurret != null && teamMat != null)
                {
                    mrTurret.sharedMaterial = teamMat;
                }
            }

            if (barrel != null)
            {
                MeshRenderer mrBarrel = barrel.GetComponent<MeshRenderer>();
                if (mrBarrel != null && barrelMat != null)
                {
                    mrBarrel.sharedMaterial = barrelMat;
                }
            }

            // Assign turret for aiming
            if (turret != null)
            {
                tt.tower = turret.gameObject;

                // Create ShootPos at the tip of the barrel in Turret local space
                GameObject sp = new GameObject("ShootPos");
                sp.transform.SetParent(turret, false);
                sp.transform.localPosition = new Vector3(0f, -0.75f, 6.2f);
                sp.transform.localRotation = Quaternion.identity;
                tt.shootInitPosition = sp;
            }
            else
            {
                tt.tower = visualWrapper;
            }

            tt.healthBarPrefab = Resources.Load<GameObject>("healthLayout");
            tt.npcInfo.npcType = NpcType.tank;

            // Save as Prefab Asset
            PrefabUtility.SaveAsPrefabAsset(root, tankPath);
            GameObject.DestroyImmediate(root);
            Debug.Log("[RebuildTank4X] SUCCESS! Tank.prefab built with 4X scale, perfect horizontal alignment, and attached cannon.");

            // Ensure UI.prefab references the tank
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
                    }
                }
                PrefabUtility.UnloadPrefabContents(uiRoot);
            }
        }
    }
}
