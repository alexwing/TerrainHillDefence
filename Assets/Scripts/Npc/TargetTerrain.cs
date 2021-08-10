using UnityEngine;
namespace HillDefence
{
    public class TargetTerrain : MonoBehaviour
    {

public static TargetTerrain instance;
        // public TerrainData tData;
        public GameObject detonationBulletPrefab;
        public GameObject detonationPrefab;
        public float explosionLife = 10;

        [Header("Terrain Destrucion")]
        [Tooltip("Height of terrain destruction")]
        [Range(0f, 40f)]
        public float terrainDestructHeight = 20f;
        [Tooltip("Width of terrain destruction")]
        [Range(0f, 50f)]
        public float terrainDestructWidth = 10f;
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

        void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.tag == "bullet")
            {
                ModifyTerrain(collision.gameObject, 10, 10, UpOrDown);
                DetonationBullet(collision.gameObject, 1);
                Destroy(collision.gameObject);
            }

        }

        public void ModifyTerrain(GameObject collision,float destructionSize, float destructionIntensity,  bool type = false)
        {

            float normalizedValue = Mathf.InverseLerp(0, 100, destructionSize);
            //  int brushSize = (int) Mathf.Lerp(0, Listbrush.Length-1, normalizedValue);
            int brushSize = (int)Mathf.Lerp(0, terrainDestructWidth, normalizedValue);

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
            int size = brushSize;
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


        public void DetonationBullet(GameObject collision, float destructionSize)
        {
            Destroy(Instantiate(detonationBulletPrefab, collision.transform.position, Quaternion.identity), explosionLife);            
        }

        public void DetonationTerrain(GameObject collision, float destructionSize)
        {
            Destroy(Instantiate(detonationPrefab, collision.transform.position, Quaternion.identity), explosionLife);
            float normalizedValue = Mathf.InverseLerp(0, 100, destructionSize);
            float explosionSize = Mathf.Lerp(0, terrainDestructWidth, normalizedValue);
            GameObject _currentEffect = Instantiate(collision.gameObject, collision.transform.position, Quaternion.identity);

            for (int i = 0; i < _currentEffect.transform.childCount; i++)
            {
                _currentEffect.transform.GetChild(i).transform.localScale = new Vector3(explosionSize, explosionSize, explosionSize) * 0.1f;
            }
            Destroy(_currentEffect, 0.5f);
            for (int i = 0; i < explosionSize; i++)
            {
                Destroy(Instantiate(detonationPrefab, Utils.RandomNearPosition(collision.transform, ramdomExplosion, 0f, ramdomExplosion).position, Quaternion.identity), explosionLife);
            }
        }
    }
}
