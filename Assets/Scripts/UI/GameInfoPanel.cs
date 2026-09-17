using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HillDefence
{
    /// <summary>
    /// HUD panel that shows live statistics for every team:
    /// soldiers alive, active towers and flags captured.
    /// Auto-generates UI if references are not assigned.
    /// </summary>
    public class GameInfoPanel : MonoBehaviour
    {
        public static GameInfoPanel instance;

        [Header("References (Optional - will auto-generate if null)")]
        [Tooltip("Parent transform where team rows are generated.")]
        public Transform rowContainer;

        [Tooltip("Prefab for a single team row (must contain a TextMeshProUGUI component).")]
        public GameObject teamRowPrefab;

        private List<TextMeshProUGUI> _rows = new List<TextMeshProUGUI>();
        private GameObject _panelRoot;

        void Awake()
        {
            if (instance == null) instance = this;
            else { Destroy(gameObject); return; }
        }

        /// <summary>Builds the HUD display after teams have been spawned.</summary>
        public void Init()
        {
            _rows.Clear();

            if (rowContainer == null || teamRowPrefab == null)
            {
                CreateDefaultHUD();
            }
            else
            {
                foreach (Transform child in rowContainer)
                {
                    Destroy(child.gameObject);
                }

                foreach (Team team in HillDefenceCreator.teams)
                {
                    GameObject row = Instantiate(teamRowPrefab, rowContainer);
                    TextMeshProUGUI label = row.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null)
                    {
                        label.color = team.teamColor;
                        _rows.Add(label);
                    }
                }
            }

            InvokeRepeating("RefreshStats", 0.1f, 1f / SceneConfig.MapRefreshRate);
        }

        private void CreateDefaultHUD()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Create HUD container in top-left
            _panelRoot = new GameObject("GameInfoHUD");
            _panelRoot.transform.SetParent(canvas.transform, false);

            RectTransform rt = _panelRoot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(15, -15);
            rt.sizeDelta = new Vector2(340, 30 + HillDefenceCreator.teams.Count * 22);

            // Semi-transparent background
            Image bg = _panelRoot.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.75f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_panelRoot.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.anchoredPosition = new Vector2(10, -6);
            titleRt.sizeDelta = new Vector2(-20, 20);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "<b>TEAM STATUS</b>";
            titleText.fontSize = 13;
            titleText.color = Color.white;

            // One row per team
            for (int i = 0; i < HillDefenceCreator.teams.Count; i++)
            {
                Team t = HillDefenceCreator.teams[i];

                GameObject rowObj = new GameObject($"TeamRow_{i}");
                rowObj.transform.SetParent(_panelRoot.transform, false);
                RectTransform rowRt = rowObj.AddComponent<RectTransform>();
                rowRt.anchorMin = new Vector2(0, 1);
                rowRt.anchorMax = new Vector2(1, 1);
                rowRt.pivot = new Vector2(0, 1);
                rowRt.anchoredPosition = new Vector2(10, -28 - (i * 20));
                rowRt.sizeDelta = new Vector2(-20, 18);

                TextMeshProUGUI rowText = rowObj.AddComponent<TextMeshProUGUI>();
                rowText.fontSize = 11;
                rowText.color = t.teamColor;
                rowText.text = $"Team {i}: Initializing...";
                _rows.Add(rowText);
            }
        }

        private void RefreshStats()
        {
            for (int i = 0; i < HillDefenceCreator.teams.Count && i < _rows.Count; i++)
            {
                Team t = HillDefenceCreator.teams[i];
                if (t.teamFlag == null || t.teamFlag.npcInfo.isDead)
                {
                    _rows[i].text = $"<b>Team {i}</b> — <color=#FF4444>DEFEATED</color>";
                }
                else
                {
                    _rows[i].text = $"<b>Team {i}</b>  |  Soldiers: {t.soldiers.Count}  |  Towers: {t.towers.Count}  |  Flags: {t.flagsWinsCount}";
                }
            }
        }

        public void StopRefresh()
        {
            CancelInvoke("RefreshStats");
        }
    }
}
