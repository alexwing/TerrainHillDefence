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

        [Tooltip("The RawImage that displays the minimap. Assign in Inspector.")]
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

            // Register click handler on the minimap RawImage
            if (mapRawImage != null)
            {
                EventTrigger trigger = mapRawImage.gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = mapRawImage.gameObject.AddComponent<EventTrigger>();

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
        /// Converts the click position to a world position and teleports the camera there.
        /// </summary>
        private void OnMapClick(PointerEventData eventData)
        {
            if (mapRawImage == null || FlyCamera.instance == null) return;

            // Convert screen point to local point inside the RawImage rect
            RectTransform rt = mapRawImage.rectTransform;
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rt, eventData.position, eventData.pressEventCamera, out localPoint))
                return;

            // Normalize to [0,1] — localPoint ranges from (-width/2, -height/2) to (width/2, height/2)
            float normX = (localPoint.x / rt.rect.width) + 0.5f;
            float normZ = (localPoint.y / rt.rect.height) + 0.5f;

            // Convert normalized map coords to world XZ
            float worldX = Mathf.Lerp(0, realWidth,  normX);
            float worldZ = Mathf.Lerp(0, realHeight, normZ);

            // Close the map and teleport the camera
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
            AIBitmap = Utils.FillColorAlpha(AIBitmap);

            foreach (NpcInfo npc in HillDefenceCreator.Npcs)
            {
                if (!npc.npcInfo.isDead && npc != null)
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

            //update position of playerPoiMap from camera main to aiMap with same position
            Vector2 pos2 = posToPostionMap(Camera.main.transform.position.x, Camera.main.transform.position.z);
            playerPoiMap.GetComponent<RectTransform>().localPosition = new Vector3(pos2.x, pos2.y);

            Console.WriteLine("refreshAIMap");

            playerPoiMap.GetComponent<RectTransform>().rotation = new Quaternion(Camera.main.transform.rotation.x, Camera.main.transform.rotation.z, 0, 0);
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
