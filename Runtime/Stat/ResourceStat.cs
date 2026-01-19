using System;
using UnityEngine;

namespace MirrorRPG.Stat
{
    /// <summary>
    /// A stat that has both Current and Max values (like Health, Stamina, Mana)
    /// Max is calculated from base + modifiers, Current is a runtime value that can't exceed Max
    /// </summary>
    [Serializable]
    public class ResourceStat : Stat
    {
        [SerializeField] private float currentValue;

        /// <summary>
        /// Current value (cannot exceed MaxValue)
        /// </summary>
        public float CurrentValue
        {
            get => currentValue;
            set
            {
                float clamped = Mathf.Clamp(value, 0f, MaxValue);
                if (currentValue != clamped)
                {
                    currentValue = clamped;
                    OnCurrentChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Maximum value (calculated from base + modifiers)
        /// </summary>
        public float MaxValue => Value;

        /// <summary>
        /// Current as percentage of max (0-1)
        /// </summary>
        public float Percent => MaxValue > 0 ? currentValue / MaxValue : 0f;

        /// <summary>
        /// Is current value at or below zero?
        /// </summary>
        public bool IsDepleted => currentValue <= 0f;

        /// <summary>
        /// Is current value at max?
        /// </summary>
        public bool IsFull => currentValue >= MaxValue;

        /// <summary>
        /// Fired when current value changes
        /// </summary>
        public event Action<ResourceStat> OnCurrentChanged;

        /// <summary>
        /// Fired when depleted (current reaches 0)
        /// </summary>
        public event Action<ResourceStat> OnDepleted;

        public ResourceStat() : base() { }

        public ResourceStat(StatDefinition definition) : base(definition)
        {
            currentValue = definition?.defaultValue ?? 0f;
        }

        public ResourceStat(StatDefinition definition, float baseValue) : base(definition, baseValue)
        {
            currentValue = baseValue;
        }

        /// <summary>
        /// Reduce current value by amount
        /// </summary>
        /// <param name="amount">Amount to reduce (positive number)</param>
        /// <returns>Actual amount reduced</returns>
        public float Reduce(float amount)
        {
            if (amount <= 0) return 0f;

            float oldValue = currentValue;
            float newValue = Mathf.Max(0f, currentValue - amount);
            currentValue = newValue;

            float actualReduction = oldValue - newValue;

            if (actualReduction > 0)
            {
                OnCurrentChanged?.Invoke(this);

                if (currentValue <= 0f)
                {
                    OnDepleted?.Invoke(this);
                }
            }

            return actualReduction;
        }

        /// <summary>
        /// Increase current value by amount (capped at max)
        /// </summary>
        /// <param name="amount">Amount to add (positive number)</param>
        /// <returns>Actual amount added</returns>
        public float Restore(float amount)
        {
            if (amount <= 0) return 0f;

            float oldValue = currentValue;
            float newValue = Mathf.Min(MaxValue, currentValue + amount);
            currentValue = newValue;

            float actualRestore = newValue - oldValue;

            if (actualRestore > 0)
            {
                OnCurrentChanged?.Invoke(this);
            }

            return actualRestore;
        }

        /// <summary>
        /// Set current to max
        /// </summary>
        public void Fill()
        {
            CurrentValue = MaxValue;
        }

        /// <summary>
        /// Set current to zero
        /// </summary>
        public void Deplete()
        {
            CurrentValue = 0f;
        }

        /// <summary>
        /// Set current value directly (clamped to 0-Max)
        /// Used for network sync
        /// </summary>
        public void SetCurrent(float value)
        {
            CurrentValue = value;
        }

        /// <summary>
        /// When max changes, optionally adjust current proportionally
        /// </summary>
        public override void AddModifier(StatModifier modifier)
        {
            float oldMax = MaxValue;
            base.AddModifier(modifier);
            float newMax = MaxValue;

            // Keep current at same percentage of max
            if (oldMax > 0 && newMax != oldMax)
            {
                float percent = currentValue / oldMax;
                currentValue = Mathf.Min(newMax, percent * newMax);
                OnCurrentChanged?.Invoke(this);
            }
        }

        public override bool RemoveModifier(StatModifier modifier)
        {
            float oldMax = MaxValue;
            bool result = base.RemoveModifier(modifier);

            if (result)
            {
                float newMax = MaxValue;
                // Clamp current to new max
                if (currentValue > newMax)
                {
                    currentValue = newMax;
                    OnCurrentChanged?.Invoke(this);
                }
            }

            return result;
        }
    }
}
