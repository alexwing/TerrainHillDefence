using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HillDefence
{
    /// <summary>
    /// HUD panel that shows live statistics for every team:
    /// soldiers alive, active towers and flags captured.
    /// </summary>
    public class GameInfoPanel : MonoBehaviour
    {
        public static GameInfoPanel instance;

        [Header("References")]
        [Tooltip("Parent transform where team rows are generated.")]
        public Transform rowContainer;

        [Tooltip("Prefab for a single team row (must contain a TextMeshProUGUI component).")]
        public GameObject teamRowPrefab;

        // Cached rows, one per team
        private List<TextMeshProUGUI> _rows = new List<TextMeshProUGUI>();

        void Awake()
        {
            if (instance == null) instance = this;
            else { Destroy(gameObject); return; }
        }

        /// <summary>Call once after teams are created to build the row list.</summary>
        public void Init()
        {
            // Clear any existing rows
            foreach (Transform child in rowContainer)
                Destroy(child.gameObject);
            _rows.Clear();

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

            InvokeRepeating("RefreshStats", 0f, 1f / SceneConfig.MapRefreshRate);
        }

        private void RefreshStats()
        {
            for (int i = 0; i < HillDefenceCreator.teams.Count && i < _rows.Count; i++)
            {
                Team t = HillDefenceCreator.teams[i];
                if (t.teamFlag == null || t.teamFlag.npcInfo.isDead)
                {
                    _rows[i].text = $"Team {i} — DEFEATED";
                    continue;
                }
                _rows[i].text =
                    $"Team {i}  |  Soldiers: {t.soldiers.Count}  |  Towers: {t.towers.Count}  |  Flags captured: {t.flagsWinsCount}";
            }
        }

        public void StopRefresh() => CancelInvoke("RefreshStats");
    }
}
