using UnityEngine;
using System.Collections.Generic;
using Combat;

namespace MirrorRPG.Combat
{
    /// <summary>
    /// ScriptableObject defining a combat effect (damage, heal, buff application)
    /// Can be used by projectiles, skills, items, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCombatEffect", menuName = "MirrorRPG/Combat/Combat Effect")]
    public class CombatEffect : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Effect name for identification")]
        public string effectName;

        [TextArea(2, 3)]
        [Tooltip("Description of the effect")]
        public string description;

        [Header("Targeting")]
        [Tooltip("Who can be affected by this effect")]
        public TargetAffinity targetAffinity = TargetAffinity.Enemy;

        [Header("Effect Type")]
        [Tooltip("What this effect does")]
        public CombatEffectType effectType = CombatEffectType.Damage;

        [Header("Damage / Heal")]
        [Tooltip("Base value (damage or heal amount)")]
        public float baseValue = 10f;

        [Tooltip("Value multiplier (applied to base value)")]
        public float valueMultiplier = 1f;

        [Tooltip("Damage type flags (for damage effects)")]
        public DamageType damageType = DamageType.None;

        [Tooltip("Can this effect critically hit?")]
        public bool canCritical = true;

        [Header("Buff/Debuff Application")]
        [Tooltip("Buffs/Debuffs to apply on hit")]
        public List<BuffApplication> buffApplications = new List<BuffApplication>();

        [Header("VFX/SFX")]
        [Tooltip("VFX to spawn on hit")]
        public GameObject hitVFX;

        [Tooltip("How long the hit VFX lasts")]
        public float hitVFXDuration = 2f;

        [Tooltip("Sound to play on hit")]
        public AudioClip hitSound;

        /// <summary>
        /// Calculate final value (damage or heal)
        /// </summary>
        public float GetFinalValue(float additionalMultiplier = 1f)
        {
            return baseValue * valueMultiplier * additionalMultiplier;
        }

        /// <summary>
        /// Check if target affinity matches
        /// </summary>
        /// <param name="isSameTeam">True if target is on the same team as the source</param>
        public bool CanAffect(bool isSameTeam, bool isSelf)
        {
            switch (targetAffinity)
            {
                case TargetAffinity.Enemy:
                    return !isSameTeam && !isSelf;
                case TargetAffinity.Ally:
                    return isSameTeam || isSelf;
                case TargetAffinity.Self:
                    return isSelf;
                case TargetAffinity.All:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Get buffs that should be applied (rolls for chance)
        /// </summary>
        public List<BuffApplication> GetApplicableBuffs()
        {
            var result = new List<BuffApplication>();
            foreach (var app in buffApplications)
            {
                if (app.ShouldApply())
                {
                    result.Add(app);
                }
            }
            return result;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(effectName))
            {
                effectName = name;
            }
            if (baseValue < 0) baseValue = 0;
            if (valueMultiplier < 0) valueMultiplier = 0;
        }
    }
}
