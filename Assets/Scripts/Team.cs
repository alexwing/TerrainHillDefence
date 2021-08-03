
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HillDefence
{
    //create team class and properties
    public class Team
    {
        public int teamNumber;
        public GameObject teamFlag;
        public Color teamColor;
        public List<Vector3> soldiersPosition = new List<Vector3>();
        public List<GameObject> soldiers = new List<GameObject>();
        public int flagsWinsCount;
        public int killCount;
        public int deathCount;
        public int flagCount;
        public int teamEnemyNumber;
         public Team enemyTeam;
         public GameObject bulletPrefab;


    }
}