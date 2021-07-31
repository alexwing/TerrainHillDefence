using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class HillDefenceCreator : MonoBehaviour
{
    Terrain terrain;
    public GameObject hill;
    public int hills = 10;
    void Start(){
        spawnHills();
    }
    void spawnHills()
    {
        // Get the terrain
        terrain = GetComponent<Terrain>();

        //spam the hill in ramdom positions in the terrain location
        for (int i = 0; i < hills; i++)
        {
            Vector3 position = new Vector3(Random.Range(0, terrain.terrainData.size.x), 0, Random.Range(0, terrain.terrainData.size.z));
            //get terraindata height from positon
            float height = terrain.SampleHeight(position);
            
            position.y = height;
            
            GameObject instanciateHill =  Instantiate(hill, position, Quaternion.identity);

            GetComponent<TargetTerrain>().modifyTerrain(instanciateHill, true, 50f, 100f);
           // GetComponent<TargetTerrain>().detonationTerrain(instanciateHill, 20f);
        }

    }

}

