using System;

namespace MirrorRPG.Loot
{
    /// <summary>
    /// Serializable entry for loot tables
    /// </summary>
    [Serializable]
    public class LootEntry
    {
        /// <summary>
        /// Item ID (not direct reference for serialization)
        /// </summary>
        public string itemId;

        /// <summary>
        /// Drop chance (0-1)
        /// </summary>
        public float dropChance = 0.5f;

        /// <summary>
        /// Minimum quantity when dropped
        /// </summary>
        public int minQuantity = 1;

        /// <summary>
        /// Maximum quantity when dropped
        /// </summary>
        public int maxQuantity = 1;

        /// <summary>
        /// Weight for weighted random selection
        /// </summary>
        public float weight = 1f;

        public LootEntry() { }

        public LootEntry(string itemId, float dropChance = 0.5f, int minQuantity = 1, int maxQuantity = 1)
        {
            this.itemId = itemId;
            this.dropChance = dropChance;
            this.minQuantity = minQuantity;
            this.maxQuantity = maxQuantity;
            this.weight = 1f;
        }
    }
}
