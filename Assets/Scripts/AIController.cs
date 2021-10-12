using UnityEngine;

namespace HillDefence
{
    public class AIController : MonoBehaviour
    {

        public float width;
        public float height;

        public static AIController instance;
        void Awake()
        {
            instance = this;
        }
        public void Init(int size)
        {
            height = width = size;
        }
        public GameNpc getNearNpc(Vector3 pos, int teamNumber, float findRange = -1, NpcType npcTypeToFind = NpcType.Any)
        {
            GameNpc bestTarget = null;
            if (findRange < 0)
            {
                findRange = Mathf.Infinity;
            }
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
                        if (dSqrToTarget < findRange)
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
