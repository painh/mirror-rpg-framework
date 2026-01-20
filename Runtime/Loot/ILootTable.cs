using System.Collections.Generic;

namespace MirrorRPG.Loot
{
    /// <summary>
    /// Interface for loot tables
    /// </summary>
    public interface ILootTable
    {
        /// <summary>
        /// All possible drop entries
        /// </summary>
        IReadOnlyList<LootEntry> Entries { get; }

        /// <summary>
        /// Minimum guaranteed drops
        /// </summary>
        int GuaranteedDrops { get; }

        /// <summary>
        /// Maximum total drops
        /// </summary>
        int MaxDrops { get; }

        /// <summary>
        /// Roll the loot table and get results
        /// </summary>
        /// <returns>List of (itemId, quantity) tuples</returns>
        List<(string itemId, int quantity)> Roll();

        /// <summary>
        /// Roll with luck modifier
        /// </summary>
        /// <param name="luckMultiplier">Multiplier for drop chances (1.0 = normal)</param>
        List<(string itemId, int quantity)> Roll(float luckMultiplier);
    }
}
