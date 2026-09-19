using UnityEngine;
using UnityEditor;

namespace HillDefence.EditorScripts
{
    public static class SwapTankComponent
    {
        [MenuItem("Tools/Swap Tank Component")]
        [InitializeOnLoadMethod]
        public static void DoIt()
        {
            if (SessionState.GetBool("SwapTankComponent3", false)) return;
            SessionState.SetBool("SwapTankComponent3", true);

            string tankPath = "Assets/Resources/Tank.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tankPath) != null)
            {
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
                
                TeamTower oldTT = contentsRoot.GetComponent<TeamTower>();
                TeamTank newTT = contentsRoot.GetComponent<TeamTank>();

                if (oldTT != null && newTT == null)
                {
                    newTT = contentsRoot.AddComponent<TeamTank>();
                    
                    // Copy fields
                    newTT.tower = oldTT.tower;
                    newTT.towerMaterial = oldTT.towerMaterial;
                    newTT.healthBarPrefab = oldTT.healthBarPrefab;
                    newTT.shootInitPosition = oldTT.shootInitPosition;
                    
                    newTT.npcInfo = oldTT.npcInfo;
                    newTT.npcInfo.npcType = NpcType.tank;

                    GameObject.DestroyImmediate(oldTT, true);
                    PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                    Debug.Log("Swapped TeamTower for TeamTank on Tank.prefab!");
                }
                else if (newTT != null)
                {
                    newTT.npcInfo.npcType = NpcType.tank;
                    PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                }
                
                PrefabUtility.UnloadPrefabContents(contentsRoot);
            }
        }
    }
}


