using System;

namespace MirrorRPG.Stat
{
    /// <summary>
    /// Represents a modifier applied to a stat
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        public ModifierType type;
        public float value;
        public int priority; // For sorting within same type
        public object source; // What applied this modifier (equipment, buff, etc.)

        public StatModifier(float value, ModifierType type, int priority = 0, object source = null)
        {
            this.value = value;
            this.type = type;
            this.priority = priority;
            this.source = source;
        }

        /// <summary>
        /// Create a flat modifier (+X)
        /// </summary>
        public static StatModifier Flat(float value, object source = null, int priority = 0)
        {
            return new StatModifier(value, ModifierType.Flat, priority, source);
        }

        /// <summary>
        /// Create a percentage add modifier (+X%)
        /// </summary>
        public static StatModifier PercentAdd(float percent, object source = null, int priority = 0)
        {
            return new StatModifier(percent, ModifierType.PercentAdd, priority, source);
        }

        /// <summary>
        /// Create a percentage multiply modifier (xX)
        /// </summary>
        public static StatModifier PercentMult(float multiplier, object source = null, int priority = 0)
        {
            return new StatModifier(multiplier, ModifierType.PercentMult, priority, source);
        }
    }
}
