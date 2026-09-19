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
        [Tooltip("The NPC this healthbar belongs to.")]
        public GameNpc npc;
        private Transform targetTransform;
        private int maxLives;

        [Tooltip("The Image used as the health fill bar.")]
        public Image healthImage;

        [Tooltip("Optional TextMeshProUGUI to display percentage (e.g. 100%).")]
        public TextMeshProUGUI healthText;

        private int _lastShootCount = -1;

        public void SetUp(GameNpc npcInfo, Transform target, int lives)
        {
            npc = npcInfo;
            targetTransform = target;
            maxLives = lives;
            _lastShootCount = -1;
            UpdateBar();
        }

        void LateUpdate()
        {
            if (npc == null || npc.isDead || targetTransform == null)
            {
                Destroy(gameObject);
                return;
            }

            if (npc.shootCount != _lastShootCount)
            {
                _lastShootCount = npc.shootCount;
                UpdateBar();
            }

            // Position directly above the character (using a large enough offset so it's above their heads, not their feet)
            transform.position = targetTransform.position + Vector3.up * 4.5f;
            
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }

        private void UpdateBar()
        {
            if (npc == null) return;

            int lives = Mathf.Max(1, maxLives);
            float hp = Mathf.Clamp01(1f - (float)npc.shootCount / (float)lives);
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
