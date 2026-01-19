using UnityEngine;

namespace MirrorRPG.Skill.Actions
{
    /// <summary>
    /// Action that spawns a VFX prefab at a specific time
    /// </summary>
    [System.Serializable]
    public class SpawnVFXAction : SkillAction
    {
        [Header("VFX Settings")]
        [Tooltip("VFX prefab to spawn")]
        public GameObject vfxPrefab;

        [Tooltip("VFX lifetime (0 = auto-destroy disabled)")]
        public float lifetime = 2f;

        [Tooltip("VFX scale multiplier")]
        public float scale = 1f;

        [Header("Position")]
        [Tooltip("Where to spawn the VFX")]
        public VFXSpawnPosition spawnPosition = VFXSpawnPosition.SpawnPoint;

        [Tooltip("Position offset")]
        public Vector3 positionOffset = Vector3.zero;

        [Header("Parenting")]
        [Tooltip("Parent the VFX to something")]
        public bool attachToParent = false;

        [Tooltip("What to parent to (if attachToParent is true)")]
        public VFXParentType parentType = VFXParentType.Owner;

        public override void Execute(SkillActionContext context)
        {
            if (vfxPrefab == null) return;

            Vector3 position = GetSpawnPosition(context);
            Quaternion rotation = GetSpawnRotation(context);

            var vfx = Object.Instantiate(vfxPrefab, position, rotation);

            // Apply scale
            if (scale != 1f)
            {
                vfx.transform.localScale *= scale;
            }

            // Parent if needed
            if (attachToParent)
            {
                Transform parent = GetParent(context);
                if (parent != null)
                {
                    vfx.transform.SetParent(parent, true);
                }
            }

            // Auto-destroy
            if (lifetime > 0)
            {
                Object.Destroy(vfx, lifetime);
            }
        }

        private Vector3 GetSpawnPosition(SkillActionContext context)
        {
            Vector3 basePosition;

            switch (spawnPosition)
            {
                case VFXSpawnPosition.Owner:
                    basePosition = context.Owner.transform.position;
                    break;
                case VFXSpawnPosition.SpawnPoint:
                    basePosition = context.SpawnPoint != null
                        ? context.SpawnPoint.position
                        : context.Owner.transform.position;
                    break;
                case VFXSpawnPosition.Target:
                    basePosition = context.Target != null
                        ? context.Target.transform.position
                        : context.Owner.transform.position;
                    break;
                default:
                    basePosition = context.Owner.transform.position;
                    break;
            }

            // Apply offset in owner's local space
            return basePosition + context.Owner.transform.TransformDirection(positionOffset);
        }

        private Quaternion GetSpawnRotation(SkillActionContext context)
        {
            return context.Owner.transform.rotation;
        }

        private Transform GetParent(SkillActionContext context)
        {
            switch (parentType)
            {
                case VFXParentType.Owner:
                    return context.Owner.transform;
                case VFXParentType.SpawnPoint:
                    return context.SpawnPoint;
                case VFXParentType.Target:
                    return context.Target?.transform;
                default:
                    return null;
            }
        }
    }

    public enum VFXSpawnPosition
    {
        [InspectorName("시전자")]
        Owner,
        [InspectorName("스폰 포인트")]
        SpawnPoint,
        [InspectorName("타겟")]
        Target
    }

    public enum VFXParentType
    {
        [InspectorName("시전자")]
        Owner,
        [InspectorName("스폰 포인트")]
        SpawnPoint,
        [InspectorName("타겟")]
        Target
    }
}
