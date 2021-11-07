public static class SceneConfig
{
    public static bool Debug = false;

    public readonly static int FindSizeMap = 180;
    public readonly static float MapRefreshRate = 25f;
    public static class FLAG
    {
        public static int Lives = 20;
        public static int DestrucionTerrainSize = 20;
        public readonly static float DetonationSize = 50f;

    }
    public static class TOWER
    {
        public static float RotationSpeed = 25f;
        public readonly static float FindEnemyRange = 15000;
        public readonly static int ShootMaxDistance = 400;
        public readonly static float TowerFrameRate = 25f;
        public readonly static int Lives = 5;
        public readonly static float shootCarence = 0.75f;
        public readonly static float shootSpeed = 100f;
        public readonly static float shootTargetHeight = 1.75f;
        public readonly static float RotationAngleMinToShoot = 120f;
        public readonly static float DestrucionTerrainSize = 15f;
        public readonly static float DetonationSize = 6f;

    }
    public static class SOLDIER
    {
        public readonly static float AttackRange = 150f;
        public readonly static float FindEnemyRange = 5000;
        public readonly static float AttackRamdomRange = 30f;
        public readonly static float SoldierWalkAnimationVelocity = 0.5f;
        public readonly static float SoldierVelocity = 3f;
        public readonly static float SoldierFrameRate = 25f;
        public readonly static float SoldierFindFrameRate = 1f;
        public readonly static int ShootMaxDistance = 300;
        public readonly static float ShootMaxDistanceCheckFrameRate = 0.5f;
        public readonly static int Lives = 5;
        public readonly static float shootCarence = 1.75f;
        public readonly static float shootSpeed = 100f;
        public readonly static float shootTargetHeight = 1.75f;

    }

    public static class TERRAIN
    {
        public static int detonationBulletSize = 5;
        public static int explosionLife = 10;
        public static int explosionBulletLife = 10;
        public static float ramdomExplosion = 1.0f;
    }

}
