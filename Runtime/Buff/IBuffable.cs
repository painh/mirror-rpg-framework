using UnityEngine;
using MirrorRPG.Stat;

namespace MirrorRPG.Buff
{
    /// <summary>
    /// Interface for entities that can receive buffs/debuffs
    /// </summary>
    public interface IBuffable
    {
        /// <summary>
        /// The GameObject this entity is attached to
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// The stat container for applying stat modifiers
        /// </summary>
        StatContainer StatContainer { get; }

        /// <summary>
        /// The buff handler managing active buffs
        /// </summary>
        BuffHandler BuffHandler { get; }

        /// <summary>
        /// Currently active status effects (combined flags)
        /// </summary>
        StatusEffect ActiveStatusEffects { get; }

        /// <summary>
        /// Check if entity has a specific status effect
        /// </summary>
        bool HasStatusEffect(StatusEffect effect);

        /// <summary>
        /// Called when a buff is applied
        /// </summary>
        void OnBuffApplied(BuffInstance buff);

        /// <summary>
        /// Called when a buff is removed
        /// </summary>
        void OnBuffRemoved(BuffInstance buff);

        /// <summary>
        /// Called when status effects change
        /// </summary>
        void OnStatusEffectsChanged(StatusEffect oldEffects, StatusEffect newEffects);
    }
}
