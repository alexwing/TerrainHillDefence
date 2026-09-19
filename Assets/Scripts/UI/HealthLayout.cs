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

        [Tooltip("Vertical offset above the character.")]
        public float yOffset = 4.5f;

        [Tooltip("Maximum distance from camera to show the health bar.")]
        public float maxVisibleDistance = 1000f;

        private int _lastShootCount = -1;
        private Canvas _canvas;
        private UnityEngine.UI.Graphic[] _graphics;

        public void SetUp(GameNpc npcInfo, Transform target, int lives, float offset = 4.5f)
        {
            npc = npcInfo;
            targetTransform = target;
            maxLives = lives;
            yOffset = offset;
            _lastShootCount = -1;

            _canvas = GetComponent<Canvas>();
            if (_canvas == null) _canvas = GetComponentInChildren<Canvas>();

            // Remove percentage text as requested
            if (healthText != null)
            {
                healthText.gameObject.SetActive(false);
            }

            // Make it larger
            transform.localScale = transform.localScale * 3.5f;

            // Make it render on top of everything (ZTest Always)
            Material alwaysOnTop = new Material(Shader.Find("UI/Default"));
            alwaysOnTop.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
            
            _graphics = GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            foreach (UnityEngine.UI.Graphic g in _graphics)
            {
                g.material = alwaysOnTop;
            }

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

            // Dynamically calculate position above the highest point of the collider
            float topY = targetTransform.position.y + yOffset; 
            Collider col = targetTransform.GetComponentInChildren<Collider>();
            if (col != null)
            {
                topY = col.bounds.max.y + 1.5f;
            }

            transform.position = new Vector3(targetTransform.position.x, topY, targetTransform.position.z);
            
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;

                // Distance culling: hide if too far from camera
                float distSq = (transform.position - Camera.main.transform.position).sqrMagnitude;
                bool isClose = distSq <= (maxVisibleDistance * maxVisibleDistance);

                if (_canvas != null)
                {
                    if (_canvas.enabled != isClose) _canvas.enabled = isClose;
                }
                else if (_graphics != null)
                {
                    foreach (var g in _graphics)
                    {
                        if (g != null && g.enabled != isClose) g.enabled = isClose;
                    }
                }
            }
        }

        private void UpdateBar()
        {
            if (npc == null) return;

            int lives = Mathf.Max(1, maxLives);
            float hp = Mathf.Clamp01(1f - (float)npc.shootCount / (float)lives);

            if (healthImage != null)
            {
                healthImage.fillAmount = hp;

                // Gradient: green (healthy) -> yellow (50%) -> red (critical)
                if (hp > 0.5f)
                    healthImage.color = Color.Lerp(Color.yellow, Color.green, (hp - 0.5f) * 2f);
                else
                    healthImage.color = Color.Lerp(Color.red, Color.yellow, hp * 2f);
            }
        }
    }
}
