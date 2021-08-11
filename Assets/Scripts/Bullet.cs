
using UnityEngine;
namespace HillDefence
{
    public class Bullet : MonoBehaviour
    {

        public Vector3 origin;
        public GameNpc npcInfo = new GameNpc();
        void Start()
        {
            InvokeRepeating("CheckDistance", 0, 1f / SceneConfig.SOLDIER.ShootMaxDistanceCheckFrameRate);
        }

        private void CheckDistance()
        {
            float originDistance = Vector3.Distance(origin, transform.position);
            if (originDistance > SceneConfig.SOLDIER.ShootMaxDistance)
            {
                Destroy(gameObject);
            }
        }

        void OnDisable()
        {
            CancelInvoke("CheckDistance");
        }
        private void OnDestroy()
        {
            CancelInvoke("CheckDistance");
        }
    }




}