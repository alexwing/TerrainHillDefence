using HillDefence;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HillDefence
{
    public class HillDefenceCreator : MonoBehaviour
    {
        Terrain terrain;
        public GameObject hill;
        public int hills = 10;
        [Tooltip("Hill size limit")]
        public float hillSize = 50f;
        public float hillsDistanceBetween = 80f;
        public float ememiesDistanceFromHill = 20f;
        public float ememiesDistanceBetween = 10f;
        public int enemiesPerTeam = 10;
        public GameObject enemyPrefab;
        public float borderSizeLimit = 100;
        // list of all hills postions
        private List<Vector3> hillPositions = new List<Vector3>();

        // list of all hill objects

        public Material flagMaterial;

        //create list on team objects
        public static List<Team> teams = new List<Team>();

        [Tooltip("Teams colors.")]
        public Color[] teamsColors;
        void Start()
        {
            spawnHills();
            spawnEnemy();
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
                Vector3 position = Utils.CreateRamdomPosition(quad, ref hillPositions, hillsDistanceBetween);

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

                //set team color
                team.teamColor = teamsColors[i % teamsColors.Length];
                instanciateTeamFlag.name = "Team " + i;

                //distribute the team color in the hills in order
                instanciateTeamFlag.GetComponent<TeamFlag>().teamColor = team.teamColor;

                GetComponent<TargetTerrain>().ModifyTerrain(instanciateTeamFlag, hillSize, 60f, true);
                // GetComponent<TargetTerrain>().detonationTerrain(instanciateHill, 20f);
            }

        }

        void spawnEnemy()
        {
            //spawn enemy
            for (int i = 0; i < teams.Count; i++)
            {
                //enemies per team
                for (int j = 0; j < enemiesPerTeam; j++)
                {
                    //spawn enemy position near the hill ramdom position
                    Vector3 enemyPosition = Utils.CreateRamdomPosition(new Vector4(teams[i].teamFlag.transform.position.x - ememiesDistanceFromHill, teams[i].teamFlag.transform.position.x + ememiesDistanceFromHill, teams[i].teamFlag.transform.position.z - ememiesDistanceFromHill, teams[i].teamFlag.transform.position.z + ememiesDistanceFromHill), ref hillPositions, ememiesDistanceBetween);
                    //spawn enemy
                    //get terraindata height from positon
                    float height = terrain.SampleHeight(enemyPosition);
                    enemyPosition.y = height;

                    GameObject enemy = Instantiate(enemyPrefab, enemyPosition, Quaternion.identity) as GameObject;

                    TeamSoldier teamSoldier = enemy.GetComponent<TeamSoldier>();
                    teamSoldier.setTeam(teams[i]);
                    enemy.name = "EnemyTeam" + i + "Soldier" + j;
                    teams[i].soldiers.Add(enemy);
                    teams[i].soldiersPosition.Add(enemyPosition);


                }
            }

        }


    }

}