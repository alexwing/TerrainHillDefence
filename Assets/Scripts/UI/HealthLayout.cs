using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HillDefence
{
    /// <summary>
    /// Displays a floating health bar and percentage above a soldier in World Space.
    /// </summary>
    public class HealthLayout : MonoBehaviour
    {
        [Tooltip("The soldier this healthbar belongs to.")]
        public TeamSoldier teamSoldier;

        [Tooltip("The Image used as the health fill bar.")]
        public Image healthImage;

        [Tooltip("Optional TextMeshProUGUI to display percentage (e.g. 100%).")]
        public TextMeshProUGUI healthText;

        [Tooltip("Vertical offset above the soldier pivot.")]
        public float yOffset = 2.5f;

        private int _lastShootCount = -1;

        public void SetUp(TeamSoldier soldier)
        {
            teamSoldier = soldier;
            _lastShootCount = -1;
            UpdateBar();
        }

        void LateUpdate()
        {
            if (!teamSoldier || teamSoldier.npcInfo.isDead)
            {
                Destroy(gameObject);
                return;
            }

            if (teamSoldier.npcInfo.shootCount != _lastShootCount)
            {
                _lastShootCount = teamSoldier.npcInfo.shootCount;
                UpdateBar();
            }

            // Position directly above soldier and match camera view
            transform.position = teamSoldier.transform.position + Vector3.up * yOffset;
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }

        private void UpdateBar()
        {
            if (teamSoldier == null) return;

            int maxLives = Mathf.Max(1, SceneConfig.SOLDIER.Lives);
            float hp = Mathf.Clamp01(1f - (float)teamSoldier.npcInfo.shootCount / (float)maxLives);
            int percent = Mathf.RoundToInt(hp * 100f);

            if (healthImage != null)
            {
                healthImage.fillAmount = hp;

                // Gradient: green (healthy) -> yellow (50%) -> red (critical)
                if (hp > 0.5f)
                    healthImage.color = Color.Lerp(Color.yellow, Color.green, (hp - 0.5f) * 2f);
                else
                    healthImage.color = Color.Lerp(Color.red, Color.yellow, hp * 2f);
            }

            if (healthText != null)
            {
                healthText.text = percent + "%";
            }
        }
    }
}
