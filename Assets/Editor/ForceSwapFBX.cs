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
            if (SessionState.GetBool("ForceSwapFBX1", false)) return;
            SessionState.SetBool("ForceSwapFBX1", true);

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
                GameObject tankModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Tank.fbx");
                GameObject newVisual = (GameObject)PrefabUtility.InstantiatePrefab(tankModel);
                newVisual.transform.SetParent(contentsRoot.transform, false);
                newVisual.transform.localPosition = Vector3.zero;
                newVisual.transform.localRotation = Quaternion.identity;
                newVisual.transform.localScale = Vector3.one * 0.35f;
                newVisual.name = "TankVisual";

                TeamTank tt = contentsRoot.GetComponent<TeamTank>();
                if (tt != null)
                {
                    tt.tower = newVisual;
                    MeshRenderer mr = newVisual.GetComponentInChildren<MeshRenderer>();
                    if (mr != null)
                    {
                        tt.towerMaterial = mr;
                    }

                    // Recreate ShootPos
                    Transform shootPos = newVisual.transform.Find("ShootPos");
                    if (shootPos == null)
                    {
                        GameObject sp = new GameObject("ShootPos");
                        sp.transform.SetParent(newVisual.transform, false);
                        sp.transform.localPosition = new Vector3(0, 1.5f, 3.0f);
                        shootPos = sp.transform;
                    }
                    tt.shootInitPosition = shootPos.gameObject;
                }

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                Debug.Log("Swapped to the new Blender FBX model!");
            }
        }
    }
}
