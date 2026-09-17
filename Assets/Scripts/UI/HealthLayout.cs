using UnityEngine;
using UnityEngine.UI;

namespace HillDefence
{
    /// <summary>
    /// Displays a floating health bar above a soldier in World Space.
    /// Attach this to a Canvas (World Space) prefab that is instantiated as a child of each soldier.
    /// </summary>
    public class HealthLayout : MonoBehaviour
    {
        [Tooltip("The soldier this healthbar belongs to.")]
        public TeamSoldier teamSoldier;

        [Tooltip("The Image used as the health fill bar.")]
        public Image healthImage;

        [Tooltip("Vertical offset above the soldier pivot.")]
        public float yOffset = 2.5f;

        private int _lastShootCount = -1;

        public void SetUp(TeamSoldier soldier)
        {
            teamSoldier = soldier;
            _lastShootCount = -1;
            UpdateBar();
        }

        void Update()
        {
            // Destroy healthbar when soldier is gone
            if (!teamSoldier)
            {
                Destroy(gameObject);
                return;
            }

            // Only refresh when shootCount has changed
            if (teamSoldier.npcInfo.shootCount != _lastShootCount)
            {
                _lastShootCount = teamSoldier.npcInfo.shootCount;
                UpdateBar();
            }

            // Keep positioned above soldier, facing the camera
            transform.position = teamSoldier.transform.position + Vector3.up * yOffset;
            if (Camera.main != null)
            {
                transform.forward = Camera.main.transform.forward;
            }
        }

        private void UpdateBar()
        {
            if (healthImage == null || teamSoldier == null) return;

            float hp = Mathf.Clamp01(1f - (float)teamSoldier.npcInfo.shootCount / (float)SceneConfig.SOLDIER.Lives);
            healthImage.fillAmount = hp;

            // Gradient: green (healthy) -> yellow (50%) -> red (critical)
            if (hp > 0.5f)
                healthImage.color = Color.Lerp(Color.yellow, Color.green, (hp - 0.5f) * 2f);
            else
                healthImage.color = Color.Lerp(Color.red, Color.yellow, hp * 2f);
        }
    }
}