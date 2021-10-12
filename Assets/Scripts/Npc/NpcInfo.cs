using UnityEngine;


namespace HillDefence
{
    public class NpcInfo : MonoBehaviour
    {
        public GameNpc npcInfo = new GameNpc();

        public GameNpc enemyNpc = null;

        [HideInInspector]
        public float shootTime = 0;


        public AudioClip shootClip;

        public GameObject shootInitPosition;


        public static GameObject npcToGameObject(GameNpc npc)
        {
            if (npc == null)
            {
                return null;
            }
            switch (npc.npcType)
            {
                case NpcType.soldier:
                    //find object in escene
                    return GameObject.Find("Soldier_" + npc.teamNumber + "_" + npc.npcNumber);

                case NpcType.flag:
                    return GameObject.Find("Flag_" + npc.teamNumber);

                case NpcType.tower:
                    return GameObject.Find("Tower_" + npc.teamNumber + "_" + npc.npcNumber);
                default:
                    return null;

            }

        }

        //shoot to enemy with carence 
        public void Shoot(float carence, float speed, int maxDistance, float targetHeight)
        {
            //shoot carence
            if (shootTime > carence)
            {

                if (enemyNpc != null)
                {                  
                    if (enemyNpc.isDead)
                    {
                        enemyNpc = null;
                        return;
                    }
                    shootTime = 0;
                   
                    //add shootTargetHeight to the position of the shootInitPosition
                    Vector3 shootTargetPosition = enemyNpc.npcObject.transform.position;
                    shootTargetPosition.y += targetHeight;
                    Vector3 dir = (shootTargetPosition - shootInitPosition.transform.position).normalized;
                    Vector3 shootPos = shootInitPosition.transform.position + dir;
                    GameObject shootSend = Instantiate(HillDefenceCreator.teams[npcInfo.teamNumber].bulletPrefab, shootPos, Quaternion.identity);
                    //move bullet to enemy
                    shootSend.GetComponent<Rigidbody>().velocity = dir * speed;
                    Bullet bullet = shootSend.GetComponent<Bullet>();
                    bullet.origin = shootPos;
                    bullet.npcInfo = npcInfo;
                    shootSend.name = "bullet_" + npcInfo.teamNumber;
                    shootSend.gameObject.tag = "bullet";
                    //  print("velocity" +shootSend.GetComponent<Rigidbody>().velocity);


                    Utils.PlaySound(shootClip, transform, Camera.main.transform, maxDistance);
                }
            }
        }
    }
}