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

        [Tooltip("Ramdom explosion particles system")]
        [Range(0f, 1f)]
        public float ramdomExplosion = 1.0f;


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
            int size = (int) destructionSize;
            int offset = Mathf.RoundToInt(size / 2);

            int x = (int)(coord.x * hmWidth) - offset;
            int y = (int)(coord.z * hmHeight) - offset;


            float[,] areaT;
            try
            {
                areaT = terr.terrainData.GetHeights(x, y, size, size);
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
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

                        //areaT[i, j] = 0;
                    }
                }
                terr.terrainData.SetHeights(x, y, areaT);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message.ToString());
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
            Destroy(Instantiate(detonationBulletPrefab, collision.transform.position, Quaternion.identity), SceneConfig.TERRAIN.explosionBulletLife);
        }

        public void DetonationTerrain(GameObject collision, float destructionSize)
        {

           // detonationPrefab.transform.localScale = new Vector3(destructionSize, destructionSize, destructionSize);
     //      GameObject detonation = Instantiate(detonationPrefab, collision.transform.position, Quaternion.identity) as GameObject;
            Destroy(Instantiate(detonationPrefab, collision.transform.position, Quaternion.identity), SceneConfig.TERRAIN.explosionLife);

            GameObject _currentEffect = Instantiate(collision.gameObject, collision.transform.position, Quaternion.identity);

            for (int i = 0; i < _currentEffect.transform.childCount; i++)
            {
                _currentEffect.transform.GetChild(i).transform.localScale = new Vector3(destructionSize, destructionSize, destructionSize) * 0.1f;
            }
            Destroy(_currentEffect, 0.5f);
            for (int i = 0; i < destructionSize; i++)
            {
                Destroy(Instantiate(detonationPrefab, Utils.RandomNearPosition(collision.transform, ramdomExplosion, 0f, ramdomExplosion).position, Quaternion.identity), SceneConfig.TERRAIN.explosionLife);
            }
            detonationTowerPrefab.transform.localScale = new Vector3(SceneConfig.TERRAIN.detonationFlagSize, SceneConfig.TERRAIN.detonationFlagSize, SceneConfig.TERRAIN.detonationFlagSize);
            Destroy(Instantiate(detonationTowerPrefab, collision.transform.position, Quaternion.identity), SceneConfig.TERRAIN.explosionLife);
        }
    }
}
