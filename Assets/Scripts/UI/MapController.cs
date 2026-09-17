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
        public GameObject playerPoiMap;

        [Tooltip("The RawImage that displays the minimap. Assign in Inspector or auto-found.")]
        public RawImage mapRawImage;

        void Awake()
        {
            instance = this;
        }

        public void Init(float realSize, int sizeMap)
        {
            realWidth = realHeight = realSize;
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
            float worldX = normX * realWidth;
            float worldZ = normZ * realHeight;

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
                                AIBitmap.SetPixel((int)pos.x, (int)pos.y, HillDefenceCreator.teams[npc.npcInfo.teamNumber].teamColor);
                                break;
                            case NpcType.tower:
                                for (int i = 0; i <= 1; i++)
                                {
                                    for (int j = 0; j <= 1; j++)
                                    {
                                        AIBitmap.SetPixel((int)pos.x + i, (int)pos.y + j, HillDefenceCreator.teams[npc.npcInfo.teamNumber].teamColor);
                                    }
                                }
                                break;
                            case NpcType.flag:
                                for (int i = -2; i <= 2; i++)
                                {
                                    for (int j = -2; j <= 2; j++)
                                    {
                                        if (i != 0 || j != 0)
                                        {
                                            AIBitmap.SetPixel((int)pos.x + i, (int)pos.y + j, HillDefenceCreator.teams[npc.npcInfo.teamNumber].teamColor);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            AIBitmap.Apply();
            Graphics.Blit(AIBitmap, AIMapTexture);

            if (playerPoiMap != null && Camera.main != null)
            {
                Vector2 pos2 = posToPostionMap(Camera.main.transform.position.x, Camera.main.transform.position.z);
                playerPoiMap.GetComponent<RectTransform>().localPosition = new Vector3(pos2.x, pos2.y);
                playerPoiMap.GetComponent<RectTransform>().rotation = new Quaternion(Camera.main.transform.rotation.x, Camera.main.transform.rotation.z, 0, 0);
            }
        }

        public Vector2 posToPostionMap(float x, float y)
        {
            float xMapNormalized = Mathf.InverseLerp(0, realWidth, x);
            float yMapNormalized = Mathf.InverseLerp(0, realHeight, y);
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
            float xMapNormalized = Mathf.InverseLerp(0, realWidth, x);
            float yMapNormalized = Mathf.InverseLerp(0, realHeight, y);
            int xMap = (int)Mathf.Lerp(0, width, xMapNormalized);
            int yMap = (int)Mathf.Lerp(0, height, yMapNormalized);
            return new Vector2(xMap, yMap);
        }
    }
}
