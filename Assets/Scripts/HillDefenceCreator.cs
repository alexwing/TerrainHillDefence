
using System.Collections.Generic;
using UnityEngine;

namespace HillDefence
{
    public class HillDefenceCreator : MonoBehaviour
    {
        Terrain terrain;
        public static HillDefenceCreator instance;
        public static Terrain TerrainInstance;

        [Header("Team")]
        public GameObject hill;
        public int hills = 10;
        [Tooltip("Hill size limit")]
        public float hillSize = 50f;
        public float hillsDistanceBetween = 80f;
        private System.Random rng = new System.Random();

        [Header("Ememies")]
        public float ememiesDistanceFromHill = 20f;
        public float ememiesDistanceBetween = 10f;
        public int enemiesPerTeam = 10;
        public GameObject enemyPrefab;
        public float borderSizeLimit = 100;

        [Header("Effect")]
        [SerializeField] private GameObject[] _magicArray;


        public static List<Team> teams = new List<Team>();

        public static List<NpcInfo> Npcs = new List<NpcInfo>();

        [Tooltip("Teams colors.")]
        public Color[] teamsColors;


        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                TerrainInstance = terrain;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            SpawnHills();
            SpawnEnemyTeam();
            SpawnSoldiers();
            // UIController.instance.CreateHealthbars();         
            AIController.instance.Init((int)terrain.terrainData.size.x);
            MapController.instance.Init((int)terrain.terrainData.size.x, SceneConfig.FindSizeMap);
        }
        void SpawnHills()
        {

            List<Vector3> hillPositions = new List<Vector3>();
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
                TeamFlag teamFlag = instanciateTeamFlag.GetComponent<TeamFlag>();
                //add team
                Team team = new Team();
                team.teamNumber = i;
                team.teamFlag = teamFlag;
                teams.Add(team);

                //set team color
                team.teamColor = teamsColors[i % teamsColors.Length];
                team.bulletPrefab = _magicArray[i % _magicArray.Length];
                instanciateTeamFlag.name = "Flag_" + i;

                //distribute the team color in the hills in order
                team.teamFlag.npcInfo.teamNumber = team.teamNumber;
                team.teamFlag.npcInfo.npcType = NpcType.flag;
                team.teamFlag.npcInfo.npcObject = instanciateTeamFlag;
                Npcs.Add(team.teamFlag);

                GetComponent<TargetTerrain>().ModifyTerrain(instanciateTeamFlag, hillSize, 60f, true);
            }

        }
        void SpawnEnemyTeam()
        {
            //set ememy to team nead distance of flag
            for (int i = 0; i < teams.Count; i++)
            {
                float minimalDistance = float.MaxValue;
                for (int j = 0; j < teams.Count; j++)
                {
                    if (i != j)
                    {
                        Vector3 hillPos = teams[i].teamFlag.transform.position;
                        Vector3 enemyPos = teams[j].teamFlag.transform.position;
                        float distance = Vector3.Distance(enemyPos, hillPos);
                        if (minimalDistance > distance)
                        {
                            minimalDistance = distance;
                            teams[i].enemyTeam = teams[j];
                        }
                    }
                }
            }
        }

        void SpawnSoldiers()
        {
            List<Vector3> hillPositions = new List<Vector3>();
            //spawn soldier
            for (int i = 0; i < teams.Count; i++)
            {
                //enemies per team
                for (int j = 0; j < enemiesPerTeam; j++)
                {
                    //spawn soldier position near the hill ramdom position
                    Vector3 enemyPosition = Utils.CreateRamdomPosition(new Vector4(teams[i].teamFlag.transform.position.x - ememiesDistanceFromHill, teams[i].teamFlag.transform.position.x + ememiesDistanceFromHill, teams[i].teamFlag.transform.position.z - ememiesDistanceFromHill, teams[i].teamFlag.transform.position.z + ememiesDistanceFromHill), ref hillPositions, ememiesDistanceBetween);
                    //spawn soldier
                    //get terraindata height from positon
                    float height = terrain.SampleHeight(enemyPosition);
                    enemyPosition.y = height;

                    GameObject soldier = Instantiate(enemyPrefab, enemyPosition, Quaternion.identity) as GameObject;

                    TeamSoldier teamSoldier = soldier.GetComponent<TeamSoldier>();
                    soldier.name = "Soldier_" + i + "_" + j;
                    teams[i].soldiers.Add(teamSoldier);
                    teamSoldier.npcInfo.teamNumber = teams[i].teamNumber;
                    teamSoldier.npcInfo.npcNumber = j;
                    teamSoldier.npcInfo.npcType = NpcType.soldier;
                    teamSoldier.npcInfo.npcObject = soldier;
                    Npcs.Add(teamSoldier);

                    teamSoldier.Init();

                }
            }

        }


        public List<TeamSoldier> Shuffle(List<TeamSoldier> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                TeamSoldier value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
        public void EvaluateWin()
        {
            int countPendingTeams = 0;
            Team win = null;
            foreach (Team team in teams)
            {
                if (!team.teamFlag.npcInfo.isDead)
                {
                    countPendingTeams++;
                    win = team;
                }
            }
            if (countPendingTeams == 1)
            {
                UIController.instance.ShowWin(win);

            }
        }
    }
}