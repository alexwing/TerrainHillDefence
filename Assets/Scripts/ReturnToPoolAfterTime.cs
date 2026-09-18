using UnityEngine;

namespace HillDefence
{
    public class ReturnToPoolAfterTime : MonoBehaviour
    {
        public float lifeTime = 2f;

        void OnEnable()
        {
            Invoke("ReturnToPool", lifeTime);
        }

        void OnDisable()
        {
            CancelInvoke();
        }

        void ReturnToPool()
        {
            if (ObjectPooler.instance != null)
            {
                ObjectPooler.instance.ReturnToPool(gameObject);
            }
            else
            {
                gameObject.SetActive(false); // Fallback
            }
        }
    }
}
