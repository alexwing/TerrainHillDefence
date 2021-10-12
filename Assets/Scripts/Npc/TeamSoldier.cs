using UnityEngine;


namespace HillDefence
{
    public class TeamSoldier : NpcInfo
    {
        // material to change color of the flag
        public GameObject body;
        public GameObject head;
        public GameObject arms;

        private float AttackDistance = 0f;

        // [HideInInspector]
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
            animator.Play("Idle", -1, Random.Range(0.0f, 1.0f));
            animateStatus = "idle";
            Color color = HillDefenceCreator.teams[npcInfo.teamNumber].teamColor;
            Utils.ChangeColor(body.GetComponent<Renderer>(), color);
            Utils.ChangeColor(head.GetComponent<Renderer>(), color);
            Utils.ChangeColor(arms.GetComponent<Renderer>(), color);
            InvokeRepeating("UpdateSoldier", Random.Range(0, 1f / SceneConfig.SOLDIER.SoldierFrameRate), 1f / SceneConfig.SOLDIER.SoldierFrameRate);
            InvokeRepeating("findEnemy", Random.Range(0, 1f / SceneConfig.SOLDIER.SoldierFindFrameRate), 1f / SceneConfig.SOLDIER.SoldierFindFrameRate);
        }

        void OnTriggerEnter(Collider collision)
        {
            if (!collision.gameObject)
            {
                return;
            }
            if (collision.gameObject.tag == "bullet" && "bullet_" + npcInfo.teamNumber != collision.gameObject.name)
            {
                TargetTerrain.instance.DetonationBullet(collision.gameObject);
                //print("bullet_" + team.teamNumber);
                if (npcInfo.shootCount >= SceneConfig.SOLDIER.Lives && !npcInfo.isDead)
                {
                    deathNPC();
                }
                else
                {
                    //atack to the shotting bullet enemy
                    Bullet findAttackingME = collision.gameObject.GetComponent<Bullet>();
                    enemyNpc = findAttackingME!=null && findAttackingME.npcInfo.teamNumber != npcInfo.teamNumber && !npcInfo.isDead ? findAttackingME.npcInfo : enemyNpc;
                }

                npcInfo.shootCount++;
                Destroy(collision.gameObject);

            }
        }

        public void deathNPC()
        {
            npcInfo.isDead = true;
            this.name = "death_" + this.name;
            is_death();
            //remove from HillDefenceCreator.soldiers
            HillDefenceCreator.Npcs.Remove(gameObject.GetComponent<TeamSoldier>());
            //remove from team.soldiers
            HillDefenceCreator.teams[npcInfo.teamNumber].soldiers.Remove(gameObject.GetComponent<TeamSoldier>());
            animateStatus = "death";
            //remove soldier collider
            Destroy(this.GetComponent<BoxCollider>());
            //remove InvokeRepeatings UpdateSoldier findEnemy
            CancelInvoke("UpdateSoldier");
            CancelInvoke("findEnemy");        
            
        }

        public void death()
        {
            animator.speed = 0;
        }

        public void ShootEvent()
        {
            Shoot(SceneConfig.SOLDIER.shootCarence, SceneConfig.SOLDIER.shootSpeed, SceneConfig.SOLDIER.ShootMaxDistance, SceneConfig.SOLDIER.shootTargetHeight);
        }

        public void findEnemy()
        {
            //if not enemy
            if (enemyNpc == null)
            {
                //find nearest enemy
                GameNpc findNpcEnemy = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.SOLDIER.FindEnemyRange, NpcType.soldier);
                if (findNpcEnemy != null)
                {                    
                    enemyNpc = findNpcEnemy;
                }
                else
                {
                    //find enemy flag
                    findNpcEnemy = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, -1, NpcType.flag);
                    enemyNpc = findNpcEnemy != null ? findNpcEnemy : enemyNpc;
                }

            }
            else
            {
                if (enemyNpc.isDead)
                {
                    enemyNpc = null;
                }
                else
                {
                    if (enemyNpc.npcType == NpcType.flag)
                    {
                        GameNpc findNpcEnemy = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.SOLDIER.FindEnemyRange, NpcType.soldier);
                        enemyNpc = findNpcEnemy != null ? findNpcEnemy : enemyNpc;
                    }
                }
            }
        }

        private void UpdateSoldier()
        {

            //is team flag is destroit
            if (HillDefenceCreator.teams[npcInfo.teamNumber].teamFlag.npcInfo.isDead && !npcInfo.isDead)
            {
                deathNPC();
                return;
            }
                    
            Vector3 beforePosition = transform.position;
            if (animateStatus == "death")
            {
                return;
            }
            shootTime += Time.deltaTime;
            float y = Terrain.activeTerrain.SampleHeight(transform.position);
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
            if (enemyNpc != null)
            {
                if (enemyNpc.isDead || enemyNpc.npcObject == null)
                {
                    enemyNpc = null;
                    return;
                }
                Vector3 myPosition = transform.position;
                float distance = Vector3.Distance(enemyNpc.npcObject.transform.position, myPosition);
                //print("distance: " + distance);
                if (distance > SceneConfig.SOLDIER.AttackRange + AttackDistance && enemyNpc != null)
                {
                    //Lerp to enemy flag ajust to frame rate
                    //before position;
                    transform.position = Vector3.Lerp(transform.position, enemyNpc.npcObject.transform.position, Time.deltaTime * SceneConfig.SOLDIER.SoldierVelocity * (1f / SceneConfig.SOLDIER.SoldierFrameRate));


                    isWalking = true;
                }
                else
                {
                    is_ataka();
                }
                transform.rotation = Quaternion.LookRotation(enemyNpc.npcObject.transform.position - transform.position);
            }

            if (isWalking)
            {
                if (animateStatus != "walking")
                {
                    is_walking();
                }
                //walking velocity
                 animator.speed = SceneConfig.SOLDIER.SoldierWalkAnimationVelocity + Mathf.Round((new Vector2(beforePosition.x, beforePosition.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude);                                
                
            }
            else
            {
                animator.speed = 1;
                if (animateStatus != "idle")
                {
                    is_idle();
                }
            }


        }

        private void is_death()
        {
            animator.speed = 2;
            animator.SetBool("is_run", false);
            animator.SetBool("is_ataka", false);
            animator.SetBool("is_hi", false);
            animator.SetBool("is_death", true);
        }        

        private void is_walking()
        {
            animator.SetBool("is_run", true);
            animator.SetBool("is_ataka", false);
            animator.SetBool("is_hi", false);
            animator.SetBool("is_death", false);
            animateStatus = "walking";
        }

        void is_ataka()
        {
            animator.SetBool("is_run", false);
            animator.SetBool("is_ataka", true);
            animator.SetBool("is_hi", false);
            animator.SetBool("is_death", false);
            isWalking = false;
        }
        private void is_idle()
        {
            animator.SetBool("is_run", false);
            animator.SetBool("is_ataka", false);
            animator.SetBool("is_hi", true);
            animator.SetBool("is_death", false);
            animateStatus = "idle";
        }
    }
}

