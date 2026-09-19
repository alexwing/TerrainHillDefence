using UnityEngine;
namespace HillDefence
{
    public class TargetTerrain : MonoBehaviour
    {

        public static TargetTerrain instance;
        // public TerrainData tData;
        public GameObject detonationBulletPrefab;
        public GameObject detonationTowerPrefab;
        public GameObject detonationPrefab;

        [Header("Terrain Destrucion")]


        [Tooltip("Up true Down False")]
        public bool UpOrDown = false;

        public AnimationCurve analogIntensityCurve;

        [Header("Sound Destrucion")]
        public AudioClip clip;
        [Tooltip("Width of terrain destruction")]
        [Range(0, 1500)]
        public int DistanceSoundLimit = 500;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);

            }
        }
        void Start()
        {
            detonationBulletPrefab.transform.localScale = new Vector3(SceneConfig.TERRAIN.detonationBulletSize, SceneConfig.TERRAIN.detonationBulletSize, SceneConfig.TERRAIN.detonationBulletSize);
        }

        void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.tag == "bullet")
            {
                ModifyTerrain(collision.gameObject, 10, 10, UpOrDown);
                DetonationBullet(collision.gameObject);
                
                if (ObjectPooler.instance != null)
                    ObjectPooler.instance.ReturnToPool(collision.gameObject);
                else
                    Destroy(collision.gameObject);
            }

        }

        public void ModifyTerrain(GameObject collision, float destructionSize, float destructionIntensity, bool type = false)
        {
            Terrain terr = GetComponent<Terrain>();
            // get the normalized position of this game object relative to the terrain
            Vector3 tempCoord = (collision.transform.position - terr.gameObject.transform.position);
            Vector3 coord;

            int hmWidth = terr.terrainData.heightmapResolution;
            int hmHeight = terr.terrainData.heightmapResolution;

            coord.x = tempCoord.x / terr.terrainData.size.x;
            coord.y = tempCoord.y / terr.terrainData.size.y;
            coord.z = tempCoord.z / terr.terrainData.size.z;

            //int size = brush.width;
            int size = (int)destructionSize;
            int offset = Mathf.RoundToInt(size / 2);

            int x = (int)(coord.x * hmWidth) - offset;
            int y = (int)(coord.z * hmHeight) - offset;


            float[,] areaT = new float[size, size];


            x = x < 0 ? 0 : x;
            y = y < 0 ? 0 : y;
            int sizex = x + size > hmWidth ? hmWidth - x : size;
            int sizey = y + size > hmHeight ? hmHeight - y : size;
            try
            {
                areaT = terr.terrainData.GetHeights(x, y, sizex, sizey);
            }
            catch (System.Exception e)
            {
                Debug.LogError("GetHeights" + e.Message.ToString());
            }

            for (int i = 0; i < areaT.GetLength(0); i++)
            {
                for (int j = 0; j < areaT.GetLength(1); j++)
                {
                    try
                    {
                        float texPixel = GetBeizer(i, j, size);
                        if (type)
                        {
                            areaT[i, j] += texPixel / 100 * destructionIntensity;
                        }
                        else
                        {
                            areaT[i, j] -= texPixel / 100 * destructionIntensity;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("areaT[i, j]" + e.Message.ToString());
                    }

                    //areaT[i, j] = 0;
                }
            }
            try
            {
                terr.terrainData.SetHeights(x, y, areaT);
            }
            catch (System.Exception e)
            {
                Debug.LogError("SetHeights " + e.Message.ToString());
            }
        }
        private float GetBeizer(int i, int j, int size)
        {
            //Create hole from beizer curve and matriz radio
            float radio = size * 0.5f;
            float radioDistance = Vector2.Distance(new Vector2(i, j), new Vector2(radio, radio));
            float normalizedRadio = Mathf.InverseLerp(0, radio, radioDistance);
            float beizerRadio = analogIntensityCurve.Evaluate(Mathf.Lerp(0, 1f, normalizedRadio));
            return beizerRadio;

        }


        public void DetonationBullet(GameObject collision)
        {
            GameObject obj = null;
            if (ObjectPooler.instance != null)
            {
                obj = ObjectPooler.instance.SpawnFromPool(detonationBulletPrefab, collision.transform.position, Quaternion.identity);
                ReturnToPoolAfterTime returner = obj.GetComponent<ReturnToPoolAfterTime>();
                if (returner == null) returner = obj.AddComponent<ReturnToPoolAfterTime>();
                returner.lifeTime = SceneConfig.TERRAIN.explosionBulletLife;
            }
            else
            {
                obj = Instantiate(detonationBulletPrefab, collision.transform.position, Quaternion.identity);
                Destroy(obj, SceneConfig.TERRAIN.explosionBulletLife);
            }

            if (obj != null)
            {
                // Force particles to play (required for object pooling) and tint them to look like dirt/smoke instead of fire
                ParticleSystem[] pss = obj.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in pss)
                {
                    var main = ps.main;
                    
                    // Force the particle system to simulate and play from the beginning
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    
                    // Tint to dirt/smoke (avoiding bright reds/yellows of fire)
                    main.startColor = new Color(0.4f, 0.38f, 0.35f, 0.8f);
                    
                    ps.Play(true);
                }
            }
        }

        public void DetonationTerrain(GameObject collision, float destructionSize)
        {
            if (ObjectPooler.instance != null)
            {
                GameObject detonation = ObjectPooler.instance.SpawnFromPool(detonationPrefab, collision.transform.position, Quaternion.identity);
                detonation.transform.localScale = new Vector3(destructionSize, destructionSize, destructionSize);
                ReturnToPoolAfterTime returner1 = detonation.GetComponent<ReturnToPoolAfterTime>();
                if (returner1 == null) returner1 = detonation.AddComponent<ReturnToPoolAfterTime>();
                returner1.lifeTime = SceneConfig.TERRAIN.explosionLife;

                GameObject _currentEffect = ObjectPooler.instance.SpawnFromPool(collision.gameObject, collision.transform.position, Quaternion.identity);
                for (int i = 0; i < _currentEffect.transform.childCount; i++)
                {
                    _currentEffect.transform.GetChild(i).transform.localScale = new Vector3(destructionSize, destructionSize, destructionSize) * 0.1f;
                }
                ReturnToPoolAfterTime returner2 = _currentEffect.GetComponent<ReturnToPoolAfterTime>();
                if (returner2 == null) returner2 = _currentEffect.AddComponent<ReturnToPoolAfterTime>();
                returner2.lifeTime = 0.5f;

                for (int i = 0; i < destructionSize * 2; i++)
                {
                    GameObject p = ObjectPooler.instance.SpawnFromPool(detonationPrefab, Utils.RandomNearPosition(collision.transform, SceneConfig.TERRAIN.ramdomExplosion, 0f, SceneConfig.TERRAIN.ramdomExplosion).position, Quaternion.identity);
                    ReturnToPoolAfterTime r = p.GetComponent<ReturnToPoolAfterTime>();
                    if (r == null) r = p.AddComponent<ReturnToPoolAfterTime>();
                    r.lifeTime = SceneConfig.TERRAIN.explosionLife;
                }

                detonationTowerPrefab.transform.localScale = new Vector3(destructionSize, destructionSize, destructionSize);
                GameObject t = ObjectPooler.instance.SpawnFromPool(detonationTowerPrefab, collision.transform.position, Quaternion.identity);
                ReturnToPoolAfterTime rt = t.GetComponent<ReturnToPoolAfterTime>();
                if (rt == null) rt = t.AddComponent<ReturnToPoolAfterTime>();
                rt.lifeTime = SceneConfig.TERRAIN.explosionLife;
            }
            else
            {
                // Fallback if pooler missing
                GameObject detonation = Instantiate(detonationPrefab, collision.transform.position, Quaternion.identity) as GameObject;
                detonation.transform.localScale = new Vector3(destructionSize, destructionSize, destructionSize);
                Destroy(detonation, SceneConfig.TERRAIN.explosionLife);

                GameObject _currentEffect = Instantiate(collision.gameObject, collision.transform.position, Quaternion.identity);
                for (int i = 0; i < _currentEffect.transform.childCount; i++)
                {
                    _currentEffect.transform.GetChild(i).transform.localScale = new Vector3(destructionSize, destructionSize, destructionSize) * 0.1f;
                }
                Destroy(_currentEffect, 0.5f);

                for (int i = 0; i < destructionSize * 2; i++)
                {
                    Destroy(Instantiate(detonationPrefab, Utils.RandomNearPosition(collision.transform, SceneConfig.TERRAIN.ramdomExplosion, 0f, SceneConfig.TERRAIN.ramdomExplosion).position, Quaternion.identity), SceneConfig.TERRAIN.explosionLife);
                }

                detonationTowerPrefab.transform.localScale = new Vector3(destructionSize, destructionSize, destructionSize);
                Destroy(Instantiate(detonationTowerPrefab, collision.transform.position, Quaternion.identity), SceneConfig.TERRAIN.explosionLife);
            }

            // Big-base explosion
            if (destructionSize >= SceneConfig.FLAG.DetonationSize)
            {
                GameObject fxHost = new GameObject("FlagExplosionFX");
                FlagExplosionFX fx = fxHost.AddComponent<FlagExplosionFX>();
                fx.Play(collision.transform.position, destructionSize, detonationPrefab, detonationTowerPrefab);
            }
        }
    }
}
