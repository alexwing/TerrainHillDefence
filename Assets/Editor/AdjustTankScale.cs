using UnityEngine;
using UnityEditor;

namespace HillDefence.EditorScripts
{
    public static class AdjustTankScale
    {
        [MenuItem("Tools/Adjust Tank Scale")]
        [InitializeOnLoadMethod]
        public static void DoIt()
        {
            if (SessionState.GetBool("AdjustTankScale1", false)) return;
            SessionState.SetBool("AdjustTankScale1", true);

            string tankPath = "Assets/Resources/Tank.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tankPath) != null)
            {
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
                
                Transform tankVis = contentsRoot.transform.Find("TankVisual");
                if (tankVis != null)
                {
                    tankVis.localScale = Vector3.one * 7.5f;
                }

                // Also adjust the shootInitPosition slightly higher and forward just in case it's inside the bigger mesh now
                Transform shootPos = tankVis.Find("ShootPos");
                if (shootPos != null)
                {
                    shootPos.localPosition = new Vector3(0, 4.0f, 6.0f);
                }

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                Debug.Log("Adjusted Tank scale to be much bigger!");
            }
        }
    }
}
