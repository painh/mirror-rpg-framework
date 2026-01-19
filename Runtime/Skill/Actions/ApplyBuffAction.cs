using UnityEngine;
using MirrorRPG.Buff;

namespace MirrorRPG.Skill.Actions
{
    /// <summary>
    /// Action that applies a buff to the caster (self-buff)
    /// </summary>
    [System.Serializable]
    public class ApplyBuffAction : SkillAction
    {
        [Header("Buff Settings")]
        [Tooltip("Buff to apply")]
        public BuffData buffData;

        [Tooltip("Target of the buff")]
        public BuffTargetType target = BuffTargetType.Self;

        [Tooltip("Number of stacks to apply")]
        [Min(1)]
        public int stacks = 1;

        [Tooltip("Chance to apply (0-1)")]
        [Range(0f, 1f)]
        public float chance = 1f;

        public override void Execute(SkillActionContext context)
        {
            if (buffData == null) return;

            // Roll for chance
            if (chance < 1f && Random.value > chance) return;

            GameObject targetObject = GetTarget(context);
            if (targetObject == null) return;

            // Try to get IBuffable from target
            var buffable = targetObject.GetComponent<IBuffable>();
            if (buffable?.BuffContainer == null) return;

            // Apply buff(s)
            for (int i = 0; i < stacks; i++)
            {
                buffable.BuffContainer.ApplyBuff(buffData, context.Owner);
            }
        }

        private GameObject GetTarget(SkillActionContext context)
        {
            switch (target)
            {
                case BuffTargetType.Self:
                    return context.Owner;
                case BuffTargetType.Target:
                    return context.Target;
                default:
                    return context.Owner;
            }
        }
    }

    public enum BuffTargetType
    {
        [InspectorName("자신")]
        Self,
        [InspectorName("타겟")]
        Target
    }
}
