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

    public Material flagMaterial;

    //create team class and properties
    public class Team
    {
        public int teamNumber;
        public GameObject teamFlag;
        public List<GameObject> soldiers = new List<GameObject>();
        public int killCount;
        public int deathCount;
        public int flagCount;
        public int teamEnemyNumber;

    }
    //create list on team objects
    public static List<Team> teams = new List<Team>();

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
            GameObject instanciateTeamFlag = Instantiate(hill, position, Quaternion.identity) as GameObject;

            //add team
            Team team = new Team();
            team.teamNumber = i;
            team.teamFlag = instanciateTeamFlag;            
            teams.Add(team);            

            //distribute the team color in the hills in order
            instanciateTeamFlag.GetComponent<TeamFlag>().teamColor=  teamsColors[i % teamsColors.Length];            
    
            GetComponent<TargetTerrain>().modifyTerrain(instanciateTeamFlag, true, hillSize, 100f);
            // GetComponent<TargetTerrain>().detonationTerrain(instanciateHill, 20f);
        }

    }


}

