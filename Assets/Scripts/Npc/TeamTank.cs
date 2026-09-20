using UnityEngine;

namespace HillDefence
{
    public class TeamTank : TeamTower
    {
        private bool isWalking = false;
        private float _currentGroundY = 0f;
        private Vector3 _currentNormal = Vector3.up;
        private bool _groundInitialized = false;
        private Transform _cachedBarrel;

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
            
            CacheBarrel();

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

        private void CacheBarrel()
        {
            if (_cachedBarrel != null) return;

            if (tower != null)
            {
                _cachedBarrel = tower.transform.Find("Barrel");
            }
            if (_cachedBarrel == null && shootInitPosition != null && shootInitPosition.transform.parent != null)
            {
                if (shootInitPosition.transform.parent.name == "Barrel")
                {
                    _cachedBarrel = shootInitPosition.transform.parent;
                }
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
            
            GameObject collisionObj = collision != null ? collision : gameObject;
            Destroy(gameObject);
            TargetTerrain.instance.ModifyTerrain(collisionObj, SceneConfig.TOWER.DestrucionTerrainSize, SceneConfig.TOWER.DestrucionTerrainSize, false);
            TargetTerrain.instance.DetonationTerrain(collisionObj, SceneConfig.TOWER.DetonationSize);
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
                // Priority: tanks (biggest threat) -> soldiers -> towers -> flag
                GameNpc found = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.tank);
                if (found == null)
                    found = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.soldier);
                if (found == null)
                    found = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.tower);
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

            float dt = 1f / SceneConfig.SOLDIER.SoldierFrameRate;
            shootTime += dt;
            CacheBarrel();

            // 1. Multi-Point Continuous Track Suspension & Terrain Bridging
            // The 4X tank spans ~28m x 19m. Small craters (2-4m) are bridged across the tracks.
            if (Terrain.activeTerrain != null)
            {
                Vector3 center = transform.position;
                Vector3 fwd = transform.forward;
                Vector3 right = transform.right;

                float trackHalfLength = 13.0f;
                float trackHalfWidth = 9.0f;

                // 6 contact sample points along left and right tracks
                Vector3 pFL = center + fwd * trackHalfLength - right * trackHalfWidth;
                Vector3 pML = center - right * trackHalfWidth;
                Vector3 pRL = center - fwd * trackHalfLength - right * trackHalfWidth;

                Vector3 pFR = center + fwd * trackHalfLength + right * trackHalfWidth;
                Vector3 pMR = center + right * trackHalfWidth;
                Vector3 pRR = center - fwd * trackHalfLength + right * trackHalfWidth;

                float hFL = Terrain.activeTerrain.SampleHeight(pFL);
                float hML = Terrain.activeTerrain.SampleHeight(pML);
                float hRL = Terrain.activeTerrain.SampleHeight(pRL);

                float hFR = Terrain.activeTerrain.SampleHeight(pFR);
                float hMR = Terrain.activeTerrain.SampleHeight(pMR);
                float hRR = Terrain.activeTerrain.SampleHeight(pRR);

                // Continuous caterpillar track bridging logic:
                // If a crater or depression exists under the middle of the track,
                // the rigid track structure bridges across from front to rear.
                float lineML = (hFL + hRL) * 0.5f;
                float effectiveML = Mathf.Max(hML, lineML);
                float leftH = Mathf.Max((hFL + effectiveML + hRL) / 3f, lineML);

                float lineMR = (hFR + hRR) * 0.5f;
                float effectiveMR = Mathf.Max(hMR, lineMR);
                float rightH = Mathf.Max((hFR + effectiveMR + hRR) / 3f, lineMR);

                float targetGroundY = (leftH + rightH) * 0.5f;

                // Macro footprint orientation (evaluated across the 26m x 18m rectangle)
                Vector3 frontContact = (new Vector3(pFL.x, hFL, pFL.z) + new Vector3(pFR.x, hFR, pFR.z)) * 0.5f;
                Vector3 rearContact = (new Vector3(pRL.x, hRL, pRL.z) + new Vector3(pRR.x, hRR, pRR.z)) * 0.5f;
                Vector3 leftContact = (new Vector3(pFL.x, leftH, pFL.z) + new Vector3(pRL.x, leftH, pRL.z)) * 0.5f;
                Vector3 rightContact = (new Vector3(pFR.x, rightH, pFR.z) + new Vector3(pRR.x, rightH, pRR.z)) * 0.5f;

                Vector3 trackForward = (frontContact - rearContact).normalized;
                Vector3 trackRight = (rightContact - leftContact).normalized;
                Vector3 chassisUp = Vector3.Cross(trackForward, trackRight).normalized;

                if (chassisUp.y < 0.3f)
                {
                    chassisUp = Vector3.up;
                }

                if (!_groundInitialized)
                {
                    _currentGroundY = targetGroundY;
                    _currentNormal = chassisUp;
                    _groundInitialized = true;
                }
                else
                {
                    // Smooth suspension damping so small bumps/holes don't cause sudden jerks
                    _currentGroundY = Mathf.Lerp(_currentGroundY, targetGroundY, dt * 7f);
                    _currentNormal = Vector3.Slerp(_currentNormal, chassisUp, dt * 5f);
                }

                transform.position = new Vector3(transform.position.x, _currentGroundY + 0.3f, transform.position.z);
            }

            // 2. Navigation & Steering
            Vector3 targetPoint = transform.position;
            bool shouldMove = false;

            if (enemyNpc != null)
            {
                if (enemyNpc.isDead || enemyNpc.npcObject == null)
                {
                    enemyNpc = null;
                }
                else
                {
                    float distance = Vector3.Distance(enemyNpc.npcObject.transform.position, transform.position);
                    if (distance > SceneConfig.SOLDIER.AttackRange)
                    {
                        targetPoint = enemyNpc.npcObject.transform.position;
                        shouldMove = true;
                    }
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
                }
            }

            if (shouldMove)
            {
                Vector3 desiredDir = (targetPoint - transform.position).normalized;
                float step = (SceneConfig.SOLDIER.SoldierVelocity * 0.5f) * dt;
                transform.position += desiredDir * step;
                isWalking = true;

                Vector3 lookDir = desiredDir;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    Vector3 projectedForward = Vector3.ProjectOnPlane(lookDir, _currentNormal).normalized;
                    if (projectedForward.sqrMagnitude > 0.01f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(projectedForward, _currentNormal);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * 4f);
                    }
                }
            }
            else
            {
                isWalking = false;
                Vector3 currentForward = transform.forward;
                Vector3 projectedForward = Vector3.ProjectOnPlane(currentForward, _currentNormal).normalized;
                if (projectedForward.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(projectedForward, _currentNormal);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * 4f);
                }
            }

            // 3. Combat, Turret Azimuth, Barrel Elevation, and Strict Alignment
            if (enemyNpc != null && enemyNpc.npcObject != null && !enemyNpc.isDead)
            {
                Vector3 targetPos = enemyNpc.npcObject.transform.position + Vector3.up * SceneConfig.TOWER.shootTargetHeight;

                // Turret yaw (deck-locked so it never clips into chassis on slopes)
                if (tower != null)
                {
                    Transform turretParent = tower.transform.parent != null ? tower.transform.parent : transform;
                    Vector3 worldAimDir = (targetPos - tower.transform.position).normalized;
                    Vector3 localAimDir = turretParent.InverseTransformDirection(worldAimDir);
                    localAimDir.y = 0;

                    if (localAimDir.sqrMagnitude > 0.001f)
                    {
                        Quaternion targetLocalRot = Quaternion.LookRotation(localAimDir.normalized, Vector3.up);
                        tower.transform.localRotation = Quaternion.Slerp(
                            tower.transform.localRotation,
                            targetLocalRot,
                            dt * SceneConfig.TOWER.RotationSpeed
                        );
                    }
                }

                // Barrel pitch (elevation/depression)
                if (_cachedBarrel != null && tower != null)
                {
                    Vector3 targetInTurret = tower.transform.InverseTransformPoint(targetPos);
                    Vector3 barrelToTargetLocal = targetInTurret - _cachedBarrel.localPosition;

                    float targetPitch = 0f;
                    if (barrelToTargetLocal.sqrMagnitude > 0.01f)
                    {
                        float horizDist = Mathf.Max(barrelToTargetLocal.z, 0.1f);
                        targetPitch = -Mathf.Atan2(barrelToTargetLocal.y, horizDist) * Mathf.Rad2Deg;
                        // Clamp to realistic tank gun pitch: -15° (up) to +8° (down)
                        targetPitch = Mathf.Clamp(targetPitch, -15f, 8f);
                    }
                    Quaternion targetBarrelRot = Quaternion.Euler(targetPitch, 0f, 0f);
                    _cachedBarrel.localRotation = Quaternion.Slerp(
                        _cachedBarrel.localRotation,
                        targetBarrelRot,
                        dt * SceneConfig.TOWER.RotationSpeed
                    );
                }

                // Strict alignment check: the barrel bore axis must point directly at the target before firing
                float distance = Vector3.Distance(enemyNpc.npcObject.transform.position, transform.position);
                if (distance <= SceneConfig.TOWER.FindEnemyRange)
                {
                    Vector3 muzzlePos = shootInitPosition != null ? shootInitPosition.transform.position : (_cachedBarrel != null ? _cachedBarrel.position : tower.transform.position);
                    Vector3 barrelFwd = shootInitPosition != null ? shootInitPosition.transform.forward : (_cachedBarrel != null ? _cachedBarrel.forward : tower.transform.forward);
                    Vector3 toTarget = (targetPos - muzzlePos).normalized;

                    float aimAngle = Vector3.Angle(barrelFwd, toTarget);
                    // Only fire if the barrel is strictly aligned with target (<= 4.5 degrees)
                    if (aimAngle <= 4.5f)
                    {
                        ShootTank(SceneConfig.TOWER.shootCarence, SceneConfig.TOWER.shootSpeed, SceneConfig.TOWER.ShootMaxDistance);
                    }
                }
            }
            else
            {
                // Reset turret and barrel to neutral when no target
                if (tower != null)
                {
                    tower.transform.localRotation = Quaternion.Slerp(tower.transform.localRotation, Quaternion.identity, dt * 2f);
                }
                if (_cachedBarrel != null)
                {
                    _cachedBarrel.localRotation = Quaternion.Slerp(_cachedBarrel.localRotation, Quaternion.identity, dt * 2f);
                }
            }
        }

        private void ShootTank(float carence, float speed, int maxDistance)
        {
            if (shootTime > carence)
            {
                if (enemyNpc != null)
                {
                    if (enemyNpc.isDead || enemyNpc.npcObject == null)
                    {
                        enemyNpc = null;
                        return;
                    }
                    shootTime = 0;

                    // Bullet fires strictly along the barrel forward bore axis
                    Vector3 dir = (shootInitPosition != null ? shootInitPosition.transform.forward : (_cachedBarrel != null ? _cachedBarrel.forward : tower.transform.forward)).normalized;
                    Vector3 muzzle = shootInitPosition != null ? shootInitPosition.transform.position : transform.position + dir * 5f;
                    Vector3 shootPos = muzzle + dir * 1.5f;

                    GameObject shootSend;
                    if (ObjectPooler.instance != null)
                    {
                        shootSend = ObjectPooler.instance.SpawnFromPool(HillDefenceCreator.teams[npcInfo.teamNumber].bulletPrefab, shootPos, Quaternion.LookRotation(dir));
                    }
                    else
                    {
                        shootSend = Instantiate(HillDefenceCreator.teams[npcInfo.teamNumber].bulletPrefab, shootPos, Quaternion.LookRotation(dir));
                    }

                    Rigidbody rb = shootSend.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = dir * speed;
                    }
                    Bullet bullet = shootSend.GetComponent<Bullet>();
                    if (bullet != null)
                    {
                        bullet.origin = shootPos;
                        bullet.npcInfo = npcInfo;
                    }
                    shootSend.name = "bullet_" + npcInfo.teamNumber;
                    shootSend.gameObject.tag = "bullet";

                    if (shootClip != null)
                    {
                        Transform camT = Camera.main != null ? Camera.main.transform : transform;
                        Utils.PlaySound(shootClip, transform, camT, maxDistance);
                    }
                }
            }
        }
    }
}

