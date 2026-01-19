using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorRPG.Stat
{
    /// <summary>
    /// Runtime stat instance with base value and modifiers
    /// </summary>
    [Serializable]
    public class Stat
    {
        [SerializeField] protected StatDefinition definition;
        [SerializeField] protected float baseValue;

        protected List<StatModifier> modifiers = new List<StatModifier>();
        protected float cachedValue;
        protected bool isDirty = true;

        public StatDefinition Definition => definition;
        public string StatId => definition?.statId ?? "";
        public float BaseValue
        {
            get => baseValue;
            set
            {
                if (baseValue != value)
                {
                    baseValue = value;
                    isDirty = true;
                    OnValueChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Final calculated value after all modifiers
        /// </summary>
        public float Value
        {
            get
            {
                if (isDirty)
                {
                    cachedValue = CalculateValue();
                    isDirty = false;
                }
                return cachedValue;
            }
        }

        public IReadOnlyList<StatModifier> Modifiers => modifiers;

        /// <summary>
        /// Fired when value changes (base or modifiers)
        /// </summary>
        public event Action<Stat> OnValueChanged;

        public Stat() { }

        public Stat(StatDefinition definition)
        {
            this.definition = definition;
            this.baseValue = definition?.defaultValue ?? 0f;
        }

        public Stat(StatDefinition definition, float baseValue)
        {
            this.definition = definition;
            this.baseValue = baseValue;
        }

        /// <summary>
        /// Calculate final value: (Base + Flat) * (1 + PercentAdd) * PercentMult
        /// </summary>
        protected virtual float CalculateValue()
        {
            float flat = 0f;
            float percentAdd = 0f;
            float percentMult = 1f;

            // Sort modifiers by type then priority
            modifiers.Sort((a, b) =>
            {
                int typeCompare = a.type.CompareTo(b.type);
                return typeCompare != 0 ? typeCompare : a.priority.CompareTo(b.priority);
            });

            foreach (var mod in modifiers)
            {
                switch (mod.type)
                {
                    case ModifierType.Flat:
                        flat += mod.value;
                        break;
                    case ModifierType.PercentAdd:
                        percentAdd += mod.value;
                        break;
                    case ModifierType.PercentMult:
                        percentMult *= mod.value;
                        break;
                }
            }

            float finalValue = (baseValue + flat) * (1f + percentAdd) * percentMult;

            // Clamp to definition limits
            if (definition != null)
            {
                finalValue = definition.ClampValue(finalValue);
            }

            return finalValue;
        }

        /// <summary>
        /// Add a modifier to this stat
        /// </summary>
        public virtual void AddModifier(StatModifier modifier)
        {
            modifiers.Add(modifier);
            isDirty = true;
            OnValueChanged?.Invoke(this);
        }

        /// <summary>
        /// Remove a specific modifier
        /// </summary>
        public virtual bool RemoveModifier(StatModifier modifier)
        {
            if (modifiers.Remove(modifier))
            {
                isDirty = true;
                OnValueChanged?.Invoke(this);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove all modifiers from a specific source
        /// </summary>
        public virtual int RemoveModifiersFromSource(object source)
        {
            int count = modifiers.RemoveAll(m => m.source == source);
            if (count > 0)
            {
                isDirty = true;
                OnValueChanged?.Invoke(this);
            }
            return count;
        }

        /// <summary>
        /// Remove all modifiers
        /// </summary>
        public virtual void ClearModifiers()
        {
            if (modifiers.Count > 0)
            {
                modifiers.Clear();
                isDirty = true;
                OnValueChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Force recalculation on next Value access
        /// </summary>
        public void SetDirty()
        {
            isDirty = true;
        }
    }
}
