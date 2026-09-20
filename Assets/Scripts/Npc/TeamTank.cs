using UnityEngine;

namespace HillDefence
{
    public class TeamTank : TeamTower
    {
        private bool isWalking = false;

        public override void Init()
        {
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer mr in renderers)
            {
                if (mr.gameObject.name != "Barrel")
                {
                    Utils.ChangeColor(mr, HillDefenceCreator.teams[npcInfo.teamNumber].teamColor);
                }
            }
            
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
                npcInfo.shootCount++;
                BulletUtils.Despawn(collision.gameObject);
                if (npcInfo.shootCount >= SceneConfig.TOWER.Lives * 2)
                {
                    deathNPCTank(collision.gameObject);
                }
                else
                {
                    Bullet findAttackingME = collision.gameObject.GetComponent<Bullet>();
                    enemyNpc = findAttackingME!=null && findAttackingME.npcInfo.teamNumber != npcInfo.teamNumber && !npcInfo.isDead ? findAttackingME.npcInfo : enemyNpc;
                }
            }
        }

        public void deathNPCTank(GameObject collision)
        {
            npcInfo.isDead = true;
            HillDefenceCreator.teams[npcInfo.teamNumber].tanks.Remove(this);
            HillDefenceCreator.Npcs.Remove(this);
            
            if (_healthBarInstance != null) Destroy(_healthBarInstance);
            
            Destroy(gameObject);
            TargetTerrain.instance.ModifyTerrain(collision, SceneConfig.TOWER.DestrucionTerrainSize, SceneConfig.TOWER.DestrucionTerrainSize, false);
            TargetTerrain.instance.DetonationTerrain(collision, SceneConfig.TOWER.DetonationSize);
            CancelInvoke("UpdateTank");
            CancelInvoke("findEnemy");                
        }

        public override void findEnemy()
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
                
                if (found == null)
                    found = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.tank);
                // If no combat units found, attack enemy flag
                if (found == null)
                    found = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, -1, NpcType.flag);

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
                transform.position = new Vector3(transform.position.x, y + 0.3f, transform.position.z);
            }

            Vector3 targetPoint = transform.position;
            bool shouldMove = false;
            float moveDistance = 0f;

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
                    targetPoint = enemyNpc.npcObject.transform.position;
                    shouldMove = true;
                    moveDistance = distance;
                }
            }
            else
            {
                Vector3 basePos = HillDefenceCreator.teams[npcInfo.teamNumber].teamFlag.transform.position;
                float distToBase = Vector3.Distance(basePos, transform.position);
                if (distToBase > 25f)
                {
                    targetPoint = basePos;
                    shouldMove = true;
                    moveDistance = distToBase;
                }
            }

            if (shouldMove)
            {
                Vector3 desiredDir = (targetPoint - transform.position).normalized;
                float step = (SceneConfig.SOLDIER.SoldierVelocity * 0.5f) * (1f / SceneConfig.SOLDIER.SoldierFrameRate);
                transform.position += desiredDir * step;
                
                isWalking = true;
                
                Vector3 lookDir = desiredDir;
                lookDir.y = 0; 
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    Vector3 normal = Vector3.up;
                    if (Terrain.activeTerrain != null)
                    {
                        TerrainData td = Terrain.activeTerrain.terrainData;
                        Vector3 terrainLocalPos = transform.position - Terrain.activeTerrain.transform.position;
                        float nx = terrainLocalPos.x / td.size.x;
                        float ny = terrainLocalPos.z / td.size.z;
                        normal = td.GetInterpolatedNormal(nx, ny);
                    }
                    
                    Vector3 projectedForward = Vector3.ProjectOnPlane(lookDir, normal).normalized;
                    if (projectedForward.sqrMagnitude > 0.01f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(projectedForward, normal);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, (1f / SceneConfig.SOLDIER.SoldierFrameRate) * 5f);
                    }
                }
            }
            else
            {
                isWalking = false;
            }

            if (enemyNpc != null && tower != null)
            {
                Vector3 flatEnemyPos = enemyNpc.npcObject.transform.position;
                Vector3 turretLookDir = (enemyNpc.npcObject.transform.position - tower.transform.position).normalized;
                turretLookDir.y = 0;
                
                if (turretLookDir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(turretLookDir);
                    tower.transform.rotation = Quaternion.Slerp(tower.transform.rotation, targetRot, (1f / SceneConfig.SOLDIER.SoldierFrameRate) * SceneConfig.TOWER.RotationSpeed);
                }
                
                float distance = Vector3.Distance(enemyNpc.npcObject.transform.position, transform.position);
                if (distance <= SceneConfig.TOWER.FindEnemyRange)
                {
                    flatEnemyPos.y = tower.transform.position.y;
                    if (Vector3.Angle(flatEnemyPos - tower.transform.position, tower.transform.forward) < SceneConfig.TOWER.RotationAngleMinToShoot)
                    {
                        Shoot(SceneConfig.TOWER.shootCarence, SceneConfig.TOWER.shootSpeed, SceneConfig.TOWER.ShootMaxDistance, SceneConfig.TOWER.shootTargetHeight);                      
                    }      
                }
            }
        }
    }
}

