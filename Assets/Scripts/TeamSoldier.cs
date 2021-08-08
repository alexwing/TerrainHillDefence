﻿using UnityEngine;


namespace HillDefence
{
    public class TeamSoldier : NpcInfo
    {
        // material to change color of the flag
        public GameObject body;
        public GameObject head;
        public GameObject arms;
        public GameObject shootInitPosition;
        private float shootTime = 0;

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
        public void Init()
        {
            Utils.ChangeColor(body.GetComponent<Renderer>(), team.teamColor);
            Utils.ChangeColor(head.GetComponent<Renderer>(), team.teamColor);
            Utils.ChangeColor(arms.GetComponent<Renderer>(), team.teamColor);
            InvokeRepeating("UpdateSoldier", Random.Range(0, 1f / SceneConfig.SOLDIER.SoldierFrameRate), 1f / SceneConfig.SOLDIER.SoldierFrameRate);
            InvokeRepeating("findEnemy", Random.Range(0, 1f / SceneConfig.SOLDIER.FindEnemyRange), 1f / SceneConfig.SOLDIER.FindEnemyRange);
        }

        //shoot to enemy with carence 
        public void Shoot()
        {
            if (enemy != null)
            {
                //shoot carence
                if (shootTime > SceneConfig.SOLDIER.shootCarence)
                {
                    // print("shootTime > shootCarence " + shootTime + " > " + shootCarence);
                    shootTime = 0;
                    //shoot
                    //add shootTargetHeight to the position of the shootInitPosition
                    Vector3 shootTargetPosition = enemy.transform.position;
                    shootTargetPosition.y += SceneConfig.SOLDIER.shootTargetHeight;
                    Vector3 dir = (shootTargetPosition - shootInitPosition.transform.position).normalized;
                    Vector3 shootPos = shootInitPosition.transform.position + dir;
                    GameObject shootSend = Instantiate(team.bulletPrefab, shootPos, Quaternion.identity);
                    //move bullet to enemy
                    shootSend.GetComponent<Rigidbody>().velocity = dir * SceneConfig.SOLDIER.shootSpeed;
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
        }


        void OnTriggerEnter(Collider collision)
        {
            if (!collision.gameObject)
            {
                return;
            }
            if (collision.gameObject.tag == "bullet" && "bullet" + team.teamNumber != collision.gameObject.name)
            {
                if (npcInfo.shootCount >= SceneConfig.SOLDIER.SoldierLife)
                {
                    animator.SetBool("is_run", false);
                    animator.SetBool("is_ataka", false);
                    animator.SetBool("is_hi", false);
                    animator.SetBool("is_death", true);
                    team.soldiers.Remove(gameObject.GetComponent<TeamSoldier>());
                    animator.Play("Standing_React_Death_Backward");
                    animateStatus = "death";
                    npcInfo.isDead = true;
                    //remove soldier collider
                    Destroy(this.GetComponent<BoxCollider>());
                }
                Destroy(collision.gameObject);
                npcInfo.shootCount++;

            }
        }
        public void death()
        {
            //remove from HillDefenceCreator.soldiers
            team.soldiers.Remove(gameObject.GetComponent<TeamSoldier>());
            //remove from team.soldiers
            team.soldiers.Remove(gameObject.GetComponent<TeamSoldier>());
            Destroy(gameObject);
        }
        public void Step()
        {
            //step
        }
        public void setTeam(Team currentTeam)
        {
            team = currentTeam;
            animator.Play("standing_idle_looking_ver_1", -1, Random.Range(0.0f, 1.0f));
            animateStatus = "idle";
            enemy = team.enemyTeam.teamFlag.gameObject;
        }

        //find nearest enemy
        public void findEnemy()
        {
            if (team.enemyTeam.teamFlag == null || team.teamNumber == team.enemyTeam.teamNumber)
            {
                AIController.instance.UpdateEnemyTeam(team);
            }
            //if not enemy or enemy is a enemy flag find near enemy
            if (enemy == null || enemy == team.enemyTeam.teamFlag.gameObject)
            {
                GameNpc npcEnemy = AIController.instance.getNearNpc(transform.position, team.teamNumber, SceneConfig.FindRange,  NpcType.Any);
                if (npcEnemy != null)
                {
                    // print(this.name + " Enemy of " +npcEnemy.npcType+ " " + npcEnemy.teamNumber + " " +npcEnemy.npcNumber + " " + npcEnemy.npcType);
                    enemy = npcToGameObject(npcEnemy);
                }
                else
                {
                    enemy = team.enemyTeam.teamFlag.gameObject;
                }
            }
        }
        private void UpdateSoldier()
        {
            if (animateStatus == "death")
            {
                return;
            }
            shootTime += Time.deltaTime;
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

