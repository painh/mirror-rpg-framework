using UnityEngine;
using System;

namespace MirrorRPG.Skill
{
    /// <summary>
    /// Individual hit timing data within a skill.
    /// Supports multi-hit implementation by configuring multiple timings per skill.
    /// </summary>
    [Serializable]
    public class SkillHitTiming
    {
        [Header("Timing")]
        [Tooltip("Hitbox activation start time (relative to animation start)")]
        public float startTime = 0.2f;

        [Tooltip("Hitbox deactivation time")]
        public float endTime = 0.4f;

        [Header("Damage")]
        [Tooltip("Damage multiplier for this hit (base damage * multiplier)")]
        [Range(0.1f, 3f)]
        public float damageMultiplier = 1f;

        [Header("Hitboxes")]
        [Tooltip("Hitbox names to activate (empty = activate all hitboxes)")]
        public string[] hitboxNames;

        /// <summary>
        /// Check if a specific hitbox should be activated during this timing
        /// </summary>
        public bool ShouldActivateHitbox(string hitboxName)
        {
            // Empty list means activate all hitboxes
            if (hitboxNames == null || hitboxNames.Length == 0)
                return true;

            foreach (var name in hitboxNames)
            {
                if (string.Equals(name, hitboxName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if this timing is currently active
        /// </summary>
        public bool IsActive(float currentTime)
        {
            return currentTime >= startTime && currentTime < endTime;
        }

        /// <summary>
        /// Check if this timing just started
        /// </summary>
        public bool JustStarted(float previousTime, float currentTime)
        {
            return previousTime < startTime && currentTime >= startTime;
        }

        /// <summary>
        /// Check if this timing just ended
        /// </summary>
        public bool JustEnded(float previousTime, float currentTime)
        {
            return previousTime < endTime && currentTime >= endTime;
        }
    }
}
