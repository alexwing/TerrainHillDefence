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
        private float[] _curveLookup;

        void Start()
        {
            detonationBulletPrefab.transform.localScale = new Vector3(SceneConfig.TERRAIN.detonationBulletSize, SceneConfig.TERRAIN.detonationBulletSize, SceneConfig.TERRAIN.detonationBulletSize);
            
            // Precompute animation curve lookup for extreme performance boost
            _curveLookup = new float[1024];
            for (int i = 0; i < 1024; i++)
            {
                _curveLookup[i] = analogIntensityCurve.Evaluate(i / 1023f);
            }
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

            x = x < 0 ? 0 : x;
            y = y < 0 ? 0 : y;
            int sizex = x + size > hmWidth ? hmWidth - x : size;
            int sizey = y + size > hmHeight ? hmHeight - y : size;

            float[,] areaT = null;
            try
            {
                areaT = terr.terrainData.GetHeights(x, y, sizex, sizey);
            }
            catch (System.Exception e)
            {
                Debug.LogError("GetHeights" + e.Message.ToString());
                return; // Abort if heightmap fetching fails
            }

            // Optimize multiplication
            float intensityMult = destructionIntensity / 100f;
            float radio = size * 0.5f;

            for (int i = 0; i < areaT.GetLength(0); i++)
            {
                for (int j = 0; j < areaT.GetLength(1); j++)
                {
                    float texPixel = GetBeizerFast(i, j, radio);
                    if (type)
                    {
                        areaT[i, j] += texPixel * intensityMult;
                    }
                    else
                    {
                        areaT[i, j] -= texPixel * intensityMult;
                    }
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
        
        private float GetBeizerFast(int i, int j, float radio)
        {
            // Fast distance calc
            float dx = i - radio;
            float dy = j - radio;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float norm = dist / radio;
            if (norm >= 1f) return 0f;
            
            // Fast lookup
            int idx = (int)(norm * 1023f);
            return _curveLookup[idx];
        }


        private Mesh rockMesh;
        private Material rockMat;

        public void DetonationBullet(GameObject collision)
        {
            if (rockMesh == null)
            {
                GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rockMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
                Destroy(tempCube);

                rockMat = new Material(Shader.Find("Legacy Shaders/Diffuse"));
                rockMat.color = new Color(0.55f, 0.50f, 0.45f, 1f); // Gris/Marrón claro
            }
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
                ParticleSystem[] pss = obj.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in pss)
                {
                    var main = ps.main;
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    
                    ParticleSystemRenderer render = ps.GetComponent<ParticleSystemRenderer>();
                    string psName = ps.gameObject.name.ToLower();

                    if (psName.Contains("sparkle") || psName.Contains("debris"))
                    {
                        // Convert into small 3D tumbling rocks
                        if (render != null && rockMesh != null)
                        {
                            render.renderMode = ParticleSystemRenderMode.Mesh;
                            render.mesh = rockMesh;
                            render.material = rockMat;
                        }
                        
                        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.25f); // Tiny rocks
                        main.startColor = new Color(0.55f, 0.50f, 0.45f, 1f);
                        main.gravityModifier = 2.0f; 

                        var rot = ps.rotationOverLifetime;
                        rot.enabled = true;
                        rot.xMultiplier = 360f;
                        rot.yMultiplier = 360f;
                        rot.zMultiplier = 360f;
                    }
                    else
                    {
                        // Restore billboard mode for smoke/flash to avoid giant cubes
                        if (render != null)
                        {
                            render.renderMode = ParticleSystemRenderMode.Billboard;
                        }
                        
                        // Tint to dirt/smoke (avoiding bright reds/yellows of fire)
                        main.startColor = new Color(0.45f, 0.40f, 0.35f, 0.6f);
                    }

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

                // Cap the number of random explosions to avoid massive FPS drops (e.g. 100+ explosions for a flag)
                int numExplosions = Mathf.Min((int)destructionSize * 2, 8);
                
                for (int i = 0; i < numExplosions; i++)
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

                int numExplosions = Mathf.Min((int)destructionSize * 2, 8);
                for (int i = 0; i < numExplosions; i++)
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
