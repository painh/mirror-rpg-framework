using System;
using System.Collections.Generic;
using MirrorRPG.Buff;
using MirrorRPG.Stat;

namespace MirrorRPG.Item
{
    /// <summary>
    /// Runtime instance of an item
    /// </summary>
    [Serializable]
    public class ItemInstance
    {
        /// <summary>
        /// Reference to the item data
        /// </summary>
        public IItemData Data { get; private set; }

        /// <summary>
        /// Current quantity (for stackable items)
        /// </summary>
        public int Quantity { get; private set; }

        /// <summary>
        /// Enhancement level (for equipment)
        /// </summary>
        public int EnhanceLevel { get; private set; }

        /// <summary>
        /// Unique instance ID for tracking
        /// </summary>
        public string InstanceId { get; private set; }

        /// <summary>
        /// Applied stat modifiers (for removal tracking)
        /// </summary>
        private Dictionary<string, StatModifier> appliedModifiers = new Dictionary<string, StatModifier>();

        /// <summary>
        /// Event fired when quantity changes
        /// </summary>
        public event Action<ItemInstance, int, int> OnQuantityChanged;

        /// <summary>
        /// Event fired when enhance level changes
        /// </summary>
        public event Action<ItemInstance, int, int> OnEnhanceLevelChanged;

        public ItemInstance(IItemData data, int quantity = 1)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Quantity = Math.Max(1, Math.Min(quantity, data.MaxStackSize));
            EnhanceLevel = 0;
            InstanceId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Item ID from data
        /// </summary>
        public string ItemId => Data.ItemId;

        /// <summary>
        /// Is this item stackable?
        /// </summary>
        public bool IsStackable => Data.IsStackable;

        /// <summary>
        /// Can add more to this stack?
        /// </summary>
        public bool CanStack => IsStackable && Quantity < Data.MaxStackSize;

        /// <summary>
        /// Space remaining in stack
        /// </summary>
        public int StackSpace => Data.MaxStackSize - Quantity;

        /// <summary>
        /// Is this equipment?
        /// </summary>
        public bool IsEquipment => Data.ItemType == ItemType.Equipment && Data.EquipmentSlot != EquipmentSlotType.None;

        /// <summary>
        /// Is this consumable?
        /// </summary>
        public bool IsConsumable => Data.ItemType == ItemType.Consumable;

        /// <summary>
        /// Add quantity to this stack
        /// </summary>
        /// <param name="amount">Amount to add</param>
        /// <returns>Overflow amount that couldn't be added</returns>
        public int AddQuantity(int amount)
        {
            if (!IsStackable || amount <= 0) return amount;

            int oldQuantity = Quantity;
            int maxAdd = Data.MaxStackSize - Quantity;
            int toAdd = Math.Min(amount, maxAdd);

            Quantity += toAdd;

            if (toAdd > 0)
            {
                OnQuantityChanged?.Invoke(this, oldQuantity, Quantity);
            }

            return amount - toAdd;
        }

        /// <summary>
        /// Remove quantity from this stack
        /// </summary>
        /// <param name="amount">Amount to remove</param>
        /// <returns>Amount actually removed</returns>
        public int RemoveQuantity(int amount)
        {
            if (amount <= 0) return 0;

            int oldQuantity = Quantity;
            int toRemove = Math.Min(amount, Quantity);

            Quantity -= toRemove;

            if (toRemove > 0)
            {
                OnQuantityChanged?.Invoke(this, oldQuantity, Quantity);
            }

            return toRemove;
        }

        /// <summary>
        /// Set quantity directly
        /// </summary>
        public void SetQuantity(int quantity)
        {
            int oldQuantity = Quantity;
            Quantity = Math.Max(0, Math.Min(quantity, Data.MaxStackSize));

            if (oldQuantity != Quantity)
            {
                OnQuantityChanged?.Invoke(this, oldQuantity, Quantity);
            }
        }

        /// <summary>
        /// Enhance the item
        /// </summary>
        /// <returns>True if enhanced successfully</returns>
        public bool Enhance()
        {
            if (!IsEquipment) return false;

            int oldLevel = EnhanceLevel;
            EnhanceLevel++;
            OnEnhanceLevelChanged?.Invoke(this, oldLevel, EnhanceLevel);
            return true;
        }

        /// <summary>
        /// Apply equipment buff to a buff container
        /// Creates a permanent buff with stat modifiers
        /// </summary>
        public void ApplyEquipmentBuff(BuffContainer buffContainer)
        {
            if (buffContainer == null || Data.StatModifiers == null || Data.StatModifiers.Count == 0)
                return;

            // Get the stat container from buff container's stat container reference
            // We need to apply modifiers directly since equipment doesn't use timed buffs
            RemoveEquipmentBuff(buffContainer);

            // Note: Equipment uses direct stat modifiers, not buffs
            // The StatContainer reference needs to be accessible
        }

        /// <summary>
        /// Apply equipment stat modifiers directly to stat container
        /// </summary>
        public void ApplyEquipmentStats(StatContainer statContainer)
        {
            if (statContainer == null) return;

            RemoveEquipmentStats(statContainer);

            foreach (var mod in Data.StatModifiers)
            {
                // Apply enhance level bonus (10% per level for flat, 5% for percent)
                float enhanceMultiplier = 1f + (EnhanceLevel * 0.1f);
                float value = mod.value * enhanceMultiplier;

                var statMod = new StatModifier(value, mod.modifierType, 0, this);
                statContainer.AddModifier(mod.statId, statMod);
                appliedModifiers[mod.statId] = statMod;
            }
        }

        /// <summary>
        /// Remove equipment buff from buff container
        /// </summary>
        public void RemoveEquipmentBuff(BuffContainer buffContainer)
        {
            // Equipment uses direct stat modifiers, handled in RemoveEquipmentStats
        }

        /// <summary>
        /// Remove equipment stat modifiers from stat container
        /// </summary>
        public void RemoveEquipmentStats(StatContainer statContainer)
        {
            if (statContainer == null) return;

            foreach (var kvp in appliedModifiers)
            {
                statContainer.RemoveModifier(kvp.Key, kvp.Value);
            }
            appliedModifiers.Clear();
        }

        /// <summary>
        /// Use consumable item
        /// </summary>
        /// <param name="buffContainer">Target buff container</param>
        /// <returns>True if used successfully</returns>
        public bool UseConsumable(BuffContainer buffContainer)
        {
            if (!IsConsumable || Quantity <= 0) return false;

            if (Data.UseEffect != null && buffContainer != null)
            {
                buffContainer.ApplyBuff(Data.UseEffect);
            }

            RemoveQuantity(1);
            return true;
        }

        /// <summary>
        /// Create a copy of this item instance
        /// </summary>
        public ItemInstance Clone()
        {
            var clone = new ItemInstance(Data, Quantity);
            clone.EnhanceLevel = EnhanceLevel;
            return clone;
        }

        /// <summary>
        /// Split stack into two
        /// </summary>
        /// <param name="amount">Amount to split off</param>
        /// <returns>New item instance with split amount, or null if invalid</returns>
        public ItemInstance Split(int amount)
        {
            if (!IsStackable || amount <= 0 || amount >= Quantity) return null;

            RemoveQuantity(amount);
            return new ItemInstance(Data, amount);
        }
    }
}
