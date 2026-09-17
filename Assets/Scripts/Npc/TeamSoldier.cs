using UnityEngine;

namespace HillDefence
{
    public class TeamSoldier : NpcInfo
    {
        // material to change color of the flag
        public GameObject body;
        public GameObject head;
        public GameObject arms;

        [Tooltip("Prefab of the World-Space Canvas healthbar. Assign in Inspector or loaded from Resources.")]
        public GameObject healthBarPrefab;

        private float AttackDistance = 0f;

        // [HideInInspector]
        private Animator animator;
        private bool isWalking = false;
        private string animateStatus = "";

        // Healthbar instance spawned in world
        private GameObject _healthBarInstance;

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

            // Spawn healthbar
            if (healthBarPrefab == null)
            {
                healthBarPrefab = Resources.Load<GameObject>("healthLayout");
            }
            if (healthBarPrefab != null)
            {
                _healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2.5f, Quaternion.identity);
                HealthLayout hl = _healthBarInstance.GetComponentInChildren<HealthLayout>();
                if (hl != null) hl.SetUp(this);
            }
        }

        void OnTriggerEnter(Collider collision)
        {
            if (!collision.gameObject) return;

            if (collision.gameObject.tag == "bullet" && "bullet_" + npcInfo.teamNumber != collision.gameObject.name)
            {
                TargetTerrain.instance.DetonationBullet(collision.gameObject);

                npcInfo.shootCount++;
                if (npcInfo.shootCount >= SceneConfig.SOLDIER.Lives && !npcInfo.isDead)
                {
                    deathNPC();
                }
                else
                {
                    // Attack the enemy that shot the bullet
                    Bullet findAttackingME = collision.gameObject.GetComponent<Bullet>();
                    enemyNpc = findAttackingME != null && findAttackingME.npcInfo.teamNumber != npcInfo.teamNumber && !npcInfo.isDead ? findAttackingME.npcInfo : enemyNpc;
                }

                Destroy(collision.gameObject);
            }
        }

        public void deathNPC()
        {
            npcInfo.isDead = true;
            this.name = "death_" + this.name;
            is_death();

            HillDefenceCreator.Npcs.Remove(gameObject.GetComponent<TeamSoldier>());
            HillDefenceCreator.teams[npcInfo.teamNumber].soldiers.Remove(gameObject.GetComponent<TeamSoldier>());
            animateStatus = "death";

            Destroy(this.GetComponent<BoxCollider>());
            CancelInvoke("UpdateSoldier");
            CancelInvoke("findEnemy");

            // Destroy healthbar
            if (_healthBarInstance != null)
                Destroy(_healthBarInstance);
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
            // Priority 1: own flag is under attack -> defend it
            TeamFlag myFlag = HillDefenceCreator.teams[npcInfo.teamNumber].teamFlag;
            if (myFlag != null && myFlag.isUnderAttack && myFlag.lastAttacker != null && !myFlag.lastAttacker.isDead)
            {
                float sqrDistToFlag = (transform.position - myFlag.transform.position).sqrMagnitude;
                if (sqrDistToFlag <= SceneConfig.SOLDIER.FlagDefenseRange * SceneConfig.SOLDIER.FlagDefenseRange)
                {
                    enemyNpc = myFlag.lastAttacker;
                    return;
                }
            }

            // If no enemy target, acquire one
            if (enemyNpc == null)
            {
                GameNpc findNpcEnemy = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.SOLDIER.FindEnemyRange, NpcType.soldier);
                if (findNpcEnemy != null)
                {
                    enemyNpc = findNpcEnemy;
                }
                else
                {
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
                else if (enemyNpc.npcType == NpcType.flag)
                {
                    GameNpc findNpcEnemy = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.SOLDIER.FindEnemyRange, NpcType.soldier);
                    enemyNpc = findNpcEnemy != null ? findNpcEnemy : enemyNpc;
                }
            }
        }

        private void UpdateSoldier()
        {
            // If team flag is destroyed, soldier dies
            if (HillDefenceCreator.teams[npcInfo.teamNumber].teamFlag.npcInfo.isDead && !npcInfo.isDead)
            {
                deathNPC();
                return;
            }

            Vector3 beforePosition = transform.position;
            if (animateStatus == "death") return;

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

                if (distance > SceneConfig.SOLDIER.AttackRange + AttackDistance && enemyNpc != null)
                {
                    // Base direction toward enemy
                    Vector3 desiredDir = (enemyNpc.npcObject.transform.position - transform.position).normalized;

                    // Steer around towers
                    desiredDir = ApplyTowerAvoidance(desiredDir);

                    // Move toward target distance in the avoidance direction
                    Vector3 targetPos = transform.position + desiredDir * distance;
                    transform.position = Vector3.Lerp(
                        transform.position,
                        targetPos,
                        Time.deltaTime * SceneConfig.SOLDIER.SoldierVelocity * (1f / SceneConfig.SOLDIER.SoldierFrameRate));

                    isWalking = true;
                }
                else
                {
                    is_ataka();
                }

                Vector3 lookTarget = enemyNpc.npcObject.transform.position - transform.position;
                lookTarget.y = 0;
                if (lookTarget.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(lookTarget);
                }
            }

            if (isWalking)
            {
                if (animateStatus != "walking")
                {
                    is_walking();
                }
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

        /// <summary>
        /// Adjusts movement direction to steer away from towers.
        /// </summary>
        private Vector3 ApplyTowerAvoidance(Vector3 desiredDir)
        {
            Collider[] nearby = Physics.OverlapSphere(transform.position, SceneConfig.SOLDIER.TowerAvoidanceRadius);
            Vector3 avoidance = Vector3.zero;
            int count = 0;

            foreach (Collider col in nearby)
            {
                TeamTower tower = col.GetComponent<TeamTower>();
                if (tower != null)
                {
                    Vector3 away = transform.position - col.transform.position;
                    away.y = 0; // Pure horizontal avoidance
                    float dist = away.magnitude;
                    if (dist > 0.05f)
                    {
                        avoidance += away.normalized / dist;
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                avoidance = (avoidance / count).normalized;
                desiredDir = (desiredDir + avoidance * SceneConfig.SOLDIER.TowerAvoidanceStrength).normalized;
            }

            return desiredDir;
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
