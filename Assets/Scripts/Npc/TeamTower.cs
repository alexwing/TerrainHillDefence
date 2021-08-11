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
        public AudioClip deathClip;
        public Team team;

        // [HideInInspector]
        public GameObject enemy = null;

        public void Init()
        {
            if (team != null)
            {
                Utils.ChangeColor(towerMaterial, team.teamColor);
                InvokeRepeating("UpdateTower", Random.Range(0, 1f / SceneConfig.TOWER.TowerFrameRate), 1f / SceneConfig.TOWER.TowerFrameRate);
                InvokeRepeating("findEnemy", Random.Range(0, 1f / SceneConfig.TOWER.FindEnemyRange), 1f / SceneConfig.TOWER.FindEnemyRange);
            }

        }

        //shoot to enemy with carence 
        public void Shoot()
        {
            if (enemy != null)
            {
                //shoot carence
                if (shootTime > SceneConfig.TOWER.shootCarence)
                {
                    // print("shootTime > shootCarence " + shootTime + " > " + shootCarence);
                    shootTime = 0;
                    //shoot
                    Vector3 shootTargetPosition = enemy.transform.position;
                    shootTargetPosition.y += SceneConfig.SOLDIER.shootTargetHeight;
                    Vector3 dir = (shootTargetPosition - shootInitPosition.transform.position).normalized;
                    Vector3 shootPos = shootInitPosition.transform.position + dir;
                    GameObject shootSend = Instantiate(team.bulletPrefab, shootPos, Quaternion.identity);
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
            if (collision.gameObject.tag == "bullet" && "bullet_" + team.teamNumber != collision.gameObject.name)
            {

                if (npcInfo.shootCount >= SceneConfig.TOWER.TowerLife)
                {
                    // team.soldiers.Remove(gameObject);
                    npcInfo.isDead = true;
                    team.towers.Remove(gameObject.GetComponent<TeamTower>());
                    HillDefenceCreator.towers.Remove(gameObject.GetComponent<TeamTower>());
                    Destroy(gameObject);
                    TargetTerrain.instance.ModifyTerrain(collision.gameObject, 15, 15, false);
                    TargetTerrain.instance.DetonationTerrain(collision.gameObject, 15);
                }
                Destroy(collision.gameObject);
                npcInfo.shootCount++;

            }
        }
        public void setTeam(Team currentTeam)
        {
            team = currentTeam;
        }

        //find nearest enemy
        public void findEnemy()
        {
            if (enemy == null)
            {
                GameNpc npcEnemy = AIController.instance.getNearNpc(transform.position, team.teamNumber, SceneConfig.FindRange, NpcType.soldier);
                if (npcEnemy != null)
                {
                    // print(this.name + " Enemy of " +npcEnemy.npcType+ " " + npcEnemy.teamNumber + " " +npcEnemy.npcNumber + " " + npcEnemy.npcType);
                    enemy = npcToGameObject(npcEnemy);
                }
                else
                {
                    enemy = null;
                }
            }
        }

        private void UpdateTower()
        {
            if (enemy != null)
            {
                Vector3 myPosition = transform.position;
                float distance = Vector3.Distance(enemy.transform.position, myPosition);
                //print("distance: " + distance);
                if (distance < SceneConfig.TOWER.FindEnemyRange )
                {
                    //rotation lerp only y
                    Quaternion rotation = Quaternion.Lerp(tower.transform.rotation, Quaternion.LookRotation(enemy.transform.position - myPosition), Time.deltaTime * SceneConfig.TOWER.RotationSpeed);
                    tower.transform.rotation = new Quaternion(tower.transform.rotation.x, rotation.y, tower.transform.rotation.z, tower.transform.rotation.w);
                    Shoot();
                }
            }
        }
    }
}

