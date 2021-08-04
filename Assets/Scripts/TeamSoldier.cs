using UnityEngine;
using System.Collections.Generic;


namespace HillDefence
{
    public class TeamSoldier : MonoBehaviour
    {
        // material to change color of the flag
        public GameObject body;
        public GameObject head;
        public GameObject arms;

        public GameObject shootInitPosition;
        private float shootCarence = 3f;
        private float shootTime = 0;
        private float shootSpeed = 100f;
        private int shootCount = 0;

        public AudioClip shootClip;
        public AudioClip deathClip;

        private float AttackDistance = 0f;
        public Team team;

        // [HideInInspector]
        public GameObject enemy = null;

        private Animator animator;
        private bool isWalking = false;
        private string animateStatus = "";

        // Use this for initialization
        void Awake()
        {
            animator = GetComponent<Animator>();
            AttackDistance = Random.Range(-SceneConfig.SOLDIER.AttackRamdomRange, SceneConfig.SOLDIER.AttackRamdomRange);
        }
        void Start()
        {
            Utils.ChangeColor(body.GetComponent<Renderer>(), team.teamColor);
            Utils.ChangeColor(head.GetComponent<Renderer>(), team.teamColor);
            Utils.ChangeColor(arms.GetComponent<Renderer>(), team.teamColor);
            InvokeRepeating("UpdateSoldier", 0, 1f / SceneConfig.SOLDIER.SoldierFrameRate);
            InvokeRepeating("findEnemy", 0, 1f / SceneConfig.SOLDIER.FindEnemyRange);
        }

        //shoot to enemy with carence 
        public void Shoot()
        {
            if (enemy != null)
            {
                //shoot carence
                if (shootTime > shootCarence)
                {
                    print("shootTime > shootCarence " + shootTime + " > " + shootCarence);
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
                    //  print("velocity" +shootSend.GetComponent<Rigidbody>().velocity);
                    animator.SetBool("is_run", false);
                    animator.SetBool("is_ataka", true);
                    animator.SetBool("is_hi", false);
                    animator.SetBool("is_death", false);
                    Utils.PlaySound(shootClip, transform, Camera.main.transform, SceneConfig.SOLDIER.ShootMaxDistance);
                }
            }
            shootTime += Time.deltaTime;

        }


        void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.tag == "bullet" && "bullet" + team.teamNumber != collision.gameObject.name)
            {

                if (shootCount >= SceneConfig.SOLDIER.SoldierLife)
                {
                    animator.SetBool("is_run", false);
                    animator.SetBool("is_ataka", false);
                    animator.SetBool("is_hi", false);
                    animator.SetBool("is_death", true);
                    team.soldiers.Remove(gameObject);
                    animator.Play("Standing_React_Death_Backward");
                    animateStatus = "death";
                    //remove soldier collider
                    Destroy(this.GetComponent<BoxCollider>());
                    Destroy(collision.gameObject);
                }
                shootCount++;

            }
        }
        public void death()
        {
            print(gameObject.name + " death");
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
            animator.Play("standing_idle_looking_ver_1", -1, Random.Range(0.0f, 1.0f));
            animateStatus = "idle";
            // enemy = team.enemyTeam.teamFlag;
        }

        //find nearest enemy
        public void findEnemy()
        {

            if (enemy == team.enemyTeam.teamFlag || enemy == null)
            {
                //find near enemy randomized from ememy team 
                System.Random rand = new System.Random();
                int n;
                int k = n = HillDefenceCreator.soldiers.Count;
                for (int i = 0; k > 0; ++i)
                {
                    int r = rand.Next(0, n - i);
                    if (r < k)
                    {
                        if (HillDefenceCreator.soldiers[r].team.teamNumber != team.teamNumber)
                        {
                            if (Vector3.Distance(transform.position, team.enemyTeam.soldiers[r].transform.position) < SceneConfig.SOLDIER.FindEnemyRange)
                            {
                                enemy = HillDefenceCreator.soldiers[r].gameObject;
                                return;
                            }
                        }
                        k--;
                    }
                }
            }
            /*
            foreach (GameObject enemyFind in team.enemyTeam.soldiers)
            {
                if (Vector3.Distance(transform.position, enemyFind.transform.position) < SceneConfig.SOLDIER.FindEnemyRange)
                {
                    enemy = enemyFind;
                    foundEnemy = true;
                    return;
                }
            }

            //find other enemy near this soldier
            foreach (Team teamFind in HillDefenceCreator.teams)
            {
                if (teamFind.teamNumber != team.teamNumber && team.enemyTeam.teamNumber != teamFind.teamNumber)
                {
                    foreach (GameObject enemyFind in teamFind.enemyTeam.soldiers)
                    {
                        if (Vector3.Distance(transform.position, enemyFind.transform.position) < SceneConfig.SOLDIER.FindEnemyRange)
                        {
                            enemy = enemyFind;
                            return;
                        }
                    }
                }

            }
        */
            //attack enemy flag

            enemy = team.enemyTeam.teamFlag;


        }
        private void UpdateSoldier()
        {
            if (animateStatus == "death")
            {
                return;
            }

            if (enemy != null)
            {
                Vector3 myPosition = transform.position;
                float distance = Vector3.Distance(enemy.transform.position, myPosition);
                //print("distance: " + distance);
                if (distance > SceneConfig.SOLDIER.AttackRange + AttackDistance)
                {
                    //Lerp to enemy flag ajust to frame rate
                    transform.position = Vector3.Lerp(transform.position, enemy.transform.position, Time.deltaTime * SceneConfig.SOLDIER.SoldierVelocity * (1f / SceneConfig.SOLDIER.SoldierFrameRate));
                    float y = Terrain.activeTerrain.SampleHeight(transform.position);
                    transform.position = new Vector3(transform.position.x, y, transform.position.z);
                    transform.rotation = Quaternion.LookRotation(enemy.transform.position - transform.position);
                    isWalking = true;
                }
                else
                {
                    Shoot();
                    isWalking = false;
                }

            }

            if (isWalking)
            {
                if (animateStatus != "walking")
                {
                    animator.SetBool("is_run", true);
                    animator.SetBool("is_ataka", false);
                    animator.SetBool("is_hi", false);
                    animator.SetBool("is_death", false);
                    animateStatus = "walking";
                }
            }
            else
            {
                if (animateStatus != "idle")
                {
                    animator.SetBool("is_run", false);
                    animator.SetBool("is_ataka", false);
                    animator.SetBool("is_hi", true);
                    animator.SetBool("is_death", false);
                    animateStatus = "idle";
                }
            }

        }
    }
}

