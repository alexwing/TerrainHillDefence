using UnityEngine;
using System.Collections.Generic;


namespace HillDefence
{
    public class AIController : MonoBehaviour
    {

        //create bit bidimensional array for the AI

        public GameNpc[,] AIMap;

        public RenderTexture AIMapTexture;
        private Texture2D AIBitmap;

        public float realWidth;
        public float realHeight;

        public int width;
        public int height;
        public static AIController instance;
        public GameObject playerPoiMap;

        void Awake()
        {
            instance = this;
        }
        public void Init(int size)
        {
            realHeight = realWidth = size;
            //   print(HillDefenceCreator.TerrainInstance.terrainData.size.x);
            height = width = SceneConfig.FindSizeMap;
            AIMap = new GameNpc[width, height];
            AIBitmap = new Texture2D(width, height);

            InvokeRepeating("refreshAIMap", 2f, 1f / SceneConfig.AICheckFrameRate);
        }

        //paint a color in array position
        public void paint(int x, int y, GameNpc npc)
        {
            AIMap[x, y] = npc;
        }


        public void refreshAIMap()
        {
            AIMap = new GameNpc[width, height];
            AIBitmap = Utils.FillColorAlpha(AIBitmap);

            foreach (TeamSoldier soldier in HillDefenceCreator.soldiers)
            {
                if (!soldier.npcInfo.isDead && soldier != null)
                {
                    Vector2 pos = posToMap(soldier.transform.position.x, soldier.transform.position.z);
                    if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                    {
                        AIMap[(int)pos.x, (int)pos.y] = soldier.npcInfo;
                        //to paint texture2d AIBitmap
                        AIBitmap.SetPixel((int)pos.x, (int)pos.y, soldier.npcInfo.teamColor);
                        //  print("paint x: " + (int)pos.x + " y: " + (int)pos.y + "color " + soldier.npcInfo.teamColor.r + " " + soldier.npcInfo.teamColor.g + " " + soldier.npcInfo.teamColor.b);
                    }
                }
            }
            foreach (TeamTower tower in HillDefenceCreator.towers)
            {
                if (!tower.npcInfo.isDead && tower != null)
                {
                    Vector2 pos = posToMap(tower.transform.position.x, tower.transform.position.z);
                    if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                    {
                        AIMap[(int)pos.x, (int)pos.y] = tower.npcInfo;
                        //to paint texture2d AIBitmap
                        for (int i = 0; i <= 1; i++)
                        {
                            for (int j = 0; j <= 1; j++)
                            {
                                AIBitmap.SetPixel((int)pos.x + i, (int)pos.y + j, tower.npcInfo.teamColor);
                            }
                        }
                    }
                }
            }
            foreach (Team team in HillDefenceCreator.teams)
            {
                if (!team.teamFlag.npcInfo.isDead && team.teamFlag != null)
                {
                    Vector2 pos = posToMap(team.teamFlag.transform.position.x, team.teamFlag.transform.position.z);
                    if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                    {
                        AIMap[(int)pos.x, (int)pos.y] = team.teamFlag.npcInfo;
                        //to paint texture2d AIBitmap
                        //paint reactangle pixes
                        for (int i = -2; i <= 2; i++)
                        {
                            for (int j = -2; j <= 2; j++)
                            {
                                if (i != 0 || j != 0)
                                {
                                    AIBitmap.SetPixel((int)pos.x + i, (int)pos.y + j, team.teamFlag.npcInfo.teamColor);
                                }
                            }
                        }
                    }
                }
            }
            AIBitmap.Apply();
            Graphics.Blit(AIBitmap, AIMapTexture);

            //update position of playerPoiMap from camera main to aiMap with same position
            Vector2 pos2 = posToPostionMap(Camera.main.transform.position.x, Camera.main.transform.position.z);
            playerPoiMap.GetComponent<RectTransform>().localPosition = new Vector3(pos2.x, pos2.y);

            playerPoiMap.GetComponent<RectTransform>().rotation = new Quaternion(Camera.main.transform.rotation.x, Camera.main.transform.rotation.z, 0, 0);
           // AsignEnemies();
        }

        public void AsignEnemies()
        {
            //find all soldiers
            foreach (TeamSoldier soldier in HillDefenceCreator.soldiers)
            {
                if (!soldier.npcInfo.isDead && soldier != null && (soldier.enemyNpc == null || soldier.enemyNpc.npcType == NpcType.flag))
                {
                    GameNpc findEnemyNpc = null;
                    Vector3 pos = posToMap(soldier.transform.position.x, soldier.transform.position.z);
                    if (soldier.enemyNpc == null || soldier.enemyNpc.npcType == NpcType.flag)
                    {
                        findEnemyNpc = getNearNpc(pos, soldier.npcInfo.teamNumber, SceneConfig.SOLDIER.FindEnemyRange);
                    }
                    if (findEnemyNpc == null && (soldier.enemyNpc == null || soldier.enemyNpc.isDead))
                    {
                        findEnemyNpc = getNearNpc(pos, soldier.npcInfo.teamNumber, width, NpcType.flag);
                    }
                    if (findEnemyNpc != null)
                    {
                        soldier.enemyNpc = findEnemyNpc;
                        soldier.enemy = NpcInfo.npcToGameObject(findEnemyNpc);
                    }
                }
            }
            foreach (TeamTower tower in HillDefenceCreator.towers)
            {
                if (!tower.npcInfo.isDead && tower != null && tower.enemyNpc == null)
                {
                    Vector3 pos = posToMap(tower.transform.position.x, tower.transform.position.z);
                    tower.enemyNpc = getNearNpc(pos, tower.npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.soldier);
                    tower.enemy = NpcInfo.npcToGameObject(tower.enemyNpc);
                }
            }

        }



        public GameNpc getNearNpc(Vector3 pos, int teamNumber, int findRange = -1, NpcType npcTypeToFind = NpcType.Any)
        {
            if (findRange == -1)
            {
                findRange = SceneConfig.FindRange;
            }

            Vector2 pos2D = posToMap(pos.x, pos.z);
            int count = 0;
            GameNpc[] nearNpcs = new GameNpc[SceneConfig.NpcFindCount];

            for (int i = -findRange; i <= findRange; i++)
            {
                for (int j = -findRange; j <= findRange; j++)
                {
                    //is findX, findY is a valid array position
                    int findX = (int)pos2D.x + i;
                    int findY = (int)pos2D.y + j;
                    if (findX >= 0 && findX < width && findY >= 0 && findY < height)
                    {
                        if (AIMap[findX, findY] != null)
                        {
                            if (AIMap[findX, findY].npcType == npcTypeToFind || npcTypeToFind == NpcType.Any)
                            {
                                if (AIMap[findX, findY].teamNumber != teamNumber)
                                {
                                    if (count < SceneConfig.NpcFindCount)
                                    {
                                        nearNpcs[count] = AIMap[findX, findY];
                                    }
                                    else
                                    {
                                        return nearNpcs[Random.Range(0, count)];
                                    }
                                    count++;
                                }
                            }
                        }
                    }
                }

            }
            return null;
        }

        // asign a new enemy team to team, not asign is enemy team flag is eliminated
        public void UpdateEnemyTeam(Team team)
        {
            foreach (Team enemy in HillDefenceCreator.teams)
            {
                if (enemy.teamFlag != null && enemy.teamNumber != team.teamEnemyNumber)
                {
                    team.enemyTeam = enemy;
                    break;
                }
            }
            HillDefenceCreator.instance.EvaluateWin();
        }
        public Vector2 posToMap(float x, float y)
        {
            float xMapNormalized = Mathf.InverseLerp(0, realWidth, x);
            float yMapNormalized = Mathf.InverseLerp(0, realHeight, y);
            int xMap = (int)Mathf.Lerp(0, width, xMapNormalized);
            int yMap = (int)Mathf.Lerp(0, height, yMapNormalized);
            //  print("x: " + x + " y: " + y + " xMap: " + xMap + " yMap: " + yMap);
            return new Vector2(xMap, yMap);
        }

        public Vector2 posToPostionMap(float x, float y)
        {
            float xMapNormalized = Mathf.InverseLerp(0, realWidth, x);
            float yMapNormalized = Mathf.InverseLerp(0, realHeight, y);
            int xMap = (int)Mathf.Lerp(0, 100, xMapNormalized);
            int yMap = (int)Mathf.Lerp(0, 100, yMapNormalized);
            //  print("x: " + x + " y: " + y + " xMap: " + xMap + " yMap: " + yMap);
            return new Vector2(xMap - 50, yMap - 50);
        }


        public Vector3 mapToPos(int x, int y)
        {
            return new Vector3(x * width, 0, y * height);

        }


    }
}
