using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HillDefence
{
    public class MapController : MonoBehaviour
    {
        public RenderTexture AIMapTexture;
        private Texture2D AIBitmap;
        public static MapController instance;
        public static int width;
        public static int height;
        public static float realWidth;
        public static float realHeight;
        private static Vector3 worldOrigin;
        public GameObject playerPoiMap;

        [Tooltip("The RawImage that displays the minimap. Assign in Inspector or auto-found.")]
        public RawImage mapRawImage;

        void Awake()
        {
            instance = this;
        }

        public void Init(float terrainWidth, float terrainHeight, Vector3 terrainOrigin, int sizeMap)
        {
            realWidth = terrainWidth;
            realHeight = terrainHeight;
            worldOrigin = terrainOrigin;
            height = width = sizeMap;
            AIBitmap = new Texture2D(width, height);

            // Auto-locate mapRawImage if not manually wired
            if (mapRawImage == null)
            {
                RawImage[] rawImages = GetComponentsInChildren<RawImage>(true);
                foreach (RawImage r in rawImages)
                {
                    if (r.texture == AIMapTexture || r.name.ToLower().Contains("map") || rawImages.Length == 1)
                    {
                        mapRawImage = r;
                        break;
                    }
                }
                if (mapRawImage == null && rawImages.Length > 0)
                {
                    mapRawImage = rawImages[0];
                }
            }

            // Register click handler on the minimap RawImage
            if (mapRawImage != null)
            {
                EventTrigger trigger = mapRawImage.gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = mapRawImage.gameObject.AddComponent<EventTrigger>();

                trigger.triggers.Clear();
                var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                entry.callback.AddListener((data) =>
                {
                    OnMapClick((PointerEventData)data);
                });
                trigger.triggers.Add(entry);
            }
        }

        /// <summary>
        /// Called when the player clicks on the minimap.
        /// Converts the click position to world coordinates and moves the camera there.
        /// </summary>
        public void OnMapClick(PointerEventData eventData)
        {
            if (mapRawImage == null || FlyCamera.instance == null) return;

            RectTransform rt = mapRawImage.rectTransform;
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rt, eventData.position, eventData.pressEventCamera, out localPoint))
                return;

            // Normalize coordinate within rect (independent of pivot/anchors)
            float normX = Mathf.Clamp01((localPoint.x - rt.rect.xMin) / rt.rect.width);
            float normZ = Mathf.Clamp01((localPoint.y - rt.rect.yMin) / rt.rect.height);

            // Convert normalized coordinates to terrain world position
            float worldX = worldOrigin.x + normX * realWidth;
            float worldZ = worldOrigin.z + normZ * realHeight;

            FlyCamera.instance.TeleportTo(new Vector3(worldX, 0, worldZ));
        }

        public void UIMapSetActive(bool active)
        {
            if (active)
            {
                InvokeRepeating("refreshAIMap", 0, 1f / SceneConfig.MapRefreshRate);
            }
            else
            {
                CancelInvoke("refreshAIMap");
            }
        }

        public void refreshAIMap()
        {
            if (AIBitmap == null) return;
            AIBitmap = Utils.FillColorAlpha(AIBitmap);

            foreach (NpcInfo npc in HillDefenceCreator.Npcs)
            {
                if (npc != null && !npc.npcInfo.isDead)
                {
                    Vector2 pos = posToMap(npc.transform.position.x, npc.transform.position.z);
                    if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                    {
                        switch (npc.npcInfo.npcType)
                        {
                            case NpcType.soldier:
                                PaintMarker((int)pos.x, (int)pos.y, 2, HillDefenceCreator.teams[npc.npcInfo.teamNumber].teamColor);
                                break;
                            case NpcType.tower:
                                PaintMarker((int)pos.x, (int)pos.y, 3, HillDefenceCreator.teams[npc.npcInfo.teamNumber].teamColor);
                                break;
                            case NpcType.flag:
                                PaintMarker((int)pos.x, (int)pos.y, 4, HillDefenceCreator.teams[npc.npcInfo.teamNumber].teamColor);
                                break;
                        }
                    }
                }
            }
            AIBitmap.Apply();
            if (AIMapTexture != null)
                Graphics.Blit(AIBitmap, AIMapTexture);

            if (playerPoiMap != null && Camera.main != null)
            {
                Vector2 pos2 = posToPostionMap(Camera.main.transform.position.x, Camera.main.transform.position.z);
                RectTransform poiRt = playerPoiMap.GetComponent<RectTransform>();
                if (poiRt != null && mapRawImage != null)
                {
                    Rect mapRect = mapRawImage.rectTransform.rect;
                    poiRt.anchorMin = new Vector2(0.5f, 0.5f);
                    poiRt.anchorMax = new Vector2(0.5f, 0.5f);
                    poiRt.anchoredPosition = new Vector2(
                        pos2.x * mapRect.width / 100f,
                        pos2.y * mapRect.height / 100f);
                    poiRt.localRotation = Quaternion.Euler(0, 0, -Camera.main.transform.eulerAngles.y);
                }
            }
        }

        private void PaintMarker(int centerX, int centerY, int radius, Color color)
        {
            int minX = Mathf.Max(0, centerX - radius);
            int maxX = Mathf.Min(width - 1, centerX + radius);
            int minY = Mathf.Max(0, centerY - radius);
            int maxY = Mathf.Min(height - 1, centerY + radius);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    AIBitmap.SetPixel(x, y, color);
                }
            }
        }

        public Vector2 posToPostionMap(float x, float y)
        {
            float xMapNormalized = Mathf.InverseLerp(worldOrigin.x, worldOrigin.x + realWidth, x);
            float yMapNormalized = Mathf.InverseLerp(worldOrigin.z, worldOrigin.z + realHeight, y);
            int xMap = (int)Mathf.Lerp(0, 100, xMapNormalized);
            int yMap = (int)Mathf.Lerp(0, 100, yMapNormalized);
            return new Vector2(xMap - 50, yMap - 50);
        }

        public Vector3 mapToPos(int x, int y)
        {
            return new Vector3(x * width, 0, y * height);
        }

        public Vector2 posToMap(float x, float y)
        {
            float xMapNormalized = Mathf.InverseLerp(worldOrigin.x, worldOrigin.x + realWidth, x);
            float yMapNormalized = Mathf.InverseLerp(worldOrigin.z, worldOrigin.z + realHeight, y);
            int xMap = (int)Mathf.Lerp(0, width, xMapNormalized);
            int yMap = (int)Mathf.Lerp(0, height, yMapNormalized);
            return new Vector2(xMap, yMap);
        }
    }
}
