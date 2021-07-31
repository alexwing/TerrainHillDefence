using UnityEngine;

public class TargetTerrain : MonoBehaviour
{

   // public TerrainData tData;
    public GameObject currentDetonatorTerrain;
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

    //public float shootDistanceToDestroy = 500.0f;
    //  [Tooltip("Ramdom explosion particles system")]
    //  public Texture2D brush;
    //  [SerializeField] private Texture2D[] Listbrush;



    [Tooltip("Up true Down False")]
    public bool UpOrDown;

    public AnimationCurve analogIntensityCurve;


    [Header("Sound Destrucion")]
    public AudioClip clip;
    [Tooltip("Width of terrain destruction")]
    [Range(0, 1500)]
    public int DistanceSoundLimit = 500;
    void Start()
    {


    }
    /*
    void textureFix()
    {
        Color32[] pixelBlock = null;
        try
        {
            pixelBlock = brush.GetPixels32();
        }
        catch (UnityException _e)
        {
            brush.filterMode = FilterMode.Point;
            RenderTexture rt = RenderTexture.GetTemporary(brush.width, brush.height);
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            Graphics.Blit(brush, rt);
            Texture2D img2 = new Texture2D(brush.width, brush.height);
            img2.ReadPixels(new Rect(0, 0, brush.width, brush.height), 0, 0);
            img2.Apply();
            RenderTexture.active = null;
            brush = img2;
            pixelBlock = brush.GetPixels32();
        }
    }
    */
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.name == "kamehameha")
        {



            modifyTerrain(collision.gameObject, UpOrDown,10,10);
            detonationTerrain(collision.gameObject,10);
            Destroy(collision.gameObject);
        }

    }



    public void modifyTerrain(GameObject collision, bool type, float destructionSize, float destructionVelocity)
    {


        float normalizedValue = Mathf.InverseLerp(0, 100, destructionSize);
        //  int brushSize = (int) Mathf.Lerp(0, Listbrush.Length-1, normalizedValue);
        int brushSize = (int)Mathf.Lerp(0, terrainDestructWidth, normalizedValue);

        // brush = Listbrush[brushSize];
        //textureFix();

        Terrain terr = GetComponent<Terrain>();
        // get the normalized position of this game object relative to the terrain
        Vector3 tempCoord = (collision.transform.position - terr.gameObject.transform.position);
        Debug.Log("collision position: "+ collision.transform.position.x + " " + collision.transform.position.y + " " + collision.transform.position.z);
        
        Vector3 coord;

        int hmWidth = terr.terrainData.heightmapResolution;
        int hmHeight = terr.terrainData.heightmapResolution;



        //int hmWidth = Mathf.RoundToInt(collision.GetComponent<KameHameHa>().Size)*10; 
        //    int hmHeight = Mathf.RoundToInt(collision.GetComponent<KameHameHa>().Size)*10;
        //float ExplosionVelocity = collision.GetComponent<KameHameHa>().Velocity;

       // float ExplosionVelocity = Mathf.Lerp(0.1f, terrainDestructHeight, destructionVelocity);
       float ExplosionVelocity = destructionVelocity;


        Debug.Log($"ExplosionVelocity {ExplosionVelocity}");

        coord.x = tempCoord.x / terr.terrainData.size.x;
        coord.y = tempCoord.y / terr.terrainData.size.y;
        coord.z = tempCoord.z / terr.terrainData.size.z;

        //int size = brush.width;
        int size = brushSize;
        int offset = Mathf.RoundToInt(size / 2);

        int x = (int)(coord.x * hmWidth) - offset;
        int y = (int)(coord.z * hmHeight) - offset;

        // int x = (int)(coord.x * hmWidth) + offsetx;
        //   int y = (int)(coord.z * hmHeight) + offsety;


        float[,] areaT;
        try
        {
            areaT = terr.terrainData.GetHeights(x, y, size, size);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    float texPixel = getBeizer(i, j, size);
                    if (type)
                    {
                        areaT[i, j] += texPixel / 100 * ExplosionVelocity;
                    }
                    else
                    {
                        areaT[i, j] -= texPixel / 100 * ExplosionVelocity;
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
    float getBeizer(int i, int j, int size)
    {
        //Create hole from beizer curve and matriz radio
        float radio = size * 0.5f;
        float radioDistance = Vector2.Distance(new Vector2(i, j), new Vector2(radio, radio));
        float normalizedRadio = Mathf.InverseLerp(0, radio, radioDistance);
        float beizerRadio = analogIntensityCurve.Evaluate(Mathf.Lerp(0, 1f, normalizedRadio));
        return beizerRadio;

    }

    public void detonationTerrain(GameObject collision, float destructionSize)
    {

        Destroy(Instantiate(currentDetonatorTerrain, collision.transform.position, Quaternion.identity), explosionLife);




        float normalizedValue = Mathf.InverseLerp(0, 100, destructionSize);
        float explosionSize = Mathf.Lerp(0, terrainDestructWidth, normalizedValue)  ;


        GameObject _currentEffect = Instantiate(collision.gameObject, collision.transform.position, Quaternion.identity);


        for (int i = 0; i < _currentEffect.transform.childCount; i++) {
            _currentEffect.transform.GetChild(i).transform.localScale = new Vector3(explosionSize, explosionSize, explosionSize) *0.1f;
        }

        Destroy(_currentEffect, 0.5f);
        //float ExplosionVelocity = collision.GetComponent<KameHameHa>().Velocity;
        // Debug.Log("Size" + collision.GetComponent<KameHameHa>().Size + " -- " + explosionSize + " -- " + ExplosionVelocity);

        for (int i = 0; i < explosionSize; i++)
        {
            Destroy(Instantiate(currentDetonatorTerrain, Utils.RandomNearPosition(collision.transform, ramdomExplosion, 0f, ramdomExplosion).position, Quaternion.identity), explosionLife);
        }


        //Utils.PlaySound(clip, collision.transform, Camera.main.transform, DistanceSoundLimit);

    }




}

