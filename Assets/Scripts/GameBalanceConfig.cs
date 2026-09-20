using UnityEngine;

namespace HillDefence
{
    /// <summary>
    /// ScriptableObject that exposes game balance and simulation parameters to the Unity Inspector.
    /// Synchronizes seamlessly with the static SceneConfig class so existing code remains 100% compatible.
    /// Supports real-time tweaking during Play Mode via OnValidate.
    /// </summary>
    [CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "Terrain Hill Defence/Game Balance Config")]
    public class GameBalanceConfig : ScriptableObject
    {
        [Header("General Settings")]
        public bool debugMode = false;
        public int findSizeMap = 180;
        public float mapRefreshRate = 25f;

        [Header("--- Flag Settings ---")]
        [Range(1, 100)] public int flagLives = 20;
        [Range(5, 50)] public int flagDestructionTerrainSize = 20;
        [Range(10f, 100f)] public float flagDetonationSize = 50f;
        public int flagExplosionWaveCount = 24;
        public float flagExplosionWaveDuration = 3f;
        public float flagExplosionMaxScale = 6f;
        public float flagCameraShakeMagnitude = 0.8f;
        public float flagCameraShakeDuration = 1.2f;
        public float flagBloomPulseIntensity = 12f;
        public float flagBloomPulseDuration = 1.5f;

        [Header("--- Tower Settings ---")]
        [Range(5f, 60f)] public float towerRotationSpeed = 25f;
        [Range(50f, 500f)] public float towerFindEnemyRange = 250f;
        public int towerShootMaxDistance = 400;
        public float towerFrameRate = 25f;
        [Range(1, 50)] public int towerLives = 5;
        [Range(0.1f, 5f)] public float towerShootCooldown = 0.75f;
        [Range(20f, 300f)] public float towerShootSpeed = 100f;
        public float towerShootTargetHeight = 1.75f;
        [Range(5f, 90f)] public float towerRotationAngleMinToShoot = 30f;
        public float towerDestructionTerrainSize = 15f;
        public float towerDetonationSize = 6f;

        [Header("--- Soldier Settings ---")]
        [Range(20f, 300f)] public float soldierAttackRange = 150f;
        public float soldierFindEnemyRange = 5000f;
        public float soldierAttackRandomRange = 30f;
        public float soldierWalkAnimSpeed = 0.5f;
        [Range(1f, 50f)] public float soldierVelocity = 15f;
        public float soldierFrameRate = 25f;
        public float soldierFindFrameRate = 1f;
        public int soldierShootMaxDistance = 300;
        public float soldierShootMaxDistanceCheckRate = 0.5f;
        [Range(1, 30)] public int soldierLives = 5;
        [Range(0.2f, 5f)] public float soldierShootCooldown = 1.75f;
        [Range(20f, 300f)] public float soldierShootSpeed = 100f;
        public float soldierShootTargetHeight = 1.75f;
        public float soldierTowerAvoidanceRadius = 15f;
        public float soldierTowerAvoidanceStrength = 2.5f;
        public float soldierFlagDefenseRange = 200f;

        [Header("--- Soldier vs Tank Evasion ---")]
        [Tooltip("Distance below which infantry will step back and retreat from advancing tanks.")]
        [Range(15f, 80f)] public float soldierTankSafeDistance = 40f;
        [Tooltip("Strength of the lateral flanking arc when soldiers advance toward enemy tanks.")]
        [Range(0f, 1.5f)] public float soldierTankFlankStrength = 0.6f;
        [Tooltip("Repulsion radius around tanks to keep soldiers from being crushed.")]
        [Range(10f, 50f)] public float soldierTankProximityAvoidRadius = 25f;

        [Header("--- Terrain & Destruction ---")]
        public int detonationBulletSize = 5;
        public int explosionLife = 10;
        public int explosionBulletLife = 10;
        public float randomExplosion = 1.0f;

        /// <summary>
        /// Push all properties from this ScriptableObject into the static SceneConfig class.
        /// </summary>
        public void ApplyToSceneConfig()
        {
            SceneConfig.Debug = debugMode;
            SceneConfig.FindSizeMap = findSizeMap;
            SceneConfig.MapRefreshRate = mapRefreshRate;

            // Flag
            SceneConfig.FLAG.Lives = flagLives;
            SceneConfig.FLAG.DestrucionTerrainSize = flagDestructionTerrainSize;
            SceneConfig.FLAG.DetonationSize = flagDetonationSize;
            SceneConfig.FLAG.ExplosionWaveCount = flagExplosionWaveCount;
            SceneConfig.FLAG.ExplosionWaveDuration = flagExplosionWaveDuration;
            SceneConfig.FLAG.ExplosionMaxScale = flagExplosionMaxScale;
            SceneConfig.FLAG.CameraShakeMagnitude = flagCameraShakeMagnitude;
            SceneConfig.FLAG.CameraShakeDuration = flagCameraShakeDuration;
            SceneConfig.FLAG.BloomPulseIntensity = flagBloomPulseIntensity;
            SceneConfig.FLAG.BloomPulseDuration = flagBloomPulseDuration;

            // Tower
            SceneConfig.TOWER.RotationSpeed = towerRotationSpeed;
            SceneConfig.TOWER.FindEnemyRange = towerFindEnemyRange;
            SceneConfig.TOWER.ShootMaxDistance = towerShootMaxDistance;
            SceneConfig.TOWER.TowerFrameRate = towerFrameRate;
            SceneConfig.TOWER.Lives = towerLives;
            SceneConfig.TOWER.shootCarence = towerShootCooldown;
            SceneConfig.TOWER.shootSpeed = towerShootSpeed;
            SceneConfig.TOWER.shootTargetHeight = towerShootTargetHeight;
            SceneConfig.TOWER.RotationAngleMinToShoot = towerRotationAngleMinToShoot;
            SceneConfig.TOWER.DestrucionTerrainSize = towerDestructionTerrainSize;
            SceneConfig.TOWER.DetonationSize = towerDetonationSize;

            // Soldier
            SceneConfig.SOLDIER.AttackRange = soldierAttackRange;
            SceneConfig.SOLDIER.FindEnemyRange = soldierFindEnemyRange;
            SceneConfig.SOLDIER.AttackRamdomRange = soldierAttackRandomRange;
            SceneConfig.SOLDIER.SoldierWalkAnimationVelocity = soldierWalkAnimSpeed;
            SceneConfig.SOLDIER.SoldierVelocity = soldierVelocity;
            SceneConfig.SOLDIER.SoldierFrameRate = soldierFrameRate;
            SceneConfig.SOLDIER.SoldierFindFrameRate = soldierFindFrameRate;
            SceneConfig.SOLDIER.ShootMaxDistance = soldierShootMaxDistance;
            SceneConfig.SOLDIER.ShootMaxDistanceCheckFrameRate = soldierShootMaxDistanceCheckRate;
            SceneConfig.SOLDIER.Lives = soldierLives;
            SceneConfig.SOLDIER.shootCarence = soldierShootCooldown;
            SceneConfig.SOLDIER.shootSpeed = soldierShootSpeed;
            SceneConfig.SOLDIER.shootTargetHeight = soldierShootTargetHeight;
            SceneConfig.SOLDIER.TowerAvoidanceRadius = soldierTowerAvoidanceRadius;
            SceneConfig.SOLDIER.TowerAvoidanceStrength = soldierTowerAvoidanceStrength;
            SceneConfig.SOLDIER.FlagDefenseRange = soldierFlagDefenseRange;
            SceneConfig.SOLDIER.TankSafeDistance = soldierTankSafeDistance;
            SceneConfig.SOLDIER.TankFlankStrength = soldierTankFlankStrength;
            SceneConfig.SOLDIER.TankProximityAvoidRadius = soldierTankProximityAvoidRadius;

            // Terrain
            SceneConfig.TERRAIN.detonationBulletSize = detonationBulletSize;
            SceneConfig.TERRAIN.explosionLife = explosionLife;
            SceneConfig.TERRAIN.explosionBulletLife = explosionBulletLife;
            SceneConfig.TERRAIN.ramdomExplosion = randomExplosion;
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                ApplyToSceneConfig();
            }
        }

        private void OnValidate()
        {
            // Enable live-tweaking during Play Mode directly from the Inspector
            if (Application.isPlaying)
            {
                ApplyToSceneConfig();
            }
        }

        /// <summary>
        /// Auto-loads any GameBalanceConfig asset found in Resources/GameBalanceConfig prior to scene load.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoLoadAndApply()
        {
            GameBalanceConfig config = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
            if (config != null)
            {
                config.ApplyToSceneConfig();
                Debug.Log("[GameBalanceConfig] Successfully loaded and applied custom balance config from Resources/GameBalanceConfig.");
            }
        }
    }
}
