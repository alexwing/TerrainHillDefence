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
        public bool isPlacingTurret = false;

        private Image _buildBtnImage;

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

            if (winOverlay != null) winOverlay.SetActive(false);
            if (restartButton != null) restartButton.SetActive(false);
            if (winText != null) winText.gameObject.SetActive(false);

            CreateHUD();
        }

        private void CreateHUD()
        {
            // ── MINIMAP: Force to absolute bottom-left ──
            RepositionMinimap();

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // ── ACTION BAR: Left side, above minimap ──
            CreateActionBar(canvas);
        }

        private void RepositionMinimap()
        {
            if (map == null) return;

            map.SetActive(isMapVisible);

            // Force the map container AND its children to bottom-left
            RectTransform mapRt = map.GetComponent<RectTransform>();
            if (mapRt != null)
            {
                mapRt.anchorMin = new Vector2(0, 0);
                mapRt.anchorMax = new Vector2(0, 0);
                mapRt.pivot = new Vector2(0, 0);
                mapRt.anchoredPosition = new Vector2(5, 5);
            }

            if (MapController.instance != null)
            {
                MapController.instance.UIMapSetActive(isMapVisible);
            }
        }

        private void CreateActionBar(Canvas canvas)
        {
            GameObject actionBar = new GameObject("ActionBar");
            actionBar.transform.SetParent(canvas.transform, false);

            RectTransform barRt = actionBar.AddComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0, 0);
            barRt.anchorMax = new Vector2(0, 0);
            barRt.pivot = new Vector2(0, 0);
            barRt.anchoredPosition = new Vector2(10, 210);
            barRt.sizeDelta = new Vector2(55, 60);

            Image barBg = actionBar.AddComponent<Image>();
            barBg.color = new Color(0.08f, 0.08f, 0.12f, 0.75f);

            // Build Turret button (text only, no unicode icons)
            _buildBtnImage = CreateActionButton(actionBar.transform, "T", "Turret", new Vector2(0, 0),
                () => { isPlacingTurret = !isPlacingTurret; });
        }

        private Image CreateActionButton(Transform parent, string icon, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject($"ActionBtn_{label}");
            btnObj.transform.SetParent(parent, false);
            RectTransform btnRt = btnObj.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0, 1);
            btnRt.anchorMax = new Vector2(1, 1);
            btnRt.pivot = new Vector2(0.5f, 1);
            btnRt.anchoredPosition = pos;
            btnRt.sizeDelta = new Vector2(0, 55);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            Button btn = btnObj.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.4f, 0.4f, 0.5f, 1f);
            cb.pressedColor = new Color(0.5f, 0.5f, 0.6f, 1f);
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            // Icon letter
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);
            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0, 0.4f);
            iconRt.anchorMax = new Vector2(1, 1);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;

            TextMeshProUGUI iconTxt = iconObj.AddComponent<TextMeshProUGUI>();
            iconTxt.text = $"<b>{icon}</b>";
            iconTxt.fontSize = 22;
            iconTxt.alignment = TextAlignmentOptions.Center;
            iconTxt.color = Color.white;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);
            RectTransform labelRt = labelObj.AddComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0, 0);
            labelRt.anchorMax = new Vector2(1, 0.4f);
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            TextMeshProUGUI labelTxt = labelObj.AddComponent<TextMeshProUGUI>();
            labelTxt.text = label;
            labelTxt.fontSize = 9;
            labelTxt.alignment = TextAlignmentOptions.Center;
            labelTxt.color = new Color(0.7f, 0.7f, 0.7f, 1f);

            return btnImg;
        }

        private void Update()
        {
            // Toggle map with M key
            if (Input.GetKeyUp(KeyCode.M) && map != null)
            {
                isMapVisible = !isMapVisible;
                map.SetActive(isMapVisible);
                if (MapController.instance != null)
                    MapController.instance.UIMapSetActive(isMapVisible);
            }

            // Highlight build button when active
            if (_buildBtnImage != null)
            {
                _buildBtnImage.color = isPlacingTurret
                    ? Color.Lerp(new Color(0.2f, 0.5f, 0.2f, 1f), new Color(0.3f, 0.7f, 0.3f, 1f), Mathf.PingPong(Time.time * 2f, 1f))
                    : new Color(0.2f, 0.2f, 0.25f, 0.9f);
            }

            // Turret placement
            if (isPlacingTurret)
            {
                if (Input.GetMouseButtonDown(1))
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

                                float pulse = Mathf.PingPong(Time.time * 3f, 0.5f);

                                if (foundTeamFlag != null)
                                {
                                    Color baseC = HillDefenceCreator.teams[foundTeamFlag.teamNumber].teamColor;
                                    Color pulseC = Color.Lerp(baseC, Color.white, pulse);
                                    Utils.ChangeColor(cursorPointer.GetComponent<TeamTower>().towerMaterial, pulseC);

                                    bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                                    if (Input.GetMouseButtonDown(0) && !isOverUI)
                                    {
                                        PlaceTurret(hit.point, foundTeamFlag);
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
                    cursorPointer.SetActive(false);
            }
        }

        private void PlaceTurret(Vector3 position, GameNpc nearFlag)
        {
            GameObject newTower = Instantiate(cursorPointer, position, Quaternion.identity);
            TeamTower teamTower = newTower.GetComponent<TeamTower>();
            teamTower.GetComponent<BoxCollider>().enabled = true;

            Color realC = HillDefenceCreator.teams[nearFlag.teamNumber].teamColor;
            realC.a = 1f;
            Utils.ChangeColor(teamTower.towerMaterial, realC);

            teamTower.npcInfo.teamNumber = nearFlag.teamNumber;
            teamTower.npcInfo.npcNumber = HillDefenceCreator.teams[nearFlag.teamNumber].towers.Count;
            teamTower.npcInfo.npcType = NpcType.tower;
            teamTower.npcInfo.npcObject = teamTower.gameObject;

            teamTower.name = "Tower_" + teamTower.npcInfo.teamNumber + "_" + teamTower.npcInfo.npcNumber;
            teamTower.Init();
            HillDefenceCreator.teams[nearFlag.teamNumber].towers.Add(teamTower);
            HillDefenceCreator.Npcs.Add(teamTower);

            isPlacingTurret = false;
            cursorPointer.SetActive(false);
        }

        public void ShowWin(Team team)
        {
            if (GameInfoPanel.instance != null)
                GameInfoPanel.instance.StopRefresh();

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
                if (restartButton != null) restartButton.SetActive(true);
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
                    Destroy(healthLayoutHolder.GetChild(i).gameObject);
            }
        }
    }
}
