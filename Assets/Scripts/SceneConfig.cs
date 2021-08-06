using System.Collections.Generic;
using UnityEngine;

public static class SceneConfig
{
    public static bool Debug = false;
    public static int flagShootsToWin = 20;
    public static class TOWER
    {
        public static float RotationSpeed = 14f;
        public readonly static float AttackRange = 150f;
        public readonly static float FindEnemyRange = 300f;
        public readonly static int ShootMaxDistance = 300;
        public readonly static float TowerFrameRate = 25f;
        public readonly static float AttackRamdomRange = 30f;
        public readonly static int TowerLife = 5;
        public readonly static float shootCarence = 0.75f;
    }
    public static class SOLDIER
    {
        public readonly static float AttackRange = 150f;
        public readonly static float FindEnemyRange = 300f;
        public readonly static float AttackRamdomRange = 30f;
        public readonly static float SoldierVelocity = 3f;
        public readonly static float SoldierFrameRate = 25f;
        public readonly static float FindEnemyFrameRate = 1f;
        public readonly static int ShootMaxDistance = 300;
        public readonly static float ShootMaxDistanceCheckFrameRate = 0.5f;
        public readonly static int SoldierLife = 5;
        public readonly static float shootCarence = 0.75f;

    }

}
