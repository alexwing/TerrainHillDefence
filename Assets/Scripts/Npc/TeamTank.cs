using UnityEngine;

namespace HillDefence
{
    public class TeamTank : TeamTower
    {
        private bool isWalking = false;

        new public void Init()
        {
            if (towerMaterial != null)
                Utils.ChangeColor(towerMaterial, HillDefenceCreator.teams[npcInfo.teamNumber].teamColor);
            
            InvokeRepeating("UpdateTank", Random.Range(0, 1f / SceneConfig.SOLDIER.SoldierFrameRate), 1f / SceneConfig.SOLDIER.SoldierFrameRate);

            if (healthBarPrefab == null)
            {
                healthBarPrefab = Resources.Load<GameObject>("healthLayout");
            }
            if (healthBarPrefab != null)
            {
                _healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * 8.5f, Quaternion.identity);
                HealthLayout hl = _healthBarInstance.GetComponentInChildren<HealthLayout>();
                if (hl != null) hl.SetUp(npcInfo, transform, SceneConfig.TOWER.Lives * 2, 8.5f);
            }
        }

        new void OnTriggerEnter(Collider collision)
        {
            if (!collision.gameObject) return;
            
            if (collision.gameObject.tag == "bullet" && "bullet_" + npcInfo.teamNumber != collision.gameObject.name)
            {
                TargetTerrain.instance.DetonationBullet(collision.gameObject);
                if (npcInfo.shootCount >= SceneConfig.TOWER.Lives * 2)
                {
                    deathNPCTank(collision.gameObject);
                }
                else
                {
                    Bullet findAttackingME = collision.gameObject.GetComponent<Bullet>();
                    enemyNpc = findAttackingME!=null && findAttackingME.npcInfo.teamNumber != npcInfo.teamNumber && !npcInfo.isDead ? findAttackingME.npcInfo : enemyNpc;
                }
                Destroy(collision.gameObject);
                npcInfo.shootCount++;
            }
        }

        public void deathNPCTank(GameObject collision)
        {
            npcInfo.isDead = true;
            HillDefenceCreator.teams[npcInfo.teamNumber].towers.Remove(this);
            HillDefenceCreator.Npcs.Remove(this);
            
            if (_healthBarInstance != null) Destroy(_healthBarInstance);
            
            Destroy(gameObject);
            TargetTerrain.instance.ModifyTerrain(collision, SceneConfig.TOWER.DestrucionTerrainSize, SceneConfig.TOWER.DestrucionTerrainSize, false);
            TargetTerrain.instance.DetonationTerrain(collision, SceneConfig.TOWER.DetonationSize);
            CancelInvoke("UpdateTank");
            CancelInvoke("findEnemyTank");                
        }

        new public void findEnemy()
        {
            if (enemyNpc != null)
            {
                if (enemyNpc.isDead || enemyNpc.npcObject == null)
                    enemyNpc = null;
                else
                {
                    float sqrDist = (enemyNpc.npcObject.transform.position - transform.position).sqrMagnitude;
                    if (sqrDist > SceneConfig.TOWER.FindEnemyRange * SceneConfig.TOWER.FindEnemyRange)
                        enemyNpc = null; 
                }
            }

            if (enemyNpc == null)
            {
                GameNpc found = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.soldier);
                if (found == null)
                    found = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.tower);
                
                // Attack other tanks too
                if (found == null)
                    found = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.tank);

                enemyNpc = found;
            }
        }

        private void UpdateTank()
        {
            if (HillDefenceCreator.teams[npcInfo.teamNumber].teamFlag.npcInfo.isDead)
            {
                deathNPCTank(this.gameObject);
                return;
            }

            shootTime += Time.deltaTime;
            
            if (Terrain.activeTerrain != null)
            {
                float y = Terrain.activeTerrain.SampleHeight(transform.position);
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
            }

            if (enemyNpc != null)
            {
                if (enemyNpc.isDead || enemyNpc.npcObject == null)
                {
                    enemyNpc = null;
                    return;
                }

                float distance = Vector3.Distance(enemyNpc.npcObject.transform.position, transform.position);

                if (distance > SceneConfig.SOLDIER.AttackRange)
                {
                    Vector3 desiredDir = (enemyNpc.npcObject.transform.position - transform.position).normalized;
                    Vector3 targetPos = transform.position + desiredDir * distance;
                    transform.position = Vector3.Lerp(
                        transform.position,
                        targetPos,
                        Time.deltaTime * (SceneConfig.SOLDIER.SoldierVelocity * 0.5f) * (1f / SceneConfig.SOLDIER.SoldierFrameRate));
                    
                    isWalking = true;
                    
                    Vector3 lookDir = desiredDir;
                    lookDir.y = 0;
                    if (lookDir.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 2f);
                    }
                }
                else
                {
                    isWalking = false;
                }

                if (tower != null)
                {
                    Quaternion rotation = Quaternion.Lerp(tower.transform.rotation, Quaternion.LookRotation(enemyNpc.npcObject.transform.position - transform.position), Time.deltaTime * SceneConfig.TOWER.RotationSpeed);
                    tower.transform.rotation = new Quaternion(rotation.x, rotation.y, tower.transform.rotation.z, tower.transform.rotation.w);
                    
                    if (distance <= SceneConfig.TOWER.FindEnemyRange)
                    {
                        if (Vector3.Angle(enemyNpc.npcObject.transform.position - transform.position, tower.transform.forward) < SceneConfig.TOWER.RotationAngleMinToShoot)
                        {
                            Shoot(SceneConfig.TOWER.shootCarence, SceneConfig.TOWER.shootSpeed, SceneConfig.TOWER.ShootMaxDistance, SceneConfig.TOWER.shootTargetHeight);                      
                        }      
                    }
                }
            }
        }
    }
}

