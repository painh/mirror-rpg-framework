using System.Collections.Generic;
using Combat;
using MirrorRPG.Combat;

namespace MirrorRPG.Skill
{
    /// <summary>
    /// Interface for skill data definitions.
    /// Implement this on your skill ScriptableObjects.
    /// </summary>
    public interface ISkillData
    {
        /// <summary>
        /// Unique identifier or name of the skill
        /// </summary>
        string SkillName { get; }

        // Duration 제거됨 - 애니메이션 클립 길이를 런타임에 Animator에서 직접 가져옴

        /// <summary>
        /// Base damage value
        /// </summary>
        float BaseDamage { get; }

        /// <summary>
        /// Damage type flags
        /// </summary>
        DamageType DamageTypes { get; }

        /// <summary>
        /// Hit timing configurations for multi-hit skills (Legacy)
        /// </summary>
        IReadOnlyList<SkillHitTiming> HitTimings { get; }

        /// <summary>
        /// Skill actions (new action system)
        /// </summary>
        IReadOnlyList<SkillAction> Actions { get; }

        /// <summary>
        /// Combat effect to apply on hit (optional)
        /// </summary>
        CombatEffect CombatEffect { get; }

        /// <summary>
        /// Animation trigger name
        /// </summary>
        string AnimationTrigger { get; }

        /// <summary>
        /// Cooldown time before skill can be used again
        /// </summary>
        float Cooldown { get; }
    }
}
