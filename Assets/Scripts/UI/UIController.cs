using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HillDefence
{
    public class UIController : MonoBehaviour
    {

        public static UIController instance;
        public GameObject cursorPointer;
        public Terrain anchorToTerrain;

        [Header("Win Screen")]
        public TextMeshProUGUI winText;
        [Tooltip("Semi-transparent overlay panel that covers the screen on win.")]
        public GameObject winOverlay;
        [Tooltip("Image component of winOverlay, used to tint it with the winner team colour.")]
        public UnityEngine.UI.Image winOverlayImage;
        [Tooltip("Extra TextMeshProUGUI inside winOverlay for final stats.")]
        public TextMeshProUGUI winStatsText;
        [Tooltip("Button to restart the scene shown on the win screen.")]
        public GameObject restartButton;

        [Header("HUD")]
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
            if (winOverlay != null) winOverlay.SetActive(false);
            if (restartButton != null) restartButton.SetActive(false);
            if (winText != null) winText.gameObject.SetActive(false);
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

        public void ShowWin(Team team)
        {
            // Stop game info refresh
            if (GameInfoPanel.instance != null)
                GameInfoPanel.instance.StopRefresh();

            // Activate win overlay and tint it with the winner team colour
            if (winOverlay != null)
            {
                winOverlay.SetActive(true);
                if (winOverlayImage != null)
                {
                    Color overlayColor = team.teamColor;
                    overlayColor.a = 0.55f;
                    winOverlayImage.color = overlayColor;
                }
            }

            // Main win text
            if (winText != null)
            {
                winText.gameObject.SetActive(true);
                winText.text = $"TEAM {team.teamNumber} WINS!";
                winText.color = team.teamColor;
            }

            // Final stats
            if (winStatsText != null)
            {
                winStatsText.gameObject.SetActive(true);
                winStatsText.text =
                    $"Soldiers remaining: {team.soldiers.Count}\n" +
                    $"Towers active: {team.towers.Count}\n" +
                    $"Flags captured: {team.flagsWinsCount}";
            }

            // Show restart button
            if (restartButton != null)
                restartButton.SetActive(true);
        }

        /// <summary>Called by the Restart button in the Win screen.</summary>
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void CreateHealthbars()
        {
            // Clear healthbars
            for (int i = 0; i < healthLayoutHolder.childCount; i++)
            {
                Destroy(healthLayoutHolder.GetChild(i).gameObject);
            }
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