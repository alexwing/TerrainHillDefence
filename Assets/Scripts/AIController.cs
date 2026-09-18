using UnityEngine;

namespace HillDefence
{
    public class AIController : MonoBehaviour
    {

        public float width;
        public float height;

        public static AIController instance;
        private int _currentTickIndex = 0;
        public int ticksPerFrame = 5; // How many NPCs process their AI per frame

        void Awake()
        {
            instance = this;
        }

        public void Init(int size)
        {
            height = width = size;
        }

        void Update()
        {
            if (HillDefenceCreator.Npcs == null || HillDefenceCreator.Npcs.Count == 0) return;

            // Process a batch of NPCs each frame to distribute CPU load
            for (int i = 0; i < ticksPerFrame; i++)
            {
                if (_currentTickIndex >= HillDefenceCreator.Npcs.Count)
                {
                    _currentTickIndex = 0; // Wrap around
                }

                NpcInfo npc = HillDefenceCreator.Npcs[_currentTickIndex];
                if (npc != null && !npc.npcInfo.isDead)
                {
                    // If it's a soldier, trigger its AI tick
                    TeamSoldier soldier = npc as TeamSoldier;
                    if (soldier != null)
                    {
                        soldier.findEnemy();
                    }
                    else
                    {
                        TeamTower tower = npc as TeamTower;
                        if (tower != null)
                        {
                            tower.findEnemy();
                        }
                    }
                }
                
                _currentTickIndex++;
            }
        }

        public GameNpc getNearNpc(Vector3 pos, int teamNumber, float findRange = -1, NpcType npcTypeToFind = NpcType.Any)
        {
            GameNpc bestTarget = null;
            float findRangeSqr = findRange < 0 ? Mathf.Infinity : findRange * findRange;
            float closestDistanceSqr = Mathf.Infinity;
            Vector2 currentPosition = new Vector2(pos.x, pos.z);
            foreach (NpcInfo potentialTarget in HillDefenceCreator.Npcs)
            {
                if (potentialTarget.npcInfo.teamNumber != teamNumber && !potentialTarget.npcInfo.isDead)
                {
                    if (potentialTarget.npcInfo.npcType == npcTypeToFind || npcTypeToFind == NpcType.Any)
                    {
                        Vector2 directionToTarget = new Vector2(potentialTarget.transform.position.x, potentialTarget.transform.position.z) - currentPosition;
                        float dSqrToTarget = directionToTarget.sqrMagnitude;
                        if (dSqrToTarget < findRangeSqr)
                        {
                            if (dSqrToTarget < closestDistanceSqr)
                            {
                                closestDistanceSqr = dSqrToTarget;
                                bestTarget = potentialTarget.npcInfo;
                            }
                        }
                    }
                }
            }
            return bestTarget;
        }
        void StateChanged(CullingGroupEvent e)
        {
            print("object " + e.index + " is " + (e.isVisible ? "visible" : "not visible"));

        }

    }
}
