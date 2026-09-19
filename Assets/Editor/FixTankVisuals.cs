using UnityEngine;
using UnityEditor;

namespace HillDefence.EditorScripts
{
    public static class FixTankVisuals
    {
        [MenuItem("Tools/Fix Tank Visuals")]
        [InitializeOnLoadMethod]
        public static void DoIt()
        {
            if (SessionState.GetBool("FixTankVisuals2", false)) return;
            SessionState.SetBool("FixTankVisuals2", true);

            string tankPath = "Assets/Resources/Tank.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tankPath) != null)
            {
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
                TeamTank tt = contentsRoot.GetComponent<TeamTank>();
                
                Transform[] all = contentsRoot.GetComponentsInChildren<Transform>(true);
                Transform foundTurret = null;
                Transform foundBarrel = null;
                Transform foundHull = null;
                
                foreach (var t in all)
                {
                    if (t.name == "Turret") foundTurret = t;
                    if (t.name == "Barrel") foundBarrel = t;
                    if (t.name == "Hull") foundHull = t;
                }

                if (foundTurret != null)
                {
                    foundTurret.localPosition = new Vector3(0, 1.0f, -0.5f); 
                }

                if (foundBarrel != null)
                {
                    foundBarrel.localPosition = new Vector3(0, 0.0f, 2.0f); 
                }

                Material newMat = new Material(Shader.Find("Standard"));
                
                if (foundHull != null)
                {
                    MeshRenderer mr = foundHull.GetComponent<MeshRenderer>();
                    if (mr != null) 
                    {
                        mr.sharedMaterial = newMat;
                        if (tt != null) tt.towerMaterial = mr; 
                    }
                }
                
                if (foundTurret != null)
                {
                    MeshRenderer mr = foundTurret.GetComponent<MeshRenderer>();
                    if (mr != null) mr.sharedMaterial = newMat;
                }
                
                if (foundBarrel != null)
                {
                    MeshRenderer mr = foundBarrel.GetComponent<MeshRenderer>();
                    if (mr != null) 
                    {
                        Material barrelMat = new Material(Shader.Find("Standard"));
                        barrelMat.color = Color.gray;
                        mr.sharedMaterial = barrelMat;
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                Debug.Log("Fixed Tank Visuals Positions!");
            }
        }
    }
}

