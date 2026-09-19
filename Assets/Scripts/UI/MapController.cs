using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HillDefence
{
    public class MapController : MonoBehaviour
    {
        public static MapController instance;

        public static float realWidth;
        public static float realHeight;
        private static Vector3 worldOrigin;
        
        public GameObject playerPoiMap;

        public RenderTexture AIMapTexture; // Restored!

        [Tooltip("The RawImage (or just an Image) that acts as the minimap background.")]
        public RawImage mapRawImage;

        private RectTransform markerContainer;
        private List<Image> markerPool = new List<Image>();

        void Awake()
        {
            instance = this;
        }

        public void Init(float terrainWidth, float terrainHeight, Vector3 terrainOrigin, int sizeMap)
        {
            realWidth = terrainWidth;
            realHeight = terrainHeight;
            worldOrigin = terrainOrigin;

            // Failsafe: if AIMapTexture was lost from the inspector, try to find it
            if (AIMapTexture == null)
            {
                RenderTexture[] rts = Resources.FindObjectsOfTypeAll<RenderTexture>();
                foreach (RenderTexture rt in rts)
                {
                    if (rt.name.Contains("Map") || rt.name.Contains("AIMap"))
                    {
                        AIMapTexture = rt;
                        break;
                    }
                }
                
                // Still null? Try to find the minimap camera and grab its target texture!
                if (AIMapTexture == null)
                {
                    Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                    foreach (Camera cam in cameras)
                    {
                        if (cam.name.Contains("Camera") && cam.targetTexture != null && cam != Camera.main)
                        {
                            AIMapTexture = cam.targetTexture;
                            break;
                        }
                    }
                }
            }

            // Auto-locate mapRawImage if not manually wired
            if (mapRawImage == null)
            {
                RawImage[] rawImages = GetComponentsInChildren<RawImage>(true);
                foreach (RawImage r in rawImages)
                {
                    if (r.name.ToLower().Contains("map") || rawImages.Length == 1)
                    {
                        mapRawImage = r;
                        break;
                    }
                }
                if (mapRawImage == null && rawImages.Length > 0)
                {
                    mapRawImage = rawImages[0];
                }
                
                // FAILSAFE: If the user deleted the RawImage, recreate it!
                if (mapRawImage == null)
                {
                    GameObject newRaw = new GameObject("MapRawImage_AutoGened");
                    newRaw.transform.SetParent(this.transform, false);
                    mapRawImage = newRaw.AddComponent<RawImage>();
                    
                    RectTransform newRt = mapRawImage.rectTransform;
                    newRt.anchorMin = Vector2.zero;
                    newRt.anchorMax = Vector2.one;
                    newRt.offsetMin = Vector2.zero;
                    newRt.offsetMax = Vector2.zero;
                }
            }

            // Create a container for our UI markers so they are properly scaled
            if (mapRawImage != null)
            {
                // Ensure the texture is assigned
                if (mapRawImage.texture == null && AIMapTexture != null)
                {
                    mapRawImage.texture = AIMapTexture;
                }

                // Force mapRawImage out of any hidden containers (like "Positions") and onto the safe root
                if (mapRawImage.transform.parent != this.transform)
                {
                    mapRawImage.transform.SetParent(this.transform, false);
                    RectTransform rawRt = mapRawImage.rectTransform;
                    if (rawRt != null) {
                        rawRt.anchorMin = Vector2.zero;
                        rawRt.anchorMax = Vector2.one;
                        rawRt.offsetMin = Vector2.zero;
                        rawRt.offsetMax = Vector2.zero;
                    }
                }

                GameObject containerObj = new GameObject("MarkerContainer");
                markerContainer = containerObj.AddComponent<RectTransform>();
                
                // Attach directly to the safe MiniMap root (this.transform) which is guaranteed to be 350x350
                markerContainer.SetParent(this.transform, false);
                markerContainer.anchorMin = Vector2.zero;
                markerContainer.anchorMax = Vector2.one;
                markerContainer.offsetMin = Vector2.zero;
                markerContainer.offsetMax = Vector2.zero;
                
                // Ensure it's rendered on top
                markerContainer.SetAsLastSibling();

                EventTrigger trigger = mapRawImage.gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = mapRawImage.gameObject.AddComponent<EventTrigger>();

                trigger.triggers.Clear();
                var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                entry.callback.AddListener((data) =>
                {
                    OnMapClick((PointerEventData)data);
                });
                trigger.triggers.Add(entry);
                
                // Make sure the RawImage is visible and white to show the relief map
                mapRawImage.color = Color.white;
                mapRawImage.gameObject.SetActive(true);
            }
        }

        public void OnMapClick(PointerEventData eventData)
        {
            if (mapRawImage == null || FlyCamera.instance == null) return;

            RectTransform rt = mapRawImage.rectTransform;
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rt, eventData.position, eventData.pressEventCamera, out localPoint))
                return;

            float normX = Mathf.Clamp01((localPoint.x - rt.rect.xMin) / rt.rect.width);
            float normZ = Mathf.Clamp01((localPoint.y - rt.rect.yMin) / rt.rect.height);

            float worldX = worldOrigin.x + normX * realWidth;
            float worldZ = worldOrigin.z + normZ * realHeight;

            FlyCamera.instance.TeleportTo(new Vector3(worldX, 0, worldZ));
        }

        public void UIMapSetActive(bool active)
        {
            if (active)
                InvokeRepeating("refreshAIMap", 0, 1f / SceneConfig.MapRefreshRate);
            else
                CancelInvoke("refreshAIMap");
        }

        public void refreshAIMap()
        {
            if (markerContainer == null) return;

            int poolIndex = 0;

            foreach (NpcInfo npc in HillDefenceCreator.Npcs)
            {
                if (npc != null && !npc.npcInfo.isDead)
                {
                    // Calculate normalized coordinates
                    float normX = Mathf.InverseLerp(worldOrigin.x, worldOrigin.x + realWidth, npc.transform.position.x);
                    float normY = Mathf.InverseLerp(worldOrigin.z, worldOrigin.z + realHeight, npc.transform.position.z);

                    if (normX >= 0f && normX <= 1f && normY >= 0f && normY <= 1f)
                    {
                        Image marker = GetMarker(poolIndex);
                        marker.gameObject.SetActive(true);
                        
                        // Scale based on type (made smaller per request)
                        float size = 3f;
                        switch (npc.npcInfo.npcType)
                        {
                            case NpcType.soldier: size = 3f; break;
                            case NpcType.tower: size = 5f; break;
                            case NpcType.flag: size = 8f; break;
                        }
                        
                        marker.rectTransform.sizeDelta = new Vector2(size, size);
                        marker.color = HillDefenceCreator.teams[npc.npcInfo.teamNumber].teamColor;

                        // Position it relative to the container using anchors
                        marker.rectTransform.anchorMin = new Vector2(normX, normY);
                        marker.rectTransform.anchorMax = new Vector2(normX, normY);
                        marker.rectTransform.anchoredPosition = Vector2.zero;

                        poolIndex++;
                    }
                }
            }

            // Hide unused markers
            for (int i = poolIndex; i < markerPool.Count; i++)
            {
                markerPool[i].gameObject.SetActive(false);
            }

            // Update Player POI camera cone
            if (playerPoiMap != null && Camera.main != null && mapRawImage != null)
            {
                float camNormX = Mathf.InverseLerp(worldOrigin.x, worldOrigin.x + realWidth, Camera.main.transform.position.x);
                float camNormY = Mathf.InverseLerp(worldOrigin.z, worldOrigin.z + realHeight, Camera.main.transform.position.z);

                RectTransform poiRt = playerPoiMap.GetComponent<RectTransform>();
                if (poiRt != null)
                {
                    // Re-parent to marker container if not already to share the same scale
                    if (poiRt.parent != markerContainer)
                    {
                        poiRt.SetParent(markerContainer, false);
                        // Make sure the POI map is drawn on top
                        poiRt.SetAsLastSibling();
                    }

                    poiRt.anchorMin = new Vector2(camNormX, camNormY);
                    poiRt.anchorMax = new Vector2(camNormX, camNormY);
                    poiRt.anchoredPosition = Vector2.zero;
                    poiRt.localRotation = Quaternion.Euler(0, 0, -Camera.main.transform.eulerAngles.y);
                    poiRt.gameObject.SetActive(true);
                }
            }
        }

        private Image GetMarker(int index)
        {
            if (index < markerPool.Count)
            {
                return markerPool[index];
            }

            // Create new marker
            GameObject obj = new GameObject("MapMarker_" + index);
            obj.transform.SetParent(markerContainer, false);
            
            Image img = obj.AddComponent<Image>();
            img.raycastTarget = false; // Don't block clicks on the map
            
            RectTransform rt = img.rectTransform;
            rt.pivot = new Vector2(0.5f, 0.5f);
            
            markerPool.Add(img);
            return img;
        }
    }
}
