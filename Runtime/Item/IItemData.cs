using System.Collections.Generic;
using MirrorRPG.Buff;

namespace MirrorRPG.Item
{
    /// <summary>
    /// Interface defining item data contract
    /// </summary>
    public interface IItemData
    {
        /// <summary>
        /// Unique identifier for this item
        /// </summary>
        string ItemId { get; }

        /// <summary>
        /// Display name shown in UI
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Item description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Category of the item
        /// </summary>
        ItemType ItemType { get; }

        /// <summary>
        /// Rarity level
        /// </summary>
        ItemRarity Rarity { get; }

        /// <summary>
        /// Equipment slot (None if not equippable)
        /// </summary>
        EquipmentSlotType EquipmentSlot { get; }

        /// <summary>
        /// Can multiple items stack in one slot?
        /// </summary>
        bool IsStackable { get; }

        /// <summary>
        /// Maximum stack size
        /// </summary>
        int MaxStackSize { get; }

        /// <summary>
        /// Stat modifiers when equipped (reuses buff system)
        /// </summary>
        IReadOnlyList<BuffStatModifier> StatModifiers { get; }

        /// <summary>
        /// Buff to apply when item is used (for consumables)
        /// </summary>
        BuffData UseEffect { get; }
    }
}
