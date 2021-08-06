using UnityEngine;
using System.Collections.Generic;


namespace HillDefence
{
    public class TeamTower : MonoBehaviour
    {
        // material to change color of the flag
        public GameObject tower;

        public GameObject shootInitPosition;
        public SkinnedMeshRenderer towerMaterial;

        private float shootTime = 0;
        private float shootSpeed = 100f;
        [HideInInspector]
        public int shootCount = 0;

        public bool isDead = false;

        public AudioClip shootClip;
        public AudioClip deathClip;

        private float AttackDistance = 0f;
        public Team team;

        // [HideInInspector]
        public GameObject enemy = null;


        // Use this for initialization
        void Awake()
        {            
            AttackDistance = Random.Range(-SceneConfig.TOWER.AttackRamdomRange, SceneConfig.TOWER.AttackRamdomRange);
        }
        public void Init()
        {
            if (team != null)
            {
                Utils.ChangeColor(towerMaterial, team.teamColor);
                InvokeRepeating("UpdateSoldier", Random.Range(0, 1f / SceneConfig.TOWER.TowerFrameRate), 1f / SceneConfig.TOWER.TowerFrameRate);
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

                    Vector3 dir = (enemy.transform.position - shootInitPosition.transform.position).normalized;
                    Vector3 shootPos = shootInitPosition.transform.position + dir;
                    GameObject shootSend = Instantiate(team.bulletPrefab, shootPos, Quaternion.identity);
                    //move bullet to enemy
                    shootSend.GetComponent<Rigidbody>().velocity = dir * shootSpeed;
                    shootSend.GetComponent<Bullet>().origin = shootPos;
                    shootSend.GetComponent<Bullet>().teamNumber = team.teamNumber;
                    shootSend.name = "bullet" + team.teamNumber;
                    shootSend.gameObject.tag = "bullet";
                    Utils.PlaySound(shootClip, transform, Camera.main.transform, SceneConfig.TOWER.ShootMaxDistance);
                }
            }
            shootTime += Time.deltaTime;

        }


        void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.tag == "bullet" && "bullet" + team.teamNumber != collision.gameObject.name)
            {

                if (shootCount >= SceneConfig.TOWER.TowerLife)
                {

                   // team.soldiers.Remove(gameObject);

                    isDead = true;
                    //remove soldier collider
                    Destroy(this.GetComponent<BoxCollider>());
                }
                Destroy(collision.gameObject);
                shootCount++;

            }
        }
        public void death()
        {
            //remove from HillDefenceCreator.soldiers
          //  team.soldiers.Remove(gameObject);
            //remove from team.soldiers
          //  team.soldiers.Remove(gameObject);
            Destroy(gameObject);
        
        }
        public void Step()
        {
            //step
            //  print("step "+isWalking);
        }
        public void setTeam(Team currentTeam)
        {
            team = currentTeam;
            enemy = team.enemyTeam.teamFlag;
        }

        //find nearest enemy
        public void findEnemy()
        {

            if (team.enemyTeam.teamFlag == null || team.teamNumber == team.enemyTeam.teamNumber)
            {
                HillDefenceCreator.instance.UpdateEnemyTeam(team);
            }
            foreach (TeamSoldier enemyFind in HillDefenceCreator.soldiers)
            {
                if (enemyFind.team.teamNumber != team.teamNumber && !enemyFind.isDead)
                {
                    if (Vector3.Distance(transform.position, enemyFind.transform.position) < SceneConfig.TOWER.FindEnemyRange)
                    {
                        enemy = enemyFind.gameObject;
                        return;
                    }
                }
            }
            enemy = team.enemyTeam.teamFlag;

        }

        private void UpdateSoldier()
        {

            if (enemy != null)
            {
                Vector3 myPosition = transform.position;
                float distance = Vector3.Distance(enemy.transform.position, myPosition);
                //print("distance: " + distance);
                if (distance < SceneConfig.TOWER.AttackRange + AttackDistance)
                {
                    //rotation lerp only y
                    Quaternion rotation = Quaternion.Lerp(tower.transform.rotation, Quaternion.LookRotation(enemy.transform.position - myPosition), Time.deltaTime * SceneConfig.TOWER.RotationSpeed);

                    tower.transform.rotation = new Quaternion ( tower.transform.rotation.x, rotation.y,  tower.transform.rotation.z,  tower.transform.rotation.w);



               //     tower.transform.rotation = Quaternion.LookRotation(enemy.transform.position - tower.transform.position);
                    Shoot();

                }

            }

           

        }
    }
}

