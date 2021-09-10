
using System.Collections.Generic;
using UnityEngine;

namespace HillDefence
{
    //create team class and properties
    public class Team
    {
        public int teamNumber;
        public TeamFlag teamFlag;
        public Color teamColor;
        public List<Vector3> soldiersPosition = new List<Vector3>();
        public List<TeamSoldier> soldiers = new List<TeamSoldier>();
        public List<TeamTower> towers = new List<TeamTower>();
        public int flagsWinsCount;
        public int killCount;
        public int deathCount;
        public int flagCount;
        public int teamEnemyNumber;
        public Team enemyTeam;
        public GameObject bulletPrefab;
    }

    public class GameNpc
    {
        public int npcNumber;
        public int teamNumber;
        public int teamEnemyNumber;
        public Color teamColor;
        public NpcType npcType;
        public bool isDead = false;
        public int shootCount = 0;

        public GameObject npcObject;
    }

    public enum NpcType
    {
        Any,
        flag,
        soldier,
        tower
    }
}