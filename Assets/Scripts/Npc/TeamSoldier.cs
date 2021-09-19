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
                //print("bullet_" + team.teamNumber);
                if (npcInfo.shootCount >= SceneConfig.SOLDIER.SoldierLife && !npcInfo.isDead)
                {

                    npcInfo.isDead = true;
                    this.name = "death_" + this.name;
                    animator.SetBool("is_run", false);
                    animator.SetBool("is_ataka", false);
                    animator.SetBool("is_hi", false);
                    animator.SetBool("is_death", true);
                    //remove from HillDefenceCreator.soldiers
                    HillDefenceCreator.Npcs.Remove(gameObject.GetComponent<TeamSoldier>());
                    //remove from team.soldiers
                    HillDefenceCreator.teams[npcInfo.teamNumber].soldiers.Remove(gameObject.GetComponent<TeamSoldier>());
                    animateStatus = "death";
                    //remove soldier collider
                    Destroy(this.GetComponent<BoxCollider>());

                }
                else
                {
                    //atack to the shotting bullet enemy
                    NpcInfo npcEnemy = collision.gameObject.GetComponent<NpcInfo>();
                    if (npcEnemy != null)
                    {
                        if (npcEnemy.npcInfo.teamNumber != npcInfo.teamNumber)
                        {
                            print(this.name + " Enemy of " + npcEnemy.npcInfo.npcType + " " + npcEnemy.npcInfo.teamNumber + " " + npcEnemy.npcInfo.npcNumber);
                            enemyNpc = npcEnemy.npcInfo;
                        }
                    }
                }

                npcInfo.shootCount++;
                Destroy(collision.gameObject);

            }
        }
        public void death()
        {
            animator.speed = 0;
         }

        public void ShootEvent()
        {
             Shoot(SceneConfig.SOLDIER.shootCarence, SceneConfig.SOLDIER.shootSpeed,SceneConfig.SOLDIER.ShootMaxDistance,SceneConfig.SOLDIER.shootTargetHeight);
        }

        //find nearest enemy
        public void findEnemy()
        {
            //if not enemy
            if (enemyNpc == null)
            {
                //find nearest enemy
                GameNpc npcEnemy = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.SOLDIER.FindEnemyRange, NpcType.soldier);
                if (npcEnemy != null)
                {
                    // print(this.name + " Enemy of " +npcEnemy.npcType+ " " + npcEnemy.teamNumber + " " +npcEnemy.npcNumber + " " + npcEnemy.npcType);
                    enemyNpc = npcEnemy;
                }
                else
                {
                    //find enemy flag
                    npcEnemy = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, -1, NpcType.flag);
                    if (npcEnemy != null)
                    {
                        enemyNpc = npcEnemy;
                    }
                }

            }
            else
            {
                if (enemyNpc.npcType == NpcType.flag)
                {
                    GameNpc npcEnemy = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.SOLDIER.FindEnemyRange, NpcType.soldier);
                    if (npcEnemy != null)
                    {
                        enemyNpc = npcEnemy;
                    }
                }
                if (enemyNpc.isDead)
                {
                    enemyNpc = null;
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
            if (enemyNpc != null)
            {
                if (enemyNpc.isDead)
                {
                    enemyNpc = null;
                    return;
                }
                Vector3 myPosition = transform.position;
                float distance = Vector3.Distance(enemyNpc.npcObject.transform.position, myPosition);
                //print("distance: " + distance);
                if (distance > SceneConfig.SOLDIER.AttackRange + AttackDistance)
                {
                    //Lerp to enemy flag ajust to frame rate
                    transform.position = Vector3.Lerp(transform.position, enemyNpc.npcObject.transform.position, Time.deltaTime * SceneConfig.SOLDIER.SoldierVelocity * (1f / SceneConfig.SOLDIER.SoldierFrameRate));
                    isWalking = true;
                }
                else
                {
                    // Shoot();
                    animator.SetBool("is_run", false);
                    animator.SetBool("is_ataka", true);
                    animator.SetBool("is_hi", false);
                    animator.SetBool("is_death", false);
                    isWalking = false;
                }
                float y = Terrain.activeTerrain.SampleHeight(transform.position);
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
                transform.rotation = Quaternion.LookRotation(enemyNpc.npcObject.transform.position - transform.position);
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

