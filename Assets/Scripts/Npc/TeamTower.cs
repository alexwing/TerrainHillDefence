using UnityEngine;
using System.Collections.Generic;


namespace HillDefence
{
    public class TeamTower : NpcInfo
    {
        public GameObject tower;
        public GameObject shootInitPosition;
        // material to change color of the tower gun
        public SkinnedMeshRenderer towerMaterial;
        private float shootTime = 0;
        public AudioClip shootClip;

        // [HideInInspector]
        public GameNpc enemyNpc = null;

        public void Init()
        {
            Utils.ChangeColor(towerMaterial,HillDefenceCreator.teams[npcInfo.teamNumber].teamColor);
            InvokeRepeating("UpdateTower", Random.Range(0, 1f / SceneConfig.TOWER.TowerFrameRate), 1f / SceneConfig.TOWER.TowerFrameRate);
            InvokeRepeating("findEnemy", Random.Range(0, 1f / SceneConfig.TOWER.FindEnemyRange), 1f / SceneConfig.TOWER.FindEnemyRange);

        }

        //shoot to enemy with carence 
        public void Shoot()
        {
            if (enemyNpc != null)
            {
                if (enemyNpc.isDead)
                {
                    enemyNpc = null;
                    return;
                }
                //shoot carence
                if (shootTime > SceneConfig.TOWER.shootCarence)
                {
                    // print("shootTime > shootCarence " + shootTime + " > " + shootCarence);
                    shootTime = 0;
                    //shoot
                    Vector3 shootTargetPosition = enemyNpc.npcObject.transform.position;
                    shootTargetPosition.y += SceneConfig.SOLDIER.shootTargetHeight;
                    Vector3 dir = (shootTargetPosition - shootInitPosition.transform.position).normalized;
                    Vector3 shootPos = shootInitPosition.transform.position + dir;
                    GameObject shootSend = Instantiate(HillDefenceCreator.teams[npcInfo.teamNumber].bulletPrefab, shootPos, Quaternion.identity);
                    //move bullet to enemy
                    shootSend.GetComponent<Rigidbody>().velocity = dir * SceneConfig.TOWER.shootSpeed;
                    Bullet bullet = shootSend.GetComponent<Bullet>();
                    bullet.origin = shootPos;
                    bullet.npcInfo = npcInfo;
                    shootSend.name = "bullet_" + npcInfo.teamNumber;
                    shootSend.gameObject.tag = "bullet";
                    Utils.PlaySound(shootClip, transform, Camera.main.transform, SceneConfig.TOWER.ShootMaxDistance);
                }
            }
            shootTime += Time.deltaTime;

        }

        void OnTriggerEnter(Collider collision)
        {
            if (!collision.gameObject)
            {
                return;
            }
            if (collision.gameObject.tag == "bullet" && "bullet_" + npcInfo.teamNumber != collision.gameObject.name)
            {

                if (npcInfo.shootCount >= SceneConfig.TOWER.TowerLife)
                {
                    // team.soldiers.Remove(gameObject);
                    npcInfo.isDead = true;
                    HillDefenceCreator.teams[npcInfo.teamNumber].towers.Remove(gameObject.GetComponent<TeamTower>());
                    HillDefenceCreator.Npcs.Remove(gameObject.GetComponent<TeamTower>());
                    Destroy(gameObject);
                    TargetTerrain.instance.ModifyTerrain(collision.gameObject, 15, 15, false);
                    TargetTerrain.instance.DetonationTerrain(collision.gameObject, 15);
                }
                Destroy(collision.gameObject);
                npcInfo.shootCount++;

            }
        }

        //find nearest enemy
        public void findEnemy()
        {
            if (enemyNpc == null)
            {
                enemyNpc = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.Any);
            }
        }

        private void UpdateTower()
        {
            if (enemyNpc != null)
            {
                if (enemyNpc.isDead)
                {
                    enemyNpc = null;
                    return;

                }
                //rotation lerp only y
                Quaternion rotation = Quaternion.Lerp(tower.transform.rotation, Quaternion.LookRotation(enemyNpc.npcObject.transform.position - transform.position), Time.deltaTime * SceneConfig.TOWER.RotationSpeed);
                tower.transform.rotation = new Quaternion(rotation.x, rotation.y, tower.transform.rotation.z, tower.transform.rotation.w);
                float distance = Vector3.Distance(enemyNpc.npcObject.transform.position, transform.position);
                if (distance <= SceneConfig.TOWER.FindEnemyRange)
                {
                    Shoot();
                }
            }
        }
    }
}

