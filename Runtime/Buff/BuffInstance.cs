using System;
using System.Collections.Generic;
using UnityEngine;
using MirrorRPG.Stat;

namespace MirrorRPG.Buff
{
    /// <summary>
    /// Runtime instance of an active buff
    /// </summary>
    public class BuffInstance
    {
        /// <summary>
        /// The buff data definition
        /// </summary>
        public BuffData Data { get; private set; }

        /// <summary>
        /// Who applied this buff (can be null for environment effects)
        /// </summary>
        public object Source { get; private set; }

        /// <summary>
        /// Current number of stacks
        /// </summary>
        public int Stacks { get; private set; }

        /// <summary>
        /// Remaining duration in seconds
        /// </summary>
        public float RemainingDuration { get; private set; }

        /// <summary>
        /// Time until next tick (for DoT/HoT)
        /// </summary>
        public float NextTickTime { get; private set; }

        /// <summary>
        /// Remaining use count (for OnUseCount removal)
        /// </summary>
        public int RemainingUseCount { get; private set; }

        /// <summary>
        /// Is this buff marked for removal?
        /// </summary>
        public bool IsExpired { get; private set; }

        /// <summary>
        /// The spawned effect instance (if any)
        /// </summary>
        public GameObject EffectInstance { get; set; }

        /// <summary>
        /// Applied stat modifiers (for removal tracking)
        /// </summary>
        private Dictionary<string, StatModifier> appliedModifiers = new Dictionary<string, StatModifier>();

        /// <summary>
        /// Event fired when stacks change
        /// </summary>
        public event Action<BuffInstance, int, int> OnStacksChanged;

        /// <summary>
        /// Event fired when duration changes significantly
        /// </summary>
        public event Action<BuffInstance> OnDurationRefreshed;

        /// <summary>
        /// Event fired on each tick
        /// </summary>
        public event Action<BuffInstance> OnTick;

        /// <summary>
        /// Event fired when buff expires
        /// </summary>
        public event Action<BuffInstance> OnExpired;

        public BuffInstance(BuffData data, object source = null)
        {
            Data = data;
            Source = source;
            Stacks = 1;
            RemainingDuration = data.duration;
            RemainingUseCount = data.maxUseCount;
            NextTickTime = data.hasTick ? data.tickInterval : float.MaxValue;
            IsExpired = false;
        }

        /// <summary>
        /// Buff ID for easy access
        /// </summary>
        public string BuffId => Data.buffId;

        /// <summary>
        /// Is this a permanent buff (duration = 0)?
        /// </summary>
        public bool IsPermanent => Data.duration <= 0f;

        /// <summary>
        /// Duration percentage (0-1) remaining
        /// </summary>
        public float DurationPercent => Data.duration > 0 ? RemainingDuration / Data.duration : 1f;

        /// <summary>
        /// Update the buff (called every frame by BuffHandler)
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        /// <returns>True if tick occurred</returns>
        public bool Update(float deltaTime)
        {
            if (IsExpired) return false;

            bool ticked = false;

            // Update duration
            if (!IsPermanent && (Data.removeCondition & RemoveCondition.Time) != 0)
            {
                RemainingDuration -= deltaTime;
                if (RemainingDuration <= 0f)
                {
                    Expire();
                    return false;
                }
            }

            // Update tick
            if (Data.hasTick)
            {
                NextTickTime -= deltaTime;
                if (NextTickTime <= 0f)
                {
                    NextTickTime += Data.tickInterval;
                    OnTick?.Invoke(this);
                    ticked = true;
                }
            }

            return ticked;
        }

        /// <summary>
        /// Add a stack to this buff
        /// </summary>
        /// <returns>True if stack was added</returns>
        public bool AddStack()
        {
            if (!Data.stackable || Stacks >= Data.maxStacks)
                return false;

            int oldStacks = Stacks;
            Stacks++;
            OnStacksChanged?.Invoke(this, oldStacks, Stacks);
            return true;
        }

        /// <summary>
        /// Remove a stack from this buff
        /// </summary>
        /// <returns>True if buff should be removed (0 stacks)</returns>
        public bool RemoveStack()
        {
            if (Stacks <= 1)
            {
                Expire();
                return true;
            }

            int oldStacks = Stacks;
            Stacks--;
            OnStacksChanged?.Invoke(this, oldStacks, Stacks);
            return false;
        }

        /// <summary>
        /// Refresh the duration to full
        /// </summary>
        public void RefreshDuration()
        {
            RemainingDuration = Data.duration;
            OnDurationRefreshed?.Invoke(this);
        }

        /// <summary>
        /// Add to the remaining duration
        /// </summary>
        public void AddDuration(float amount)
        {
            RemainingDuration += amount;
            OnDurationRefreshed?.Invoke(this);
        }

        /// <summary>
        /// Consume a use (for OnUseCount removal)
        /// </summary>
        /// <returns>True if buff should be removed</returns>
        public bool ConsumeUse()
        {
            if ((Data.removeCondition & RemoveCondition.OnUseCount) == 0)
                return false;

            RemainingUseCount--;
            if (RemainingUseCount <= 0)
            {
                Expire();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called when entity takes damage (for OnHit removal)
        /// </summary>
        /// <returns>True if buff should be removed</returns>
        public bool OnTakeDamage()
        {
            if ((Data.removeCondition & RemoveCondition.OnHit) == 0)
                return false;

            Expire();
            return true;
        }

        /// <summary>
        /// Mark this buff as expired
        /// </summary>
        public void Expire()
        {
            if (IsExpired) return;

            IsExpired = true;
            OnExpired?.Invoke(this);
        }

        /// <summary>
        /// Apply stat modifiers to a stat container
        /// </summary>
        public void ApplyModifiers(StatContainer statContainer)
        {
            if (statContainer == null) return;

            RemoveModifiers(statContainer);

            foreach (var mod in Data.statModifiers)
            {
                var statMod = mod.ToStatModifier(this, Stacks);
                statContainer.AddModifier(mod.statId, statMod);
                appliedModifiers[mod.statId] = statMod;
            }
        }

        /// <summary>
        /// Remove stat modifiers from a stat container
        /// </summary>
        public void RemoveModifiers(StatContainer statContainer)
        {
            if (statContainer == null) return;

            foreach (var kvp in appliedModifiers)
            {
                statContainer.RemoveModifier(kvp.Key, kvp.Value);
            }
            appliedModifiers.Clear();
        }

        /// <summary>
        /// Reapply modifiers (when stacks change)
        /// </summary>
        public void RefreshModifiers(StatContainer statContainer)
        {
            ApplyModifiers(statContainer);
        }
    }
}
