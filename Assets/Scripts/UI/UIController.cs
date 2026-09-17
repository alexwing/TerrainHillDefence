using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        public Image winOverlayImage;
        [Tooltip("Extra TextMeshProUGUI inside winOverlay for final stats.")]
        public TextMeshProUGUI winStatsText;
        [Tooltip("Button to restart the scene shown on the win screen.")]
        public GameObject restartButton;

        [Header("HUD")]
        public Transform healthLayoutHolder;
        public GameObject healthLayout;
        public GameObject map;

        public bool isMapVisible = true;
        public bool isHudVisible = true;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (map != null) map.SetActive(isMapVisible);
            FlyCamera.lockMovement = isMapVisible;
            
            if (winOverlay != null) winOverlay.SetActive(false);
            if (restartButton != null) restartButton.SetActive(false);
            if (winText != null) winText.gameObject.SetActive(false);
            
            CreateUIOptions();
        }

        public bool isPlacingTurret = false;

        private void Update()
        {
            // Toggle map display with M
            if (Input.GetKeyUp(KeyCode.M) && map != null)
            {
                isMapVisible = !isMapVisible;
                map.SetActive(isMapVisible);
                if (MapController.instance != null)
                {
                    MapController.instance.UIMapSetActive(isMapVisible);
                }
                // When map is open, unlock cursor so player can click on it
                FlyCamera.lockMovement = isMapVisible;
            }

            // Turret Placement State Machine
            if (isPlacingTurret)
            {
                if (Input.GetMouseButtonDown(1)) // Right click to cancel
                {
                    isPlacingTurret = false;
                    if (cursorPointer != null) cursorPointer.SetActive(false);
                    return;
                }

                if (anchorToTerrain)
                {
                    RaycastHit hit;
                    if (Camera.main != null && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10000f))
                    {
                        if (hit.collider.gameObject == anchorToTerrain.gameObject)
                        {
                            GameNpc foundTeamFlag = AIController.instance.getNearNpc(hit.point, -1, -1, NpcType.flag);

                            if (cursorPointer != null)
                            {
                                cursorPointer.SetActive(true);
                                cursorPointer.GetComponent<BoxCollider>().enabled = false;
                                cursorPointer.transform.position = hit.point;

                                // Pulse animation using Lerp with White (works on Opaque materials)
                                float pulse = Mathf.PingPong(Time.time * 3f, 0.5f);

                                if (foundTeamFlag != null)
                                {
                                    Color baseC = HillDefenceCreator.teams[foundTeamFlag.teamNumber].teamColor;
                                    Color pulseC = Color.Lerp(baseC, Color.white, pulse);
                                    Utils.ChangeColor(cursorPointer.GetComponent<TeamTower>().towerMaterial, pulseC);

                                    // Left click to place (ignore UI clicks)
                                    bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                                    if (Input.GetMouseButtonDown(0) && !isOverUI)
                                    {
                                        GameObject newTower = Instantiate(cursorPointer, hit.point, Quaternion.identity) as GameObject;
                                        TeamTower teamTower = newTower.GetComponent<TeamTower>();
                                        teamTower.GetComponent<BoxCollider>().enabled = true;
                                        
                                        // Reset alpha for the real tower
                                        Color realC = HillDefenceCreator.teams[foundTeamFlag.teamNumber].teamColor;
                                        realC.a = 1f;
                                        Utils.ChangeColor(teamTower.towerMaterial, realC);

                                        teamTower.npcInfo.teamNumber = foundTeamFlag.teamNumber;
                                        teamTower.npcInfo.npcNumber = HillDefenceCreator.teams[foundTeamFlag.teamNumber].towers.Count;
                                        teamTower.npcInfo.npcType = NpcType.tower;
                                        teamTower.npcInfo.npcObject = teamTower.gameObject;

                                        teamTower.name = "Tower_" + teamTower.npcInfo.teamNumber + "_" + teamTower.npcInfo.npcNumber;
                                        teamTower.Init();
                                        HillDefenceCreator.teams[foundTeamFlag.teamNumber].towers.Add(teamTower);
                                        HillDefenceCreator.Npcs.Add(teamTower);

                                        isPlacingTurret = false; // Exit placement mode
                                        cursorPointer.SetActive(false);
                                    }
                                }
                                else
                                {
                                    Color pulseC = Color.Lerp(Color.black, Color.red, pulse);
                                    Utils.ChangeColor(cursorPointer.GetComponent<TeamTower>().towerMaterial, pulseC);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (cursorPointer != null && cursorPointer.activeSelf)
                {
                    cursorPointer.SetActive(false);
                }
            }
        }

        public void ShowWin(Team team)
        {
            if (GameInfoPanel.instance != null)
                GameInfoPanel.instance.StopRefresh();

            // Unlock mouse cursor for win screen interaction
            FlyCamera.lockMovement = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (winOverlay != null)
            {
                winOverlay.SetActive(true);
                if (winOverlayImage != null)
                {
                    Color c = team.teamColor;
                    c.a = 0.65f;
                    winOverlayImage.color = c;
                }
                if (winText != null)
                {
                    winText.gameObject.SetActive(true);
                    winText.text = $"TEAM {team.teamNumber} WINS!";
                    winText.color = team.teamColor;
                }
                if (winStatsText != null)
                {
                    winStatsText.gameObject.SetActive(true);
                    winStatsText.text =
                        $"Soldiers remaining: {team.soldiers.Count}\n" +
                        $"Towers active: {team.towers.Count}\n" +
                        $"Flags captured: {team.flagsWinsCount}";
                }
                if (restartButton != null)
                {
                    restartButton.SetActive(true);
                }
            }
            else
            {
                CreateDefaultWinScreen(team);
            }
        }

        private void CreateDefaultWinScreen(Team team)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject modal = new GameObject("WinModal");
            modal.transform.SetParent(canvas.transform, false);

            RectTransform modalRt = modal.AddComponent<RectTransform>();
            modalRt.anchorMin = new Vector2(0.5f, 0.5f);
            modalRt.anchorMax = new Vector2(0.5f, 0.5f);
            modalRt.pivot = new Vector2(0.5f, 0.5f);
            modalRt.sizeDelta = new Vector2(450, 260);

            Image modalBg = modal.AddComponent<Image>();
            Color bgColor = team.teamColor * 0.4f;
            bgColor.a = 0.9f;
            modalBg.color = bgColor;

            // Title
            GameObject titleObj = new GameObject("WinTitle");
            titleObj.transform.SetParent(modal.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.anchoredPosition = new Vector2(0, -20);
            titleRt.sizeDelta = new Vector2(-20, 50);

            TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.text = $"<b>TEAM {team.teamNumber} VICTORIOUS!</b>";
            titleTxt.fontSize = 26;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.color = Color.white;

            // Stats
            GameObject statsObj = new GameObject("WinStats");
            statsObj.transform.SetParent(modal.transform, false);
            RectTransform statsRt = statsObj.AddComponent<RectTransform>();
            statsRt.anchorMin = new Vector2(0, 0.35f);
            statsRt.anchorMax = new Vector2(1, 0.75f);
            statsRt.pivot = new Vector2(0.5f, 0.5f);
            statsRt.anchoredPosition = Vector2.zero;
            statsRt.sizeDelta = new Vector2(-40, 0);

            TextMeshProUGUI statsTxt = statsObj.AddComponent<TextMeshProUGUI>();
            statsTxt.text =
                $"<b>Soldiers Remaining:</b> {team.soldiers.Count}\n" +
                $"<b>Towers Active:</b> {team.towers.Count}\n" +
                $"<b>Flags Captured:</b> {team.flagsWinsCount}";
            statsTxt.fontSize = 16;
            statsTxt.alignment = TextAlignmentOptions.Center;
            statsTxt.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            // Restart button
            GameObject btnObj = new GameObject("RestartButton");
            btnObj.transform.SetParent(modal.transform, false);
            RectTransform btnRt = btnObj.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0);
            btnRt.anchorMax = new Vector2(0.5f, 0);
            btnRt.pivot = new Vector2(0.5f, 0);
            btnRt.anchoredPosition = new Vector2(0, 20);
            btnRt.sizeDelta = new Vector2(180, 45);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f);

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(RestartGame);

            GameObject btnTextObj = new GameObject("BtnText");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRt = btnTextObj.AddComponent<RectTransform>();
            btnTextRt.anchorMin = Vector2.zero;
            btnTextRt.anchorMax = Vector2.one;
            btnTextRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnTxt = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnTxt.text = "<b>PLAY AGAIN</b>";
            btnTxt.fontSize = 16;
            btnTxt.alignment = TextAlignmentOptions.Center;
            btnTxt.color = Color.white;
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void CreateHealthbars()
        {
            if (healthLayoutHolder != null)
            {
                for (int i = 0; i < healthLayoutHolder.childCount; i++)
                {
                    Destroy(healthLayoutHolder.GetChild(i).gameObject);
                }
            }
        }
        
        private void CreateUIOptions()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject optionsPanel = new GameObject("OptionsPanel");
            optionsPanel.transform.SetParent(canvas.transform, false);

            RectTransform panelRt = optionsPanel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(1, 1);
            panelRt.anchorMax = new Vector2(1, 1);
            panelRt.pivot = new Vector2(1, 1);
            panelRt.anchoredPosition = new Vector2(-10, -10);
            panelRt.sizeDelta = new Vector2(150, 145); // Increased height

            // Toggle Map Button
            Button mapBtn = CreateButton(optionsPanel.transform, "Toggle Map (M)", new Vector2(0, 0));
            mapBtn.onClick.AddListener(() =>
            {
                isMapVisible = !isMapVisible;
                if (map != null) map.SetActive(isMapVisible);
                if (MapController.instance != null) MapController.instance.UIMapSetActive(isMapVisible);
                FlyCamera.lockMovement = isMapVisible;
            });

            // Toggle HUD Button
            Button hudBtn = CreateButton(optionsPanel.transform, "Toggle HUD", new Vector2(0, -45));
            hudBtn.onClick.AddListener(() =>
            {
                isHudVisible = !isHudVisible;
                if (GameInfoPanel.instance != null) GameInfoPanel.instance.gameObject.SetActive(isHudVisible);
            });

            // Build Turret Button
            Button buildBtn = CreateButton(optionsPanel.transform, "Build Turret", new Vector2(0, -90));
            buildBtn.onClick.AddListener(() =>
            {
                isPlacingTurret = !isPlacingTurret;
            });
        }

        private Button CreateButton(Transform parent, string textStr, Vector2 pos)
        {
            GameObject btnObj = new GameObject("OptionButton");
            btnObj.transform.SetParent(parent, false);
            RectTransform btnRt = btnObj.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 1);
            btnRt.anchorMax = new Vector2(0.5f, 1);
            btnRt.pivot = new Vector2(0.5f, 1);
            btnRt.anchoredPosition = pos;
            btnRt.sizeDelta = new Vector2(140, 40);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Button btn = btnObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.text = textStr;
            txt.fontSize = 14;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;

            return btn;
        }
    }
}
