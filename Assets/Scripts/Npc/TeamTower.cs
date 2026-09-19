using UnityEngine;


namespace HillDefence
{
    public class TeamTower : NpcInfo
    {
        public GameObject tower;
        // material to change color of the tower gun
        public SkinnedMeshRenderer towerMaterial;

        public GameObject healthBarPrefab;
        private GameObject _healthBarInstance;

        public void Init()
        {
            Utils.ChangeColor(towerMaterial, HillDefenceCreator.teams[npcInfo.teamNumber].teamColor);
            InvokeRepeating("UpdateTower", Random.Range(0, 1f / SceneConfig.TOWER.TowerFrameRate), 1f / SceneConfig.TOWER.TowerFrameRate);

            if (healthBarPrefab == null)
            {
                healthBarPrefab = Resources.Load<GameObject>("healthLayout");
            }
            if (healthBarPrefab != null)
            {
                _healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * 6.5f, Quaternion.identity);
                HealthLayout hl = _healthBarInstance.GetComponentInChildren<HealthLayout>();
                if (hl != null) hl.SetUp(npcInfo, transform, SceneConfig.TOWER.Lives);
            }
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
                if (npcInfo.shootCount >= SceneConfig.TOWER.Lives)
                {
                    deathNPC(collision.gameObject);
                }
                else
                {
                    //atack to the shotting bullet enemy
                    Bullet findAttackingME = collision.gameObject.GetComponent<Bullet>();
                    enemyNpc = findAttackingME!=null && findAttackingME.npcInfo.teamNumber != npcInfo.teamNumber && !npcInfo.isDead ? findAttackingME.npcInfo : enemyNpc;
                }
                Destroy(collision.gameObject);
                npcInfo.shootCount++;

            }
        }

        public void deathNPC(GameObject collision)
        {
            npcInfo.isDead = true;
            HillDefenceCreator.teams[npcInfo.teamNumber].towers.Remove(gameObject.GetComponent<TeamTower>());
            HillDefenceCreator.Npcs.Remove(gameObject.GetComponent<TeamTower>());
            
            if (_healthBarInstance != null) Destroy(_healthBarInstance);
            
            Destroy(gameObject);
            TargetTerrain.instance.ModifyTerrain(collision, SceneConfig.TOWER.DestrucionTerrainSize, SceneConfig.TOWER.DestrucionTerrainSize, false);
            TargetTerrain.instance.DetonationTerrain(collision, SceneConfig.TOWER.DetonationSize);
            CancelInvoke("UpdateTower");
            CancelInvoke("findEnemy");                
        }

        //find nearest enemy soldier or tower within detection range (NOT flags)
        public void findEnemy()
        {
            // If current target is dead or out of range, drop it
            if (enemyNpc != null)
            {
                if (enemyNpc.isDead || enemyNpc.npcObject == null)
                {
                    enemyNpc = null;
                }
                else
                {
                    float sqrDist = (enemyNpc.npcObject.transform.position - transform.position).sqrMagnitude;
                    if (sqrDist > SceneConfig.TOWER.FindEnemyRange * SceneConfig.TOWER.FindEnemyRange)
                    {
                        enemyNpc = null; // out of range, go standby
                    }
                }
            }

            // Search for a new target if we don't have one
            if (enemyNpc == null)
            {
                // Priority: soldiers first, then enemy towers
                GameNpc found = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.soldier);
                if (found == null)
                {
                    found = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.tower);
                }
                enemyNpc = found;
            }
        }

        private void UpdateTower()
        {
            shootTime += Time.deltaTime;
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
                
                float sqrDistance = (enemyNpc.npcObject.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance <= SceneConfig.TOWER.FindEnemyRange * SceneConfig.TOWER.FindEnemyRange)
                {
                    //check rotation is near to enemy
                    if (Vector3.Angle(enemyNpc.npcObject.transform.position - transform.position, transform.forward) < SceneConfig.TOWER.RotationAngleMinToShoot)
                    {
                        Shoot(SceneConfig.TOWER.shootCarence, SceneConfig.TOWER.shootSpeed, SceneConfig.TOWER.ShootMaxDistance, SceneConfig.TOWER.shootTargetHeight);                      
                    }      
                }

            }
            //is team flag is destroid
            if (HillDefenceCreator.teams[npcInfo.teamNumber].teamFlag.npcInfo.isDead)
            {
                deathNPC(this.gameObject);
            }
        }
    }
}

