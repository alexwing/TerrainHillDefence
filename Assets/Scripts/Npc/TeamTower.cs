using UnityEngine;
using System.Collections.Generic;


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

                if (npcInfo.shootCount >= SceneConfig.TOWER.Lives)
                {
                    npcInfo.isDead = true;
                    HillDefenceCreator.teams[npcInfo.teamNumber].towers.Remove(gameObject.GetComponent<TeamTower>());
                    HillDefenceCreator.Npcs.Remove(gameObject.GetComponent<TeamTower>());
                    Destroy(gameObject);
                    TargetTerrain.instance.ModifyTerrain(collision.gameObject, SceneConfig.TOWER.DestrucionTerrainSize, SceneConfig.TOWER.DestrucionTerrainSize, false);
                    TargetTerrain.instance.DetonationTerrain(collision.gameObject, SceneConfig.TOWER.DestrucionTerrainSize);
                }
                Destroy(collision.gameObject);
                npcInfo.shootCount++;

            }
        }

        //find nearest enemy
        public void findEnemy()
        {
            if (enemyNpc == null)
            {
                enemyNpc = AIController.instance.getNearNpc(transform.position, npcInfo.teamNumber, SceneConfig.TOWER.FindEnemyRange, NpcType.Any);
            }
            else
            {
                if (enemyNpc.isDead)
                {
                    enemyNpc = null;
                }
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
                float distance = Vector3.Distance(enemyNpc.npcObject.transform.position, transform.position);
                if (distance <= SceneConfig.TOWER.FindEnemyRange)
                {
                    //check rotation is near to enemy
                    if (Vector3.Angle(enemyNpc.npcObject.transform.position - transform.position, transform.forward) < SceneConfig.TOWER.RotationAngleMinToShoot)
                    {
                        Shoot(SceneConfig.TOWER.shootCarence, SceneConfig.TOWER.shootSpeed, SceneConfig.TOWER.ShootMaxDistance, SceneConfig.TOWER.shootTargetHeight);
                        Debug.Log("Tower is shoting");
                    }      
                }

            }
        }
    }
}

