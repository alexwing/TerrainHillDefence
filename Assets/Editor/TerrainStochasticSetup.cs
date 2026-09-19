using UnityEngine;
using UnityEditor;

namespace HillDefence
{
    public class TerrainStochasticSetup
    {
        [MenuItem("Tools/Setup Stochastic Terrain")]
        public static void SetupTerrain()
        {
            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                terrain = Object.FindFirstObjectByType<Terrain>();
            }

            if (terrain != null)
            {
                Shader stochasticShader = Shader.Find("Custom/StochasticTerrain");
                if (stochasticShader == null)
                {
                    Debug.LogError("Custom/StochasticTerrain shader not found!");
                    return;
                }

                Material customMaterial = new Material(stochasticShader);
                
                // Save the material to assets so it persists
                string path = "Assets/Materials/StochasticTerrainMaterial.mat";
                if (!System.IO.Directory.Exists("Assets/Materials"))
                {
                    System.IO.Directory.CreateDirectory("Assets/Materials");
                }
                
                AssetDatabase.CreateAsset(customMaterial, path);
                AssetDatabase.SaveAssets();

                terrain.materialTemplate = customMaterial;
                Debug.Log("Stochastic Terrain Material applied to " + terrain.gameObject.name);
            }
            else
            {
                Debug.LogError("No Terrain found in the scene.");
            }
        }
    }
}
