using System;
using System.Collections.Generic;
using MirrorRPG.Item;

namespace MirrorRPG.Inventory
{
    /// <summary>
    /// Container that manages item slots
    /// </summary>
    public class InventoryContainer
    {
        private readonly ItemSlot[] slots;
        private readonly int capacity;

        /// <summary>
        /// Number of slots in this inventory
        /// </summary>
        public int Capacity => capacity;

        /// <summary>
        /// Number of occupied slots
        /// </summary>
        public int OccupiedSlots
        {
            get
            {
                int count = 0;
                for (int i = 0; i < capacity; i++)
                {
                    if (!slots[i].IsEmpty) count++;
                }
                return count;
            }
        }

        /// <summary>
        /// Number of empty slots
        /// </summary>
        public int EmptySlots => capacity - OccupiedSlots;

        /// <summary>
        /// Is the inventory full?
        /// </summary>
        public bool IsFull => EmptySlots == 0;

        /// <summary>
        /// Is the inventory empty?
        /// </summary>
        public bool IsEmpty => OccupiedSlots == 0;

        #region Events

        /// <summary>
        /// Fired when a slot changes
        /// </summary>
        public event Action<int, ItemSlot> OnSlotChanged;

        /// <summary>
        /// Fired when an item is added
        /// </summary>
        public event Action<ItemInstance, int> OnItemAdded;

        /// <summary>
        /// Fired when an item is removed
        /// </summary>
        public event Action<ItemInstance, int> OnItemRemoved;

        #endregion

        #region Constructor

        public InventoryContainer(int capacity)
        {
            this.capacity = Math.Max(1, capacity);
            slots = new ItemSlot[this.capacity];

            for (int i = 0; i < this.capacity; i++)
            {
                slots[i] = new ItemSlot(i);
            }
        }

        #endregion

        #region Slot Access

        /// <summary>
        /// Get slot at index
        /// </summary>
        public ItemSlot GetSlot(int index)
        {
            if (index < 0 || index >= capacity)
                return default;

            return slots[index];
        }

        /// <summary>
        /// Get item at slot index
        /// </summary>
        public ItemInstance GetItemAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= capacity)
                return null;

            return slots[slotIndex].Item;
        }

        /// <summary>
        /// Indexer for slot access
        /// </summary>
        public ItemSlot this[int index] => GetSlot(index);

        /// <summary>
        /// Get all slots
        /// </summary>
        public IReadOnlyList<ItemSlot> Slots => slots;

        #endregion

        #region Add Item

        /// <summary>
        /// Add item by data and quantity
        /// </summary>
        /// <param name="itemData">Item data</param>
        /// <param name="quantity">Quantity to add</param>
        /// <returns>Quantity actually added</returns>
        public int AddItem(IItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0) return 0;

            int remaining = quantity;
            int added = 0;

            // First, try to stack with existing items
            if (itemData.IsStackable)
            {
                for (int i = 0; i < capacity && remaining > 0; i++)
                {
                    if (!slots[i].IsEmpty && slots[i].ItemId == itemData.ItemId && slots[i].Item.CanStack)
                    {
                        int beforeAdd = slots[i].Item.Quantity;
                        remaining = slots[i].Item.AddQuantity(remaining);
                        int addedToSlot = slots[i].Item.Quantity - beforeAdd;

                        if (addedToSlot > 0)
                        {
                            added += addedToSlot;
                            OnSlotChanged?.Invoke(i, slots[i]);
                        }
                    }
                }
            }

            // Then, use empty slots for remaining quantity
            while (remaining > 0)
            {
                int emptySlot = FindEmptySlot();
                if (emptySlot < 0) break;

                int toAdd = Math.Min(remaining, itemData.MaxStackSize);
                var newItem = new ItemInstance(itemData, toAdd);
                slots[emptySlot].SetItem(newItem);

                added += toAdd;
                remaining -= toAdd;

                OnSlotChanged?.Invoke(emptySlot, slots[emptySlot]);
                OnItemAdded?.Invoke(newItem, toAdd);
            }

            return added;
        }

        /// <summary>
        /// Add an existing item instance
        /// </summary>
        /// <param name="item">Item instance to add</param>
        /// <returns>True if added successfully</returns>
        public bool AddItem(ItemInstance item)
        {
            if (item == null) return false;

            // Try to stack first
            if (item.IsStackable)
            {
                for (int i = 0; i < capacity; i++)
                {
                    if (!slots[i].IsEmpty && slots[i].ItemId == item.ItemId && slots[i].Item.CanStack)
                    {
                        int remaining = slots[i].Item.AddQuantity(item.Quantity);
                        if (remaining < item.Quantity)
                        {
                            OnSlotChanged?.Invoke(i, slots[i]);
                            if (remaining == 0)
                            {
                                return true;
                            }
                            item.SetQuantity(remaining);
                        }
                    }
                }
            }

            // Find empty slot
            int emptySlot = FindEmptySlot();
            if (emptySlot < 0) return false;

            slots[emptySlot].SetItem(item);
            OnSlotChanged?.Invoke(emptySlot, slots[emptySlot]);
            OnItemAdded?.Invoke(item, item.Quantity);

            return true;
        }

        /// <summary>
        /// Add item to specific slot
        /// </summary>
        public bool AddItemToSlot(int slotIndex, ItemInstance item)
        {
            if (slotIndex < 0 || slotIndex >= capacity) return false;
            if (item == null) return false;

            if (!slots[slotIndex].IsEmpty) return false;

            slots[slotIndex].SetItem(item);
            OnSlotChanged?.Invoke(slotIndex, slots[slotIndex]);
            OnItemAdded?.Invoke(item, item.Quantity);

            return true;
        }

        #endregion

        #region Remove Item

        /// <summary>
        /// Remove item by ID and quantity
        /// </summary>
        /// <param name="itemId">Item ID to remove</param>
        /// <param name="quantity">Quantity to remove</param>
        /// <returns>Quantity actually removed</returns>
        public int RemoveItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return 0;

            int remaining = quantity;
            int removed = 0;

            // Remove from slots in reverse order (LIFO)
            for (int i = capacity - 1; i >= 0 && remaining > 0; i--)
            {
                if (slots[i].IsEmpty || slots[i].ItemId != itemId) continue;

                var item = slots[i].Item;
                int toRemove = Math.Min(remaining, item.Quantity);
                int actualRemoved = item.RemoveQuantity(toRemove);

                removed += actualRemoved;
                remaining -= actualRemoved;

                if (item.Quantity <= 0)
                {
                    var removedItem = item;
                    slots[i].Clear();
                    OnItemRemoved?.Invoke(removedItem, actualRemoved);
                }

                OnSlotChanged?.Invoke(i, slots[i]);
            }

            return removed;
        }

        /// <summary>
        /// Remove item from specific slot
        /// </summary>
        /// <param name="slotIndex">Slot index</param>
        /// <param name="quantity">Quantity to remove (0 = all)</param>
        /// <returns>Removed item instance (or null)</returns>
        public ItemInstance RemoveItemFromSlot(int slotIndex, int quantity = 0)
        {
            if (slotIndex < 0 || slotIndex >= capacity) return null;
            if (slots[slotIndex].IsEmpty) return null;

            var item = slots[slotIndex].Item;

            if (quantity <= 0 || quantity >= item.Quantity)
            {
                // Remove all
                slots[slotIndex].Clear();
                OnSlotChanged?.Invoke(slotIndex, slots[slotIndex]);
                OnItemRemoved?.Invoke(item, item.Quantity);
                return item;
            }
            else
            {
                // Split stack
                var splitItem = item.Split(quantity);
                OnSlotChanged?.Invoke(slotIndex, slots[slotIndex]);
                OnItemRemoved?.Invoke(splitItem, quantity);
                return splitItem;
            }
        }

        /// <summary>
        /// Clear a slot
        /// </summary>
        public ItemInstance ClearSlot(int slotIndex)
        {
            return RemoveItemFromSlot(slotIndex, 0);
        }

        #endregion

        #region Move/Swap

        /// <summary>
        /// Move item from one slot to another
        /// </summary>
        public bool MoveItem(int fromSlot, int toSlot)
        {
            if (fromSlot < 0 || fromSlot >= capacity) return false;
            if (toSlot < 0 || toSlot >= capacity) return false;
            if (fromSlot == toSlot) return false;
            if (slots[fromSlot].IsEmpty) return false;

            var fromItem = slots[fromSlot].Item;

            // Try to stack
            if (!slots[toSlot].IsEmpty && slots[toSlot].ItemId == fromItem.ItemId && fromItem.IsStackable)
            {
                int remaining = slots[toSlot].Item.AddQuantity(fromItem.Quantity);

                if (remaining == 0)
                {
                    slots[fromSlot].Clear();
                }
                else
                {
                    fromItem.SetQuantity(remaining);
                }

                OnSlotChanged?.Invoke(fromSlot, slots[fromSlot]);
                OnSlotChanged?.Invoke(toSlot, slots[toSlot]);
                return true;
            }

            // Swap
            var toItem = slots[toSlot].Item;
            slots[toSlot].SetItem(fromItem);
            slots[fromSlot].SetItem(toItem);

            OnSlotChanged?.Invoke(fromSlot, slots[fromSlot]);
            OnSlotChanged?.Invoke(toSlot, slots[toSlot]);

            return true;
        }

        /// <summary>
        /// Swap items between two slots
        /// </summary>
        public void SwapSlots(int slotA, int slotB)
        {
            if (slotA < 0 || slotA >= capacity) return;
            if (slotB < 0 || slotB >= capacity) return;
            if (slotA == slotB) return;

            var itemA = slots[slotA].Item;
            var itemB = slots[slotB].Item;

            slots[slotA].SetItem(itemB);
            slots[slotB].SetItem(itemA);

            OnSlotChanged?.Invoke(slotA, slots[slotA]);
            OnSlotChanged?.Invoke(slotB, slots[slotB]);
        }

        #endregion

        #region Query

        /// <summary>
        /// Check if inventory has item with quantity
        /// </summary>
        public bool HasItem(string itemId, int quantity = 1)
        {
            return GetItemCount(itemId) >= quantity;
        }

        /// <summary>
        /// Get total count of an item
        /// </summary>
        public int GetItemCount(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return 0;

            int count = 0;
            for (int i = 0; i < capacity; i++)
            {
                if (!slots[i].IsEmpty && slots[i].ItemId == itemId)
                {
                    count += slots[i].Item.Quantity;
                }
            }
            return count;
        }

        /// <summary>
        /// Find first slot containing item
        /// </summary>
        public int FindItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return -1;

            for (int i = 0; i < capacity; i++)
            {
                if (!slots[i].IsEmpty && slots[i].ItemId == itemId)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find all slots containing item
        /// </summary>
        public List<int> FindAllSlots(string itemId)
        {
            var result = new List<int>();
            if (string.IsNullOrEmpty(itemId)) return result;

            for (int i = 0; i < capacity; i++)
            {
                if (!slots[i].IsEmpty && slots[i].ItemId == itemId)
                {
                    result.Add(i);
                }
            }
            return result;
        }

        /// <summary>
        /// Find first empty slot
        /// </summary>
        public int FindEmptySlot()
        {
            for (int i = 0; i < capacity; i++)
            {
                if (slots[i].IsEmpty)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Can add item?
        /// </summary>
        public bool CanAddItem(IItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0) return false;

            int remaining = quantity;

            // Check stackable space
            if (itemData.IsStackable)
            {
                for (int i = 0; i < capacity && remaining > 0; i++)
                {
                    if (!slots[i].IsEmpty && slots[i].ItemId == itemData.ItemId && slots[i].Item.CanStack)
                    {
                        remaining -= slots[i].Item.StackSpace;
                    }
                }
            }

            // Check empty slots
            int emptyCount = EmptySlots;
            int slotsNeeded = (int)Math.Ceiling((double)remaining / itemData.MaxStackSize);

            return emptyCount >= slotsNeeded;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Clear all slots
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < capacity; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    var item = slots[i].Item;
                    slots[i].Clear();
                    OnSlotChanged?.Invoke(i, slots[i]);
                    OnItemRemoved?.Invoke(item, item.Quantity);
                }
            }
        }

        /// <summary>
        /// Sort inventory by item type and rarity
        /// </summary>
        public void Sort()
        {
            var items = new List<ItemInstance>();

            // Collect all items
            for (int i = 0; i < capacity; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    items.Add(slots[i].Item);
                    slots[i].Clear();
                }
            }

            // Sort by type, then rarity, then name
            items.Sort((a, b) =>
            {
                int typeCompare = a.Data.ItemType.CompareTo(b.Data.ItemType);
                if (typeCompare != 0) return typeCompare;

                int rarityCompare = b.Data.Rarity.CompareTo(a.Data.Rarity); // Descending
                if (rarityCompare != 0) return rarityCompare;

                return string.Compare(a.Data.DisplayName, b.Data.DisplayName, StringComparison.Ordinal);
            });

            // Redistribute items
            int slotIndex = 0;
            foreach (var item in items)
            {
                if (slotIndex >= capacity) break;
                slots[slotIndex].SetItem(item);
                slotIndex++;
            }

            // Notify all slots changed
            for (int i = 0; i < capacity; i++)
            {
                OnSlotChanged?.Invoke(i, slots[i]);
            }
        }

        #endregion
    }
}
