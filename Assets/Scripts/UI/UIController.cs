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
        public GameObject winOverlay;
        public Image winOverlayImage;
        public TextMeshProUGUI winStatsText;
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
        }

        /// <summary>Called from LateUpdate on first frame to ensure Canvas is ready.</summary>
        private bool _hudCreated = false;

        void LateUpdate()
        {
            if (!_hudCreated)
            {
                _hudCreated = true;
                CreateHUD();
            }

            // Force minimap to bottom-left every frame (overrides any layout)
            ForceMinimapPosition();
        }

        private void CreateHUD()
        {
            Canvas canvas = FindScreenCanvas();
            if (canvas == null) return;

            // 1. Create Restart Button (Top-Right)
            CreateRestartButton(canvas);

            // 2. Setup Minimap Wrapper (Bottom-Right)
            SetupMinimapWrapper(canvas);

            // 3. Action bar (above minimap, Bottom-Right)
            CreateActionBar(canvas);
        }

        private void CreateRestartButton(Canvas canvas)
        {
            GameObject btnObj = new GameObject("RestartBtn_TopRight");
            btnObj.transform.SetParent(canvas.transform, false);
            RectTransform btnRt = btnObj.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(1, 1); // Top-Right
            btnRt.anchorMax = new Vector2(1, 1);
            btnRt.pivot = new Vector2(1, 1);
            btnRt.anchoredPosition = new Vector2(-10, -10);
            btnRt.sizeDelta = new Vector2(100, 40);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.6f, 0.15f, 0.15f, 0.9f);

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(RestartGame);

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero; txtRt.offsetMax = Vector2.zero;
            
            TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.text = "<b>RESTART</b>";
            txt.fontSize = 14;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
        }

        private void SetupMinimapWrapper(Canvas canvas)
        {
            if (map == null) return;

            // Create a dedicated wrapper at Bottom-Right
            GameObject mapWrapper = new GameObject("MapWrapper");
            mapWrapper.transform.SetParent(canvas.transform, false);
            RectTransform wrapperRt = mapWrapper.AddComponent<RectTransform>();
            wrapperRt.anchorMin = new Vector2(1, 0); // Bottom-Right
            wrapperRt.anchorMax = new Vector2(1, 0);
            wrapperRt.pivot = new Vector2(1, 0);
            wrapperRt.anchoredPosition = new Vector2(-10, 10);
            wrapperRt.sizeDelta = new Vector2(200, 200);

            map.SetActive(isMapVisible);
            map.transform.SetParent(mapWrapper.transform, false);
            
            RectTransform mapRt = map.GetComponent<RectTransform>();
            if (mapRt != null)
            {
                // Stretch map to fill the wrapper and reset any weird scaling/pivots
                mapRt.anchorMin = Vector2.zero;
                mapRt.anchorMax = Vector2.one;
                mapRt.pivot = new Vector2(0.5f, 0.5f);
                mapRt.offsetMin = Vector2.zero;
                mapRt.offsetMax = Vector2.zero;
                mapRt.localScale = Vector3.one;
            }

            if (MapController.instance != null)
                MapController.instance.UIMapSetActive(isMapVisible);
        }

        private void ForceMinimapPosition()
        {
            // No longer needed, wrapper handles it.
        }

        private Canvas FindScreenCanvas()
        {
            Canvas[] all = FindObjectsOfType<Canvas>();
            foreach (Canvas c in all)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                    return c;
            }
            Canvas self = GetComponentInParent<Canvas>();
            if (self != null) return self;
            return all.Length > 0 ? all[0] : null;
        }

        private void CreateActionBar(Canvas canvas)
        {
            GameObject actionBar = new GameObject("ActionBar");
            actionBar.transform.SetParent(canvas.transform, false);

            RectTransform barRt = actionBar.AddComponent<RectTransform>();
            barRt.anchorMin = new Vector2(1, 0); // Bottom-Right
            barRt.anchorMax = new Vector2(1, 0);
            barRt.pivot = new Vector2(1, 0);
            // Position above the minimap wrapper (wrapper is 200 height + 10 padding = 210)
            barRt.anchoredPosition = new Vector2(-10, 220);
            barRt.sizeDelta = new Vector2(55, 55);

            Image barBg = actionBar.AddComponent<Image>();
            barBg.color = new Color(0.08f, 0.08f, 0.12f, 0.75f);

            // Build Turret button
            _buildBtnImage = CreateActionButton(actionBar.transform, "T", "Turret", new Vector2(0, 0),
                () => { isPlacingTurret = !isPlacingTurret; });
        }

        private Image CreateActionButton(Transform parent, string icon, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject($"ActionBtn_{label}");
            btnObj.transform.SetParent(parent, false);
            RectTransform btnRt = btnObj.AddComponent<RectTransform>();
            btnRt.anchorMin = Vector2.zero;
            btnRt.anchorMax = Vector2.one;
            btnRt.offsetMin = Vector2.zero;
            btnRt.offsetMax = Vector2.zero;

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            Button btn = btnObj.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.4f, 0.4f, 0.5f, 1f);
            cb.pressedColor = new Color(0.5f, 0.5f, 0.6f, 1f);
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);
            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0, 0.35f);
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
            labelRt.anchorMax = new Vector2(1, 0.35f);
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
            // Toggle map with M
            if (Input.GetKeyUp(KeyCode.M) && map != null)
            {
                isMapVisible = !isMapVisible;
                map.SetActive(isMapVisible);
                if (MapController.instance != null)
                    MapController.instance.UIMapSetActive(isMapVisible);
            }

            // Highlight build button
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
                                        PlaceTurret(hit.point, foundTeamFlag);
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
                if (winOverlayImage != null) { Color c = team.teamColor; c.a = 0.65f; winOverlayImage.color = c; }
                if (winText != null) { winText.gameObject.SetActive(true); winText.text = $"TEAM {team.teamNumber} WINS!"; winText.color = team.teamColor; }
                if (winStatsText != null) { winStatsText.gameObject.SetActive(true); winStatsText.text = $"Soldiers remaining: {team.soldiers.Count}\nTowers active: {team.towers.Count}\nFlags captured: {team.flagsWinsCount}"; }
                if (restartButton != null) restartButton.SetActive(true);
            }
            else
            {
                CreateDefaultWinScreen(team);
            }
        }

        private void CreateDefaultWinScreen(Team team)
        {
            Canvas canvas = FindScreenCanvas();
            if (canvas == null) return;

            GameObject modal = new GameObject("WinModal");
            modal.transform.SetParent(canvas.transform, false);
            RectTransform modalRt = modal.AddComponent<RectTransform>();
            modalRt.anchorMin = new Vector2(0.5f, 0.5f);
            modalRt.anchorMax = new Vector2(0.5f, 0.5f);
            modalRt.pivot = new Vector2(0.5f, 0.5f);
            modalRt.sizeDelta = new Vector2(450, 260);
            Image modalBg = modal.AddComponent<Image>();
            Color bgColor = team.teamColor * 0.4f; bgColor.a = 0.9f; modalBg.color = bgColor;

            GameObject titleObj = new GameObject("WinTitle");
            titleObj.transform.SetParent(modal.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1); titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1); titleRt.anchoredPosition = new Vector2(0, -20); titleRt.sizeDelta = new Vector2(-20, 50);
            TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.text = $"<b>TEAM {team.teamNumber} VICTORIOUS!</b>"; titleTxt.fontSize = 26;
            titleTxt.alignment = TextAlignmentOptions.Center; titleTxt.color = Color.white;

            GameObject statsObj = new GameObject("WinStats");
            statsObj.transform.SetParent(modal.transform, false);
            RectTransform statsRt = statsObj.AddComponent<RectTransform>();
            statsRt.anchorMin = new Vector2(0, 0.35f); statsRt.anchorMax = new Vector2(1, 0.75f);
            statsRt.pivot = new Vector2(0.5f, 0.5f); statsRt.anchoredPosition = Vector2.zero; statsRt.sizeDelta = new Vector2(-40, 0);
            TextMeshProUGUI statsTxt = statsObj.AddComponent<TextMeshProUGUI>();
            statsTxt.text = $"<b>Soldiers:</b> {team.soldiers.Count}\n<b>Towers:</b> {team.towers.Count}\n<b>Flags:</b> {team.flagsWinsCount}";
            statsTxt.fontSize = 16; statsTxt.alignment = TextAlignmentOptions.Center; statsTxt.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            GameObject btnObj = new GameObject("RestartButton");
            btnObj.transform.SetParent(modal.transform, false);
            RectTransform btnRt = btnObj.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0); btnRt.anchorMax = new Vector2(0.5f, 0);
            btnRt.pivot = new Vector2(0.5f, 0); btnRt.anchoredPosition = new Vector2(0, 20); btnRt.sizeDelta = new Vector2(180, 45);
            Image btnImg = btnObj.AddComponent<Image>(); btnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f);
            Button btn = btnObj.AddComponent<Button>(); btn.onClick.AddListener(RestartGame);
            GameObject btnTextObj = new GameObject("BtnText");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRt = btnTextObj.AddComponent<RectTransform>();
            btnTextRt.anchorMin = Vector2.zero; btnTextRt.anchorMax = Vector2.one; btnTextRt.sizeDelta = Vector2.zero;
            TextMeshProUGUI btnTxt = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnTxt.text = "<b>PLAY AGAIN</b>"; btnTxt.fontSize = 16;
            btnTxt.alignment = TextAlignmentOptions.Center; btnTxt.color = Color.white;
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void CreateHealthbars()
        {
            if (healthLayoutHolder != null)
                for (int i = 0; i < healthLayoutHolder.childCount; i++)
                    Destroy(healthLayoutHolder.GetChild(i).gameObject);
        }
    }
}
