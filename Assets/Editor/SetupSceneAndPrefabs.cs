using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using HillDefence;

public static class SetupSceneAndPrefabs
{
    [MenuItem("HillDefence/Setup Project")]
    public static void ConfigureAll()
    {
        Debug.Log(">>> Starting automated setup of Scene and Prefabs...");

        ConfigureHealthBarPrefab();
        ConfigureSoldierPrefab();
        ConfigureUIPrefab();
        ConfigureScene();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(">>> Setup completed successfully!");
    }

    private static void ConfigureHealthBarPrefab()
    {
        string path = "Assets/Prefabs/UI/healthLayout.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            // Add World-Space Canvas if missing
            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas == null) canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(60, 6);
            root.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);

            // Find FG image
            Transform fgTrans = root.transform.Find("FG");
            Image fgImage = fgTrans != null ? fgTrans.GetComponent<Image>() : null;
            if (fgImage != null)
            {
                fgImage.type = Image.Type.Filled;
                fgImage.fillMethod = Image.FillMethod.Horizontal;
                fgImage.fillOrigin = 0;
                fgImage.fillAmount = 1f;
                fgImage.color = Color.green;
            }

            // Find or create Percentage Text
            Transform textTrans = root.transform.Find("PercentText");
            TextMeshProUGUI tmpText = null;
            if (textTrans == null)
            {
                GameObject textObj = new GameObject("PercentText");
                textObj.transform.SetParent(root.transform, false);
                RectTransform textRt = textObj.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;
                textRt.anchoredPosition = new Vector2(0, 8);

                tmpText = textObj.AddComponent<TextMeshProUGUI>();
                tmpText.fontSize = 12;
                tmpText.alignment = TextAlignmentOptions.Center;
                tmpText.color = Color.white;
                tmpText.text = "100%";
            }
            else
            {
                tmpText = textTrans.GetComponent<TextMeshProUGUI>();
            }

            // Wire HealthLayout script
            HealthLayout hl = root.GetComponent<HealthLayout>();
            if (hl == null) hl = root.AddComponent<HealthLayout>();
            hl.healthImage = fgImage;
            hl.healthText = tmpText;
            hl.yOffset = 2.5f;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Debug.Log("HealthLayout.prefab configured successfully.");

            // Also copy to Assets/Resources/healthLayout.prefab
            string resourcesDir = "Assets/Resources";
            if (!Directory.Exists(resourcesDir)) Directory.CreateDirectory(resourcesDir);
            File.Copy(path, "Assets/Resources/healthLayout.prefab", true);
            AssetDatabase.ImportAsset("Assets/Resources/healthLayout.prefab");
            Debug.Log("Copied healthLayout to Resources/healthLayout.prefab");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ConfigureSoldierPrefab()
    {
        string path = "Assets/Prefabs/Soldier.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            TeamSoldier soldier = root.GetComponent<TeamSoldier>();
            if (soldier != null)
            {
                GameObject hpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/healthLayout.prefab");
                soldier.healthBarPrefab = hpPrefab;
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log("Soldier.prefab updated with healthBarPrefab.");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ConfigureUIPrefab()
    {
        string path = "Assets/Prefabs/UI.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            MapController mapCtrl = root.GetComponentInChildren<MapController>(true);
            if (mapCtrl != null)
            {
                RawImage[] rawImages = root.GetComponentsInChildren<RawImage>(true);
                foreach (RawImage r in rawImages)
                {
                    if (r.name.ToLower().Contains("map") || r.texture != null || rawImages.Length == 1)
                    {
                        mapCtrl.mapRawImage = r;
                        break;
                    }
                }
                if (mapCtrl.mapRawImage == null && rawImages.Length > 0)
                {
                    mapCtrl.mapRawImage = rawImages[0];
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log("UI.prefab updated with mapRawImage reference.");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ConfigureScene()
    {
        string scenePath = "Assets/Scenes/TerrainHillDefence.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Configure Main Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Disable legacy MobileBloom if present
            var mobileBloom = mainCam.GetComponent("MobileBloom") as MonoBehaviour;
            if (mobileBloom != null)
            {
                mobileBloom.enabled = false;
                Debug.Log("Disabled legacy MobileBloom on Main Camera.");
            }

            // Setup PostProcessLayer
            PostProcessLayer ppLayer = mainCam.GetComponent<PostProcessLayer>();
            if (ppLayer == null)
            {
                ppLayer = mainCam.gameObject.AddComponent<PostProcessLayer>();
            }
            ppLayer.volumeTrigger = mainCam.transform;
            ppLayer.volumeLayer = ~0; // Everything
            ppLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;

            // Assign PostProcessResources
            PostProcessResources res = AssetDatabase.LoadAssetAtPath<PostProcessResources>(
                "Packages/com.unity.postprocessing/PostProcessing/PostProcessResources.asset");
            if (res != null)
            {
                ppLayer.Init(res);
                SerializedObject so = new SerializedObject(ppLayer);
                SerializedProperty prop = so.FindProperty("m_Resources");
                if (prop != null)
                {
                    prop.objectReferenceValue = res;
                    so.ApplyModifiedProperties();
                }
            }
            Debug.Log("Configured PostProcessLayer on Main Camera.");
        }

        // 2. Configure PostProcessVolume
        GameObject ppVolObj = GameObject.Find("PostProcessVolume");
        if (ppVolObj == null)
        {
            ppVolObj = new GameObject("PostProcessVolume");
        }
        PostProcessVolume ppVol = ppVolObj.GetComponent<PostProcessVolume>();
        if (ppVol == null) ppVol = ppVolObj.AddComponent<PostProcessVolume>();
        ppVol.isGlobal = true;
        ppVol.priority = 10;

        CameraPostFX camPostFX = ppVolObj.GetComponent<CameraPostFX>();
        if (camPostFX == null) camPostFX = ppVolObj.AddComponent<CameraPostFX>();

        // 3. Ensure GameInfoPanel is present on UI
        UIController uiCtrl = Object.FindObjectOfType<UIController>();
        if (uiCtrl != null)
        {
            GameInfoPanel gip = uiCtrl.GetComponent<GameInfoPanel>();
            if (gip == null) uiCtrl.gameObject.AddComponent<GameInfoPanel>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Saved configured scene: " + scenePath);
    }
}
