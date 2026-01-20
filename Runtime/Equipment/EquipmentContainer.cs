using System;
using System.Collections.Generic;
using MirrorRPG.Item;
using MirrorRPG.Stat;

namespace MirrorRPG.Equipment
{
    /// <summary>
    /// Container that manages equipped items
    /// </summary>
    public class EquipmentContainer
    {
        private readonly Dictionary<EquipmentSlotType, ItemInstance> equippedItems = new Dictionary<EquipmentSlotType, ItemInstance>();
        private readonly StatContainer statContainer;

        /// <summary>
        /// All equipped items
        /// </summary>
        public IReadOnlyDictionary<EquipmentSlotType, ItemInstance> EquippedItems => equippedItems;

        #region Events

        /// <summary>
        /// Fired when equipment changes in any slot
        /// </summary>
        public event Action<EquipmentSlotType, ItemInstance, ItemInstance> OnEquipmentChanged;

        /// <summary>
        /// Fired when an item is equipped
        /// </summary>
        public event Action<EquipmentSlotType, ItemInstance> OnItemEquipped;

        /// <summary>
        /// Fired when an item is unequipped
        /// </summary>
        public event Action<EquipmentSlotType, ItemInstance> OnItemUnequipped;

        #endregion

        #region Constructor

        /// <summary>
        /// Create equipment container with stat container for stat modifications
        /// </summary>
        /// <param name="statContainer">StatContainer to apply equipment stats to</param>
        public EquipmentContainer(StatContainer statContainer)
        {
            this.statContainer = statContainer;

            // Initialize all equipment slots
            foreach (EquipmentSlotType slot in Enum.GetValues(typeof(EquipmentSlotType)))
            {
                if (slot != EquipmentSlotType.None)
                {
                    equippedItems[slot] = null;
                }
            }
        }

        #endregion

        #region Equip/Unequip

        /// <summary>
        /// Equip an item
        /// </summary>
        /// <param name="item">Item to equip</param>
        /// <returns>Previously equipped item (or null)</returns>
        public ItemInstance Equip(ItemInstance item)
        {
            if (item == null) return null;
            if (!item.IsEquipment) return null;

            var slot = item.Data.EquipmentSlot;
            if (slot == EquipmentSlotType.None) return null;

            // Get previously equipped item
            var previousItem = GetEquippedItem(slot);

            // Unequip previous item (remove stats)
            if (previousItem != null)
            {
                previousItem.RemoveEquipmentStats(statContainer);
            }

            // Equip new item
            equippedItems[slot] = item;

            // Apply new item stats
            item.ApplyEquipmentStats(statContainer);

            // Fire events
            OnEquipmentChanged?.Invoke(slot, previousItem, item);
            OnItemEquipped?.Invoke(slot, item);

            if (previousItem != null)
            {
                OnItemUnequipped?.Invoke(slot, previousItem);
            }

            return previousItem;
        }

        /// <summary>
        /// Unequip item from slot
        /// </summary>
        /// <param name="slot">Slot to unequip</param>
        /// <returns>Unequipped item (or null)</returns>
        public ItemInstance Unequip(EquipmentSlotType slot)
        {
            if (slot == EquipmentSlotType.None) return null;

            if (!equippedItems.TryGetValue(slot, out var item) || item == null)
                return null;

            // Remove stats
            item.RemoveEquipmentStats(statContainer);

            // Clear slot
            equippedItems[slot] = null;

            // Fire events
            OnEquipmentChanged?.Invoke(slot, item, null);
            OnItemUnequipped?.Invoke(slot, item);

            return item;
        }

        /// <summary>
        /// Unequip a specific item
        /// </summary>
        /// <param name="item">Item to unequip</param>
        /// <returns>True if unequipped</returns>
        public bool Unequip(ItemInstance item)
        {
            if (item == null) return false;

            foreach (var kvp in equippedItems)
            {
                if (kvp.Value == item)
                {
                    Unequip(kvp.Key);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Unequip all items
        /// </summary>
        /// <returns>List of unequipped items</returns>
        public List<ItemInstance> UnequipAll()
        {
            var unequipped = new List<ItemInstance>();

            foreach (EquipmentSlotType slot in Enum.GetValues(typeof(EquipmentSlotType)))
            {
                if (slot == EquipmentSlotType.None) continue;

                var item = Unequip(slot);
                if (item != null)
                {
                    unequipped.Add(item);
                }
            }

            return unequipped;
        }

        #endregion

        #region Query

        /// <summary>
        /// Get equipped item in slot
        /// </summary>
        public ItemInstance GetEquippedItem(EquipmentSlotType slot)
        {
            if (slot == EquipmentSlotType.None) return null;

            equippedItems.TryGetValue(slot, out var item);
            return item;
        }

        /// <summary>
        /// Check if slot has equipment
        /// </summary>
        public bool HasEquipment(EquipmentSlotType slot)
        {
            return GetEquippedItem(slot) != null;
        }

        /// <summary>
        /// Check if specific item is equipped
        /// </summary>
        public bool IsEquipped(ItemInstance item)
        {
            if (item == null) return false;

            foreach (var kvp in equippedItems)
            {
                if (kvp.Value == item)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if an item with specific ID is equipped
        /// </summary>
        public bool IsEquipped(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;

            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null && kvp.Value.ItemId == itemId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get all equipped items
        /// </summary>
        public List<ItemInstance> GetAllEquippedItems()
        {
            var items = new List<ItemInstance>();

            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    items.Add(kvp.Value);
                }
            }

            return items;
        }

        /// <summary>
        /// Get number of equipped items
        /// </summary>
        public int EquippedCount
        {
            get
            {
                int count = 0;
                foreach (var kvp in equippedItems)
                {
                    if (kvp.Value != null) count++;
                }
                return count;
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Can equip item?
        /// </summary>
        public bool CanEquip(ItemInstance item)
        {
            if (item == null) return false;
            if (!item.IsEquipment) return false;
            if (item.Data.EquipmentSlot == EquipmentSlotType.None) return false;

            // Could add level requirements check here
            // if (entity.Level < item.Data.RequiredLevel) return false;

            return true;
        }

        /// <summary>
        /// Reapply all equipment stats (useful after stat container changes)
        /// </summary>
        public void ReapplyAllStats()
        {
            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.RemoveEquipmentStats(statContainer);
                    kvp.Value.ApplyEquipmentStats(statContainer);
                }
            }
        }

        #endregion
    }
}
