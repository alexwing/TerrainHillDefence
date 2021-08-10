/**
 * Reset terrain from TerrainData Backup
 */

using UnityEngine;

public class TerrainReset : MonoBehaviour {
	
	private Terrain terrainToRestore;
    public TerrainData TerrainDataBackup;

    private float[,] originalHeights;

    private void Awake()
    {
		terrainToRestore = GetComponent<Terrain>();
		restoreTerrain();
	}

    private void OnDestroy()
	{
		restoreTerrain();
	}
	

	private void restoreTerrain()
    {
		this.originalHeights = TerrainDataBackup.GetHeights(
			0, 0, terrainToRestore.terrainData.heightmapResolution, terrainToRestore.terrainData.heightmapResolution);

		this.terrainToRestore.terrainData.SetHeights(0, 0, this.originalHeights);
	}
}
