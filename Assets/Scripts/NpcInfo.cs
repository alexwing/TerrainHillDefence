using UnityEngine;
using System.Collections.Generic;


namespace HillDefence
{
    public class NpcInfo : MonoBehaviour
    {
        public GameNpc npcInfo = new GameNpc();


        public GameObject npcToGameObject(GameNpc npc)
        {
            if (npc == null)
            {
                return null;
            }
            switch (npc.npcType)
            {
                case NpcType.soldier:
                    //find object in escene
                    return GameObject.Find("Soldier_" + npc.teamNumber + "_" + npc.npcNumber);

                case NpcType.flag:
                    return GameObject.Find("Flag_" + npc.teamNumber);

                case NpcType.tower:
                    return GameObject.Find("Tower_" + npc.teamNumber + "_" + npc.npcNumber);
                default:
                    return null;

            }

        }        
       
    }

}