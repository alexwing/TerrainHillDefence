using System.Collections.Generic;
using UnityEngine;

namespace HillDefence
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler instance;

        private Dictionary<string, Queue<GameObject>> poolDictionary;

        void Awake()
        {
            if (instance == null || instance != this)
            {
                instance = this;
            }
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
        }

        public GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            string poolKey = prefab.name;

            if (!poolDictionary.ContainsKey(poolKey))
            {
                poolDictionary.Add(poolKey, new Queue<GameObject>());
            }

            GameObject objectToSpawn = null;
            
            // Find an inactive object in the pool
            while (poolDictionary[poolKey].Count > 0)
            {
                GameObject obj = poolDictionary[poolKey].Dequeue();
                if (obj != null && !obj.activeInHierarchy)
                {
                    objectToSpawn = obj;
                    break;
                }
            }

            if (objectToSpawn == null)
            {
                objectToSpawn = Instantiate(prefab);
                objectToSpawn.name = prefab.name; // Keep name clean for future pooling
            }

            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);

            return objectToSpawn;
        }

        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            
            string poolKey = obj.name;
            if (!poolDictionary.ContainsKey(poolKey))
            {
                poolDictionary.Add(poolKey, new Queue<GameObject>());
            }
            
            poolDictionary[poolKey].Enqueue(obj);
        }
    }
}
