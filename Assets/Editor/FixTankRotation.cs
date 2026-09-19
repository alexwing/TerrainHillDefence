using UnityEngine;
using UnityEditor;

namespace HillDefence.EditorScripts
{
    public static class FixTankRotation
    {
        [MenuItem("Tools/Fix Tank Rotation")]
        [InitializeOnLoadMethod]
        public static void DoIt()
        {
            if (SessionState.GetBool("FixTankRotation2", false)) return;
            SessionState.SetBool("FixTankRotation2", true);

            string tankPath = "Assets/Resources/Tank.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tankPath) != null)
            {
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(tankPath);
                
                Transform tankVis = contentsRoot.transform.Find("TankVisual");
                if (tankVis != null)
                {
                    tankVis.localRotation = Quaternion.Euler(0, -90, 0); // Override completely
                }
                
                PrefabUtility.SaveAsPrefabAsset(contentsRoot, tankPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                Debug.Log("Fixed TankVisual local rotation!");
            }
        }
    }
}
