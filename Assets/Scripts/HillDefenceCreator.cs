
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
            if (instance == null || instance != this)
            {
                instance = this;
                TerrainInstance = terrain;
                if (GetComponent<ObjectPooler>() == null)
                {
                    gameObject.AddComponent<ObjectPooler>();
                }
            }
            
            // Hand-picked list of 25 highly distinct colors to guarantee maximum contrast between teams.
            // These are intentionally non-sequential and avoid dark/grey tones.
            teamsColors = new Color[]
            {
                new Color32(255, 0, 0, 255),     // 0: Red
                new Color32(0, 128, 255, 255),   // 1: Azure Blue
                new Color32(0, 255, 0, 255),     // 2: Lime Green
                new Color32(255, 255, 0, 255),   // 3: Yellow
                new Color32(255, 0, 255, 255),   // 4: Magenta
                new Color32(0, 255, 255, 255),   // 5: Cyan
                new Color32(255, 128, 0, 255),   // 6: Orange
                new Color32(128, 0, 255, 255),   // 7: Purple
                new Color32(0, 255, 128, 255),   // 8: Spring Green
                new Color32(255, 0, 128, 255),   // 9: Rose
                new Color32(128, 255, 0, 255),   // 10: Chartreuse
                new Color32(128, 0, 0, 255),     // 11: Maroon
                new Color32(0, 128, 128, 255),   // 12: Teal
                new Color32(0, 0, 128, 255),     // 13: Navy
                new Color32(128, 128, 0, 255),   // 14: Olive
                new Color32(255, 165, 0, 255),   // 15: Bright Orange
                new Color32(0, 100, 0, 255),     // 16: Dark Green
                new Color32(139, 69, 19, 255),   // 17: Saddle Brown
                new Color32(255, 20, 147, 255),  // 18: Deep Pink
                new Color32(75, 0, 130, 255),    // 19: Indigo
                new Color32(255, 215, 0, 255),   // 20: Gold
                new Color32(250, 128, 114, 255), // 21: Salmon
                new Color32(64, 224, 208, 255),  // 22: Turquoise
                new Color32(220, 20, 60, 255),   // 23: Crimson
                new Color32(238, 130, 238, 255)  // 24: Violet
            };

            // Critical for restarting the scene properly: clear static variables
            teams.Clear();
            Npcs.Clear();
            FlyCamera.lockMovement = false;
        }

        void Start()
        {
            SpawnHills();
            SpawnEnemyTeam();
            SpawnSoldiers();
            AIController.instance.Init((int)terrain.terrainData.size.x);
            MapController.instance.Init(
                terrain.terrainData.size.x,
                terrain.terrainData.size.z,
                terrain.transform.position,
                SceneConfig.FindSizeMap);
            if (GameInfoPanel.instance == null)
            {
                if (UIController.instance != null)
                    UIController.instance.gameObject.AddComponent<GameInfoPanel>();
                else
                    new GameObject("GameInfoPanel").AddComponent<GameInfoPanel>();
            }
            if (GameInfoPanel.instance != null) GameInfoPanel.instance.Init();
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