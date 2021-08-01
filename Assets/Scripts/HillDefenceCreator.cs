using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HillDefenceCreator : MonoBehaviour
{
    Terrain terrain;
    public GameObject hill;
    public int hills = 10;
    [Tooltip("Hill size limit")]
    public float hillSize = 50f;
    public float hillsDistanceBetween = 80f;
    float borderSizeLimit = 100;
    // list of all hills postions
    private List<Vector3> hillPositions = new List<Vector3>();

    // list of all hill objects
    private List<GameObject> teams = new List<GameObject>();

        public Material flagMaterial;

    [Tooltip("Teams colors.")]
    public Color[] teamsColors;
    void Start()
    {
        spawnHills();
    }
    void spawnHills()
    {
        // Get the terrain
        terrain = GetComponent<Terrain>();

        //spam the hill in ramdom positions in the terrain location
        for (int i = 0; i < hills; i++)
        {

            Vector4 quad = new Vector4(borderSizeLimit, terrain.terrainData.size.x - borderSizeLimit, borderSizeLimit, terrain.terrainData.size.z - borderSizeLimit);

            // Random position in the terrain with a size limit and a distance limit between hills
            Vector3 position = Utils.CreateRamdomPosition(quad,ref hillPositions,hillsDistanceBetween);

            //get terraindata height from positon
            float height = terrain.SampleHeight(position);

            position.y = height;
            //clone instanciate Hill
            GameObject instanciateHill = Instantiate(hill, position, Quaternion.identity) as GameObject;

            //add hill to list
            teams.Add(instanciateHill);            

            //distribute the team color in the hills in order
            instanciateHill.GetComponent<TeamFlag>().teamColor=  teamsColors[i % teamsColors.Length];            
    
            GetComponent<TargetTerrain>().modifyTerrain(instanciateHill, true, hillSize, 100f);
            // GetComponent<TargetTerrain>().detonationTerrain(instanciateHill, 20f);
        }

    }


}

