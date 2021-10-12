using TMPro;
using UnityEngine;

namespace HillDefence
{
    public class UIController : MonoBehaviour
    {

        public static UIController instance;
        public GameObject cursorPointer;
        public Terrain anchorToTerrain;

        public TextMeshProUGUI winText;

        public Transform healthLayoutHolder;
        public GameObject healthLayout;
        public GameObject map;

        public bool isMapVisible = false;



        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);

            }
            map.SetActive(false);
        }

        private void Update()
        {
            //show/hide map
            if (Input.GetKeyUp(KeyCode.M))
            {
                isMapVisible = !isMapVisible;
                map.SetActive(isMapVisible);
                MapController.instance.UIMapSetActive(isMapVisible);
            }

            //raycast mouse cursor to objetct pointer in terrain
            if (Input.GetMouseButton(0) && anchorToTerrain)
            {
                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10000f))
                {
                    if (hit.collider.gameObject == anchorToTerrain.gameObject)
                    {
                        //find the near flag tower in the terrain
                        GameNpc foundTeamTower = null;

                        foundTeamTower = AIController.instance.getNearNpc(hit.point, -1, -1, NpcType.flag);

                        cursorPointer.SetActive(true);
                        //disable cursorPointer collider
                        cursorPointer.GetComponent<BoxCollider>().enabled = false;

                        //  Debug.Log("position x: " + hit.transform.position.x + " position z: " + hit.transform.position.z);
                        cursorPointer.transform.position = hit.point;
                        if (foundTeamTower != null)
                        {
                            Utils.ChangeColor(cursorPointer.GetComponent<TeamTower>().towerMaterial, HillDefenceCreator.teams[foundTeamTower.teamNumber].teamColor);
                            if (Utils.DoubleClick())
                            {
                                //instanciate a new cursor pointer
                                GameObject newTower = Instantiate(cursorPointer, hit.point, Quaternion.identity) as GameObject;
                                TeamTower teamTower = newTower.GetComponent<TeamTower>();
                                teamTower.GetComponent<BoxCollider>().enabled = true;
                                if (teamTower != null)
                                {
                                    teamTower.npcInfo.teamNumber = foundTeamTower.teamNumber;
                                    teamTower.npcInfo.npcNumber = HillDefenceCreator.teams[foundTeamTower.teamNumber].towers.Count - 1;
                                    teamTower.npcInfo.npcType = NpcType.tower;
                                    teamTower.npcInfo.npcObject = teamTower.gameObject;

                                    teamTower.name = "Tower_" + teamTower.npcInfo.teamNumber + "_" + teamTower.npcInfo.npcNumber;
                                    teamTower.Init();
                                    HillDefenceCreator.teams[foundTeamTower.teamNumber].towers.Add(teamTower);
                                    HillDefenceCreator.Npcs.Add(teamTower);

                                }
                            }
                        }
                        else
                        {
                            Utils.ChangeColor(cursorPointer.GetComponent<TeamTower>().towerMaterial, Color.black);
                        }
                    }
                }
            }
            else
            {
                //hide cursor pointer when no mouse click
                cursorPointer.SetActive(false);
            }
        }

        public void ShowWin(Team Team)
        {
            winText.transform.gameObject.SetActive(true);
            winText.text = Team.teamNumber + " wins!";
            // winText.color = Team.teamColor;

        }

        public void CreateHealthbars()
        {
            // Clear healthbars
            for (int i = 0; i < healthLayoutHolder.childCount; i++)
            {
                Destroy(healthLayoutHolder.GetChild(i).gameObject);
            }

            // print ("CreateHealthbars"+HillDefenceCreator.soldiers.Count);

            /*for (int i = 0; i < HillDefenceCreator.soldiers.Count; i++)
            {
                CreateHealthbar(HillDefenceCreator.soldiers[i]);
            }*/

        }

        void CreateHealthbar(TeamSoldier teamSoldier)
        {
            GameObject newHealthbar = GameObject.Instantiate(healthLayout, healthLayoutHolder);
            newHealthbar.transform.SetParent(healthLayoutHolder);
            newHealthbar.transform.position = Vector3.zero;

            newHealthbar.GetComponent<HealthLayout>().SetUp(teamSoldier);
        }
    }

}