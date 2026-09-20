using UnityEngine;
using UnityEditor;
using System.IO;

namespace HillDefence.EditorScripts
{
    [InitializeOnLoad]
    public static class GameBalanceMenu
    {
        static GameBalanceMenu()
        {
            EditorApplication.delayCall += GenerateDefaultConfig;
        }

        [MenuItem("Tools/Generate Default GameBalanceConfig Asset")]
        public static void GenerateDefaultConfig()
        {
            string folderPath = "Assets/Resources";
            string assetPath = "Assets/Resources/GameBalanceConfig.asset";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            GameBalanceConfig existing = AssetDatabase.LoadAssetAtPath<GameBalanceConfig>(assetPath);
            if (existing != null)
            {
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                Debug.Log("[GameBalanceMenu] GameBalanceConfig already exists at " + assetPath);
                return;
            }

            GameBalanceConfig newConfig = ScriptableObject.CreateInstance<GameBalanceConfig>();
            AssetDatabase.CreateAsset(newConfig, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = newConfig;
            EditorGUIUtility.PingObject(newConfig);
            Debug.Log("[GameBalanceMenu] Created new default GameBalanceConfig asset at " + assetPath);
        }
    }
}
