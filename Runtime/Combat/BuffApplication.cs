using UnityEngine;
using MirrorRPG.Buff;

namespace MirrorRPG.Combat
{
    /// <summary>
    /// Defines how a buff/debuff is applied as part of a combat effect
    /// </summary>
    [System.Serializable]
    public class BuffApplication
    {
        [Tooltip("Buff/Debuff to apply")]
        public BuffData buffData;

        [Tooltip("Chance to apply (0-1, 1 = 100%)")]
        [Range(0f, 1f)]
        public float chance = 1f;

        [Tooltip("Number of stacks to apply")]
        [Min(1)]
        public int stacks = 1;

        /// <summary>
        /// Roll for application based on chance
        /// </summary>
        public bool ShouldApply()
        {
            if (buffData == null) return false;
            if (chance >= 1f) return true;
            if (chance <= 0f) return false;
            return Random.value <= chance;
        }
    }
}
