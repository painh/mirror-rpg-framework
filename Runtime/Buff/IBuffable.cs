using MirrorRPG.Stat;

namespace MirrorRPG.Buff
{
    /// <summary>
    /// Interface for entities that can receive buffs/debuffs.
    /// Simplified interface - just provides access to containers.
    /// </summary>
    public interface IBuffable
    {
        /// <summary>
        /// The stat container for applying stat modifiers
        /// </summary>
        StatContainer StatContainer { get; }

        /// <summary>
        /// The buff container managing active buffs
        /// </summary>
        BuffContainer BuffContainer { get; }

        /// <summary>
        /// Currently active status effects (combined flags)
        /// </summary>
        StatusEffect ActiveStatusEffects { get; }

        /// <summary>
        /// Check if entity has a specific status effect
        /// </summary>
        bool HasStatusEffect(StatusEffect effect);
    }
}
