using System;
using MirrorRPG.Item;

namespace MirrorRPG.Inventory
{
    /// <summary>
    /// Represents a slot in an inventory
    /// </summary>
    [Serializable]
    public struct ItemSlot
    {
        /// <summary>
        /// Slot index in the inventory
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Item instance in this slot (null if empty)
        /// </summary>
        public ItemInstance Item { get; private set; }

        /// <summary>
        /// Is this slot empty?
        /// </summary>
        public bool IsEmpty => Item == null || Item.Quantity <= 0;

        /// <summary>
        /// Is this slot occupied?
        /// </summary>
        public bool HasItem => !IsEmpty;

        /// <summary>
        /// Quantity in this slot
        /// </summary>
        public int Quantity => Item?.Quantity ?? 0;

        /// <summary>
        /// Item ID in this slot
        /// </summary>
        public string ItemId => Item?.ItemId;

        public ItemSlot(int index, ItemInstance item = null)
        {
            Index = index;
            Item = item;
        }

        /// <summary>
        /// Set the item in this slot
        /// </summary>
        public void SetItem(ItemInstance item)
        {
            Item = item;
        }

        /// <summary>
        /// Clear this slot
        /// </summary>
        public void Clear()
        {
            Item = null;
        }

        /// <summary>
        /// Can this slot accept the given item?
        /// </summary>
        public bool CanAccept(ItemInstance item)
        {
            if (item == null) return true; // Can always clear

            if (IsEmpty) return true; // Empty slot accepts anything

            // Same item and stackable?
            if (Item.ItemId == item.ItemId && Item.CanStack)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to stack an item into this slot
        /// </summary>
        /// <param name="item">Item to stack</param>
        /// <returns>Remaining quantity that couldn't be stacked</returns>
        public int TryStack(ItemInstance item)
        {
            if (item == null) return 0;

            if (IsEmpty)
            {
                SetItem(item);
                return 0;
            }

            if (Item.ItemId != item.ItemId || !Item.CanStack)
            {
                return item.Quantity;
            }

            return Item.AddQuantity(item.Quantity);
        }
    }
}
