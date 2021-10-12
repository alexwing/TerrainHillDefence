using UnityEngine;


namespace HillDefence
{
    public class TeamTower : NpcInfo
    {
        public GameObject tower;
        // material to change color of the tower gun
        public SkinnedMeshRenderer towerMaterial;

        public void Init()
        {
            Utils.ChangeColor(towerMaterial, HillDefenceCreator.teams[npcInfo.teamNumber].teamColor);
            InvokeRepeating("UpdateTower", Random.Range(0, 1f / SceneConfig.TOWER.TowerFrameRate), 1f / SceneConfig.TOWER.TowerFrameRate);
            InvokeRepeating("findEnemy", Random.Range(0, 1f / SceneConfig.TOWER.FindEnemyRange), 1f / SceneConfig.TOWER.FindEnemyRange);
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
            Destroy(gameObject);
            TargetTerrain.instance.ModifyTerrain(collision, SceneConfig.TOWER.DestrucionTerrainSize, SceneConfig.TOWER.DestrucionTerrainSize, false);
            TargetTerrain.instance.DetonationTerrain(collision, SceneConfig.TOWER.DestrucionTerrainSize);
            CancelInvoke("UpdateTower");
            CancelInvoke("findEnemy");                
        }

        //find nearest enemy
        public void findEnemy()
        {
            enemyNpc = enemyNpc == null || enemyNpc.isDead ? AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.Any) : null;
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
                float distance = Vector3.Distance(enemyNpc.npcObject.transform.position, transform.position);
                if (distance <= SceneConfig.TOWER.FindEnemyRange)
                {
                    //check rotation is near to enemy
                    if (Vector3.Angle(enemyNpc.npcObject.transform.position - transform.position, transform.forward) < SceneConfig.TOWER.RotationAngleMinToShoot)
                    {
                        Shoot(SceneConfig.TOWER.shootCarence, SceneConfig.TOWER.shootSpeed, SceneConfig.TOWER.ShootMaxDistance, SceneConfig.TOWER.shootTargetHeight);                      
                    }      
                }

            }
            //is team flag is destroit
            if (HillDefenceCreator.teams[npcInfo.teamNumber].teamFlag.npcInfo.isDead)
            {
                deathNPC(this.gameObject);
            }
        }
    }
}

