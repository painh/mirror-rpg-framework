using System.Collections.Generic;
using Combat;

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

        /// <summary>
        /// Total duration of the skill in seconds
        /// </summary>
        float Duration { get; }

        /// <summary>
        /// Base damage value
        /// </summary>
        float BaseDamage { get; }

        /// <summary>
        /// Damage type flags
        /// </summary>
        DamageType DamageTypes { get; }

        /// <summary>
        /// Hit timing configurations for multi-hit skills
        /// </summary>
        IReadOnlyList<SkillHitTiming> HitTimings { get; }

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
