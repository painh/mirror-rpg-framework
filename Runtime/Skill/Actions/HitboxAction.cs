using UnityEngine;
using Combat;
using MirrorRPG.Combat;

namespace MirrorRPG.Skill.Actions
{
    /// <summary>
    /// Action that activates/deactivates hitboxes during a time window
    /// </summary>
    [System.Serializable]
    public class HitboxAction : DurationSkillAction
    {
        [Header("Hitbox Settings")]
        [Tooltip("Names of hitboxes to activate (empty = all hitboxes)")]
        public string[] hitboxNames = new string[0];

        [Tooltip("Damage multiplier for this hit")]
        [Range(0.1f, 5f)]
        public float damageMultiplier = 1f;

        [Header("Combat Effect")]
        [Tooltip("Combat effect to apply on hit (optional, overrides skill default)")]
        public CombatEffect combatEffect;

        public override void OnStart(SkillActionContext context)
        {
            // Activate hitboxes
            // Implementation depends on the hitbox system
            // This will be handled by the skill executor
        }

        public override void OnEnd(SkillActionContext context)
        {
            // Deactivate hitboxes
        }

        public override void OnCancel(SkillActionContext context)
        {
            // Make sure hitboxes are deactivated when cancelled
            OnEnd(context);
        }

        /// <summary>
        /// Check if a specific hitbox should be activated
        /// </summary>
        public bool ShouldActivateHitbox(string hitboxName)
        {
            if (hitboxNames == null || hitboxNames.Length == 0)
                return true; // Activate all hitboxes

            foreach (var name in hitboxNames)
            {
                if (name == hitboxName)
                    return true;
            }
            return false;
        }
    }
}
