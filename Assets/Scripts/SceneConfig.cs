using System.Collections.Generic;
using UnityEngine;

public static class SceneConfig
{
    public static bool Debug = false;

    public readonly static int FindSizeMap = 180;
    public readonly static float MapRefreshRate = 25f;
    public static class FLAG
    {
        public static int FlagShootsToWin = 20;

    }
    public static class TOWER
    {
        public static float RotationSpeed = 25f;
        public readonly static float FindEnemyRange = 15000;
        public readonly static int ShootMaxDistance = 400;
        public readonly static float TowerFrameRate = 25f;
        public readonly static int TowerLife = 5;
        public readonly static float shootCarence = 0.75f;
        public readonly static float shootSpeed = 100f;
        public readonly static float shootTargetHeight = 0f;
        public readonly static float RotationAngleMinToShoot = 20f;
        
        
        public readonly static float DestrucionTerrainSize = 15f;
        
    }
    public static class SOLDIER
    {
        public readonly static float AttackRange = 150f;
        public readonly static float FindEnemyRange = 5000;
        public readonly static float AttackRamdomRange = 30f;
        public readonly static float SoldierVelocity = 3f;
        public readonly static float SoldierFrameRate = 25f;
        public readonly static float SoldierFindFrameRate = 1f;
        public readonly static int ShootMaxDistance = 300;
        public readonly static float ShootMaxDistanceCheckFrameRate = 0.5f;
        public readonly static int SoldierLife = 5;
        public readonly static float shootCarence = 1.75f;
        public readonly static float shootSpeed = 100f;
        public readonly static float shootTargetHeight = 1.75f;

    }

    public static class TERRAIN
    {
        public static int detonationBulletSize = 5;
        public static int detonationFlagSize = 20;
        public static int explosionLife = 10;
        public static int explosionBulletLife = 10;
    }

}
