using System;
using MirrorRPG.Stat;

namespace MirrorRPG.Buff
{
    /// <summary>
    /// Defines a stat modification for a buff
    /// </summary>
    [Serializable]
    public class BuffStatModifier
    {
        /// <summary>
        /// The stat ID to modify (e.g., "Attack", "Defense", "Speed")
        /// </summary>
        public string statId;

        /// <summary>
        /// Type of modification
        /// </summary>
        public ModifierType modifierType;

        /// <summary>
        /// Value of the modification (per stack)
        /// </summary>
        public float value;

        /// <summary>
        /// Create a StatModifier from this definition
        /// </summary>
        public StatModifier ToStatModifier(object source, int stacks = 1)
        {
            return new StatModifier(value * stacks, modifierType, 0, source);
        }
    }
}
