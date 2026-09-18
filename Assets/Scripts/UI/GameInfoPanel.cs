using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HillDefence
{
    /// <summary>
    /// Top bar HUD: One button per team flag showing color, health, soldiers alive/total.
    /// Click a flag button to fly the camera to that flag.
    /// Must be on the same GameObject as UIController (which has the Canvas).
    /// </summary>
    public class GameInfoPanel : MonoBehaviour
    {
        public static GameInfoPanel instance;

        private List<FlagButton> _flagButtons = new List<FlagButton>();
        private GameObject _panelRoot;
        private int _initialSoldierCount;

        private class FlagButton
        {
            public int teamIndex;
            public GameObject root;
            public Image bgImage;
            public Image healthFill;
            public TextMeshProUGUI label;
        }

        void Awake()
        {
            if (instance == null) instance = this;
            else { Destroy(this); return; }
        }

        public void Init()
        {
            _flagButtons.Clear();
            _initialSoldierCount = HillDefenceCreator.instance.enemiesPerTeam;
            CreateTopBar();
            InvokeRepeating("RefreshStats", 0.5f, 0.5f);
        }

        /// <summary>
        /// Finds the Screen Space Overlay canvas (not WorldSpace healthbar canvases).
        /// </summary>
        private Canvas FindScreenCanvas()
        {
            Canvas[] all = FindObjectsOfType<Canvas>();
            foreach (Canvas c in all)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                    return c;
            }
            // Fallback: try the canvas on this gameobject
            Canvas self = GetComponentInParent<Canvas>();
            if (self != null) return self;
            // Last resort
            return all.Length > 0 ? all[0] : null;
        }

        private void CreateTopBar()
        {
            Canvas canvas = FindScreenCanvas();
            if (canvas == null)
            {
                Debug.LogError("GameInfoPanel: No Screen Space Canvas found!");
                return;
            }

            int teamCount = HillDefenceCreator.teams.Count;
            float btnWidth = 140f;
            float btnHeight = 45f;
            float spacing = 4f;

            // Container anchored at TOP-LEFT
            _panelRoot = new GameObject("FlagStatusBar");
            _panelRoot.transform.SetParent(canvas.transform, false);
            RectTransform containerRt = _panelRoot.AddComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0, 1);
            containerRt.anchorMax = new Vector2(0, 1);
            containerRt.pivot = new Vector2(0, 1);
            containerRt.anchoredPosition = new Vector2(5, -5);
            containerRt.sizeDelta = new Vector2(btnWidth, teamCount * (btnHeight + spacing));

            for (int i = 0; i < teamCount; i++)
            {
                Team team = HillDefenceCreator.teams[i];
                float yPos = -i * (btnHeight + spacing);

                // Button root
                GameObject btnObj = new GameObject($"FlagBtn_{i}");
                btnObj.transform.SetParent(_panelRoot.transform, false);
                RectTransform btnRt = btnObj.AddComponent<RectTransform>();
                btnRt.anchorMin = new Vector2(0, 1);
                btnRt.anchorMax = new Vector2(1, 1);
                btnRt.pivot = new Vector2(0, 1);
                btnRt.anchoredPosition = new Vector2(0, yPos);
                btnRt.sizeDelta = new Vector2(0, btnHeight);

                // Background
                Image bgImg = btnObj.AddComponent<Image>();
                Color bgCol = team.teamColor * 0.3f;
                bgCol.a = 0.85f;
                bgImg.color = bgCol;

                // Left color stripe (the "flag")
                GameObject stripe = new GameObject("FlagStripe");
                stripe.transform.SetParent(btnObj.transform, false);
                RectTransform stripeRt = stripe.AddComponent<RectTransform>();
                stripeRt.anchorMin = new Vector2(0, 0);
                stripeRt.anchorMax = new Vector2(0, 1);
                stripeRt.pivot = new Vector2(0, 0.5f);
                stripeRt.anchoredPosition = Vector2.zero;
                stripeRt.sizeDelta = new Vector2(8, 0);
                Image stripeImg = stripe.AddComponent<Image>();
                stripeImg.color = team.teamColor;

                // Health bar at bottom
                GameObject hpBg = new GameObject("HpBg");
                hpBg.transform.SetParent(btnObj.transform, false);
                RectTransform hpBgRt = hpBg.AddComponent<RectTransform>();
                hpBgRt.anchorMin = new Vector2(0, 0);
                hpBgRt.anchorMax = new Vector2(1, 0);
                hpBgRt.pivot = new Vector2(0, 0);
                hpBgRt.anchoredPosition = new Vector2(10, 2);
                hpBgRt.sizeDelta = new Vector2(-14, 5);
                Image hpBgImg = hpBg.AddComponent<Image>();
                hpBgImg.color = new Color(0.1f, 0.1f, 0.1f, 1f);

                // Health bar fill
                GameObject hpFill = new GameObject("HpFill");
                hpFill.transform.SetParent(hpBg.transform, false);
                RectTransform hpFillRt = hpFill.AddComponent<RectTransform>();
                hpFillRt.anchorMin = Vector2.zero;
                hpFillRt.anchorMax = Vector2.one;
                hpFillRt.pivot = new Vector2(0, 0.5f);
                hpFillRt.offsetMin = Vector2.zero;
                hpFillRt.offsetMax = Vector2.zero;
                Image hpFillImg = hpFill.AddComponent<Image>();
                hpFillImg.color = team.teamColor;
                hpFillImg.type = Image.Type.Filled;
                hpFillImg.fillMethod = Image.FillMethod.Horizontal;
                hpFillImg.fillOrigin = 0;
                hpFillImg.fillAmount = 1f;

                // Label text
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(btnObj.transform, false);
                RectTransform labelRt = labelObj.AddComponent<RectTransform>();
                labelRt.anchorMin = new Vector2(0, 0.12f);
                labelRt.anchorMax = new Vector2(1, 1);
                labelRt.pivot = new Vector2(0.5f, 0.5f);
                labelRt.offsetMin = new Vector2(12, 0);
                labelRt.offsetMax = new Vector2(-4, -2);

                TextMeshProUGUI labelTxt = labelObj.AddComponent<TextMeshProUGUI>();
                labelTxt.fontSize = 11;
                labelTxt.alignment = TextAlignmentOptions.MidlineLeft;
                labelTxt.color = Color.white;
                labelTxt.text = $"<b>Team {i}</b>\nSoldiers: {_initialSoldierCount}/{_initialSoldierCount}";
                labelTxt.enableWordWrapping = false;
                labelTxt.overflowMode = TextOverflowModes.Ellipsis;

                // Outline for selection highlight
                Outline outline = btnObj.AddComponent<Outline>();
                outline.effectColor = Color.white;
                outline.effectDistance = new Vector2(2, -2);
                outline.enabled = false;

                // Click to teleport and select
                Button btn = btnObj.AddComponent<Button>();
                btn.transition = Selectable.Transition.ColorTint;
                ColorBlock cb = btn.colors;
                cb.highlightedColor = new Color(0.35f, 0.35f, 0.4f, 1f);
                cb.pressedColor = new Color(0.45f, 0.45f, 0.5f, 1f);
                btn.colors = cb;
                int capturedIndex = i;
                btn.onClick.AddListener(() => OnFlagClicked(capturedIndex));

                _flagButtons.Add(new FlagButton
                {
                    teamIndex = i,
                    root = btnObj,
                    bgImage = bgImg,
                    healthFill = hpFillImg,
                    label = labelTxt
                });
            }
        }

        private void OnFlagClicked(int teamIndex)
        {
            if (teamIndex < 0 || teamIndex >= HillDefenceCreator.teams.Count) return;
            
            if (UIController.instance != null)
            {
                UIController.instance.SelectTeam(teamIndex);
            }

            Team t = HillDefenceCreator.teams[teamIndex];
            if (t.teamFlag != null && !t.teamFlag.npcInfo.isDead && FlyCamera.instance != null)
            {
                FlyCamera.instance.TeleportTo(t.teamFlag.transform.position);
            }

            RefreshStats();
        }

        private void RefreshStats()
        {
            int selectedTeam = UIController.instance != null ? UIController.instance.selectedTeamIndex : -1;

            for (int i = 0; i < _flagButtons.Count && i < HillDefenceCreator.teams.Count; i++)
            {
                Team t = HillDefenceCreator.teams[i];
                FlagButton fb = _flagButtons[i];

                Outline outline = fb.root.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = (i == selectedTeam);
                }

                if (t.teamFlag == null || t.teamFlag.npcInfo.isDead)
                {
                    fb.label.text = $"<b>Team {i}</b>\n<color=#FF4444>DEFEATED</color>";
                    fb.healthFill.fillAmount = 0;
                    Color dead = new Color(0.15f, 0.15f, 0.15f, 0.7f);
                    fb.bgImage.color = dead;
                }
                else
                {
                    int alive = t.soldiers.Count;
                    int towers = t.towers.Count;
                    float flagHp = 1f - (float)t.teamFlag.npcInfo.shootCount / SceneConfig.FLAG.Lives;
                    flagHp = Mathf.Clamp01(flagHp);

                    fb.healthFill.fillAmount = flagHp;
                    fb.label.text = $"<b>Team {i}</b>\nSoldiers: {alive}/{_initialSoldierCount}  Towers: {towers}";
                }
            }
        }

        public void StopRefresh()
        {
            CancelInvoke("RefreshStats");
        }
    }
}
