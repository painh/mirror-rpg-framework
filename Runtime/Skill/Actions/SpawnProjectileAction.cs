using UnityEngine;

namespace MirrorRPG.Skill.Actions
{
    /// <summary>
    /// Action that spawns a projectile at a specific time
    /// Supports both direct prefab reference and Resources path loading
    /// </summary>
    [System.Serializable]
    public class SpawnProjectileAction : SkillAction
    {
        [Header("Projectile Settings")]
        [Tooltip("Projectile prefab to spawn (직접 참조)")]
        public GameObject projectilePrefab;

        [Tooltip("Resources 경로로 프리팹 로드 (예: 'Projectile')")]
        public string projectilePrefabPath = "Projectile";

        [Tooltip("ProjectileData 경로 (예: 'ProjectileData/Fireball')")]
        public string projectileDataPath;

        [Tooltip("Spawn position offset from spawn point")]
        public Vector3 spawnOffset = Vector3.zero;

        [Tooltip("Spawn rotation offset (euler angles)")]
        public Vector3 rotationOffset = Vector3.zero;

        [Header("Targeting")]
        [Tooltip("Aim at current target")]
        public bool aimAtTarget = true;

        [Tooltip("Use owner's forward direction if no target")]
        public bool useOwnerForward = true;

        [Header("Options")]
        [Tooltip("Number of projectiles to spawn")]
        [Min(1)]
        public int count = 1;

        [Tooltip("Spread angle between multiple projectiles (degrees)")]
        [Range(0f, 180f)]
        public float spreadAngle = 0f;

        [Tooltip("Damage multiplier for spawned projectile")]
        public float damageMultiplier = 1f;

        public override void Execute(SkillActionContext context)
        {
            // Load prefab
            GameObject prefab = projectilePrefab;
            if (prefab == null && !string.IsNullOrEmpty(projectilePrefabPath))
            {
                prefab = Resources.Load<GameObject>(projectilePrefabPath);
            }

            if (prefab == null)
            {
                Debug.LogWarning("[SpawnProjectileAction] Projectile prefab is null");
                return;
            }

            Transform spawnPoint = context.SpawnPoint ?? context.Owner.transform;
            Vector3 basePosition = spawnPoint.position + spawnPoint.TransformDirection(spawnOffset);

            // Determine base direction
            Vector3 direction = GetDirection(context, spawnPoint);
            Quaternion baseRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(rotationOffset);

            // Spawn projectile(s)
            if (count == 1)
            {
                SpawnSingle(context, prefab, basePosition, baseRotation, direction);
            }
            else
            {
                SpawnMultiple(context, prefab, basePosition, baseRotation, direction);
            }
        }

        private Vector3 GetDirection(SkillActionContext context, Transform spawnPoint)
        {
            // Use IAimProvider interface if available
            if (context.Owner != null)
            {
                var aimProvider = context.Owner.GetComponent<IAimProvider>();
                if (aimProvider != null)
                {
                    Vector3 aimTarget = aimProvider.GetAimTarget();
                    return (aimTarget - spawnPoint.position).normalized;
                }
            }

            // Try to aim at target
            if (aimAtTarget && context.Target != null)
            {
                Vector3 targetPos = context.Target.transform.position;
                targetPos.y = spawnPoint.position.y; // Keep on same plane
                return (targetPos - spawnPoint.position).normalized;
            }

            // Use context direction if provided
            if (context.Direction != Vector3.zero)
            {
                return context.Direction.normalized;
            }

            // Use owner's forward
            if (useOwnerForward && context.Owner != null)
            {
                return context.Owner.transform.forward;
            }

            return spawnPoint.forward;
        }

        private void SpawnSingle(SkillActionContext context, GameObject prefab, Vector3 position, Quaternion rotation, Vector3 direction)
        {
            var projectileObj = Object.Instantiate(prefab, position, rotation);
            InitializeProjectile(context, projectileObj, direction);
        }

        private void SpawnMultiple(SkillActionContext context, GameObject prefab, Vector3 position, Quaternion baseRotation, Vector3 direction)
        {
            float totalSpread = spreadAngle * (count - 1);
            float startAngle = -totalSpread / 2f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (spreadAngle * i);
                Quaternion rotation = baseRotation * Quaternion.Euler(0f, angle, 0f);
                Vector3 spreadDir = rotation * Vector3.forward;

                var projectileObj = Object.Instantiate(prefab, position, rotation);
                InitializeProjectile(context, projectileObj, spreadDir);
            }
        }

        private void InitializeProjectile(SkillActionContext context, GameObject projectileObj, Vector3 direction)
        {
            // Try IProjectileInitializable interface first
            var initializable = projectileObj.GetComponent<IProjectileInitializable>();
            if (initializable != null)
            {
                initializable.Initialize(context.Owner, direction, projectileDataPath, damageMultiplier);
                return;
            }

            // Store context data for projectile to use
            context.CustomData["DamageMultiplier"] = damageMultiplier;
            context.CustomData["Owner"] = context.Owner;
            context.CustomData["Direction"] = direction;
            context.CustomData["ProjectileDataPath"] = projectileDataPath;
        }
    }

    /// <summary>
    /// Interface for projectiles that can be initialized by SpawnProjectileAction
    /// Implement this in your project's Projectile class
    /// </summary>
    public interface IProjectileInitializable
    {
        void Initialize(GameObject owner, Vector3 direction, string dataPath, float damageMultiplier);
    }

    /// <summary>
    /// Interface for aim systems that provide targeting information
    /// Implement this in your project's AimSystem class
    /// </summary>
    public interface IAimProvider
    {
        Vector3 GetAimTarget();
    }
}
