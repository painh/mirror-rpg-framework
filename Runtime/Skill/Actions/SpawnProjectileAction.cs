using UnityEngine;

namespace MirrorRPG.Skill.Actions
{
    /// <summary>
    /// Action that spawns a projectile at a specific time
    /// </summary>
    [System.Serializable]
    public class SpawnProjectileAction : SkillAction
    {
        [Header("Projectile Settings")]
        [Tooltip("Projectile prefab to spawn")]
        public GameObject projectilePrefab;

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
            if (projectilePrefab == null)
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
                SpawnSingle(context, basePosition, baseRotation);
            }
            else
            {
                SpawnMultiple(context, basePosition, baseRotation);
            }
        }

        private Vector3 GetDirection(SkillActionContext context, Transform spawnPoint)
        {
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

        private void SpawnSingle(SkillActionContext context, Vector3 position, Quaternion rotation)
        {
            var projectile = Object.Instantiate(projectilePrefab, position, rotation);
            InitializeProjectile(context, projectile);
        }

        private void SpawnMultiple(SkillActionContext context, Vector3 position, Quaternion baseRotation)
        {
            float totalSpread = spreadAngle * (count - 1);
            float startAngle = -totalSpread / 2f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (spreadAngle * i);
                Quaternion rotation = baseRotation * Quaternion.Euler(0f, angle, 0f);

                var projectile = Object.Instantiate(projectilePrefab, position, rotation);
                InitializeProjectile(context, projectile);
            }
        }

        private void InitializeProjectile(SkillActionContext context, GameObject projectile)
        {
            // Store context data for projectile to use
            // The projectile script should look for this
            context.CustomData["DamageMultiplier"] = damageMultiplier;
            context.CustomData["Owner"] = context.Owner;

            // Try to initialize via interface or specific component
            var initializable = projectile.GetComponent<IProjectileInitializable>();
            if (initializable != null)
            {
                initializable.Initialize(context.Owner, damageMultiplier);
            }
        }
    }

    /// <summary>
    /// Interface for projectiles that can be initialized by SpawnProjectileAction
    /// </summary>
    public interface IProjectileInitializable
    {
        void Initialize(GameObject owner, float damageMultiplier);
    }
}
