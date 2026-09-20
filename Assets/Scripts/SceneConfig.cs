public static class SceneConfig
{
    public static bool Debug = false;

    public static int FindSizeMap = 180;
    public static float MapRefreshRate = 25f;

    public static class FLAG
    {
        public static int Lives = 20;
        public static int DestrucionTerrainSize = 20;
        public static float DetonationSize = 50f;
        // Explosion FX parameters
        public static int ExplosionWaveCount = 24;       // particles in the expanding ring
        public static float ExplosionWaveDuration = 3f;  // seconds to expand
        public static float ExplosionMaxScale = 6f;      // peak scale of each fireball
        public static float CameraShakeMagnitude = 0.8f;
        public static float CameraShakeDuration = 1.2f;
        // Post FX bloom pulse
        public static float BloomPulseIntensity = 12f;
        public static float BloomPulseDuration = 1.5f;
    }

    public static class TOWER
    {
        public static float RotationSpeed = 25f;
        public static float FindEnemyRange = 250f;   // detection radius (world units)
        public static int ShootMaxDistance = 400;
        public static float TowerFrameRate = 25f;
        public static int Lives = 5;
        public static float shootCarence = 0.75f;
        public static float shootSpeed = 100f;
        public static float shootTargetHeight = 1.75f;
        public static float RotationAngleMinToShoot = 30f;
        public static float DestrucionTerrainSize = 15f;
        public static float DetonationSize = 6f;
    }

    public static class SOLDIER
    {
        public static float AttackRange = 150f;
        public static float FindEnemyRange = 5000;
        public static float AttackRamdomRange = 30f;
        public static float SoldierWalkAnimationVelocity = 0.5f;
        public static float SoldierVelocity = 15f;
        public static float SoldierFrameRate = 25f;
        public static float SoldierFindFrameRate = 1f;
        public static int ShootMaxDistance = 300;
        public static float ShootMaxDistanceCheckFrameRate = 0.5f;
        public static int Lives = 5;
        public static float shootCarence = 1.75f;
        public static float shootSpeed = 100f;
        public static float shootTargetHeight = 1.75f;
        // Avoidance of own towers
        public static float TowerAvoidanceRadius = 15f;
        public static float TowerAvoidanceStrength = 2.5f;
        // Flag defense: distance within which soldiers react to flag under attack
        public static float FlagDefenseRange = 200f;
        // Tank evasion and flanking
        public static float TankSafeDistance = 40f;
        public static float TankFlankStrength = 0.6f;
        public static float TankProximityAvoidRadius = 25f;
    }

    public static class TERRAIN
    {
        public static int detonationBulletSize = 5;
        public static int explosionLife = 10;
        public static int explosionBulletLife = 10;
        public static float ramdomExplosion = 1.0f;
    }
}
