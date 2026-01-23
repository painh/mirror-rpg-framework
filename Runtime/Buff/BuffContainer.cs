using System;
using System.Collections.Generic;
using MirrorRPG.Stat;

namespace MirrorRPG.Buff
{
    /// <summary>
    /// Pure C# class that manages active buffs on an entity.
    /// Similar pattern to StatContainer.
    /// </summary>
    public class BuffContainer
    {
        // Active buffs
        private readonly Dictionary<string, List<BuffInstance>> activeBuffs = new Dictionary<string, List<BuffInstance>>();
        private readonly List<BuffInstance> allBuffs = new List<BuffInstance>();

        // Cached status effects
        private StatusEffect cachedStatusEffects = StatusEffect.None;
        private bool statusEffectsDirty = true;

        // Reference to stat container for applying modifiers
        private readonly StatContainer statContainer;

        /// <summary>
        /// All active buff instances
        /// </summary>
        public IReadOnlyList<BuffInstance> ActiveBuffs => allBuffs;

        /// <summary>
        /// Number of active buffs
        /// </summary>
        public int BuffCount => allBuffs.Count;

        /// <summary>
        /// Current combined status effects
        /// </summary>
        public StatusEffect ActiveStatusEffects
        {
            get
            {
                if (statusEffectsDirty)
                {
                    RecalculateStatusEffects();
                }
                return cachedStatusEffects;
            }
        }

        #region Events

        /// <summary>
        /// Event fired when a buff is applied
        /// </summary>
        public event Action<BuffInstance> OnBuffApplied;

        /// <summary>
        /// Event fired when a buff is removed
        /// </summary>
        public event Action<BuffInstance> OnBuffRemoved;

        /// <summary>
        /// Event fired when a buff ticks (DoT/HoT)
        /// </summary>
        public event Action<BuffInstance, float> OnBuffTick;

        /// <summary>
        /// Event fired when status effects change
        /// </summary>
        public event Action<StatusEffect, StatusEffect> OnStatusEffectsChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// Create a new BuffContainer
        /// </summary>
        /// <param name="statContainer">StatContainer for applying stat modifiers (can be null)</param>
        public BuffContainer(StatContainer statContainer = null)
        {
            this.statContainer = statContainer;
        }

        #endregion

        #region Update

        /// <summary>
        /// Update all active buffs. Call this from Entity.Update()
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            for (int i = allBuffs.Count - 1; i >= 0; i--)
            {
                var buff = allBuffs[i];

                bool ticked = buff.Update(deltaTime);

                if (ticked)
                {
                    ProcessTick(buff);
                }

                if (buff.IsExpired)
                {
                    RemoveBuffInternal(buff);
                }
            }
        }

        #endregion

        #region Apply/Remove Buff

        /// <summary>
        /// Apply a buff
        /// </summary>
        /// <param name="data">Buff data definition</param>
        /// <param name="source">Source that applied the buff (can be null)</param>
        /// <returns>The buff instance, or null if immune</returns>
        public BuffInstance ApplyBuff(BuffData data, object source = null)
        {
            if (data == null) return null;

            // Check if immune to debuffs
            if (data.isDebuff && HasStatusEffect(StatusEffect.Immune))
            {
                return null;
            }

            // Check for existing buff
            if (activeBuffs.TryGetValue(data.buffId, out var existingList) && existingList.Count > 0)
            {
                return HandleExistingBuff(existingList, data, source);
            }

            // Create new buff instance
            var buff = new BuffInstance(data, source);
            AddBuffInternal(buff);
            return buff;
        }

        /// <summary>
        /// Handle applying a buff that already exists
        /// </summary>
        private BuffInstance HandleExistingBuff(List<BuffInstance> existingList, BuffData data, object source)
        {
            var existing = existingList[0]; // For non-independent, use first

            switch (data.stackBehavior)
            {
                case StackBehavior.Ignore:
                    // Do nothing, just return existing
                    return existing;

                case StackBehavior.RefreshDuration:
                    existing.RefreshDuration();
                    return existing;

                case StackBehavior.AddDuration:
                    existing.AddDuration(data.duration);
                    return existing;

                case StackBehavior.StackAndRefresh:
                    if (existing.AddStack())
                    {
                        existing.RefreshDuration();
                        existing.RefreshModifiers(statContainer);
                    }
                    else
                    {
                        existing.RefreshDuration();
                    }
                    return existing;

                case StackBehavior.Independent:
                    if (existingList.Count < data.maxStacks)
                    {
                        var newBuff = new BuffInstance(data, source);
                        AddBuffInternal(newBuff);
                        return newBuff;
                    }
                    // At max stacks, refresh oldest
                    existingList[0].RefreshDuration();
                    return existingList[0];

                default:
                    return existing;
            }
        }

        /// <summary>
        /// Add a buff instance internally
        /// </summary>
        private void AddBuffInternal(BuffInstance buff)
        {
            // Add to collections
            if (!activeBuffs.ContainsKey(buff.BuffId))
            {
                activeBuffs[buff.BuffId] = new List<BuffInstance>();
            }
            activeBuffs[buff.BuffId].Add(buff);
            allBuffs.Add(buff);

            // Apply stat modifiers
            buff.ApplyModifiers(statContainer);

            // Mark status effects dirty
            if (buff.Data.statusEffects != StatusEffect.None)
            {
                statusEffectsDirty = true;
            }

            // Subscribe to buff events
            buff.OnExpired += OnBuffExpired;
            buff.OnStacksChanged += OnBuffStacksChanged;

            // Notify (VFX handling is done by subscribers)
            OnBuffApplied?.Invoke(buff);
        }

        /// <summary>
        /// Remove a buff instance internally
        /// </summary>
        private void RemoveBuffInternal(BuffInstance buff)
        {
            // Remove from collections
            if (activeBuffs.TryGetValue(buff.BuffId, out var list))
            {
                list.Remove(buff);
                if (list.Count == 0)
                {
                    activeBuffs.Remove(buff.BuffId);
                }
            }
            allBuffs.Remove(buff);

            // Remove stat modifiers
            buff.RemoveModifiers(statContainer);

            // Mark status effects dirty
            if (buff.Data.statusEffects != StatusEffect.None)
            {
                statusEffectsDirty = true;
            }

            // Unsubscribe
            buff.OnExpired -= OnBuffExpired;
            buff.OnStacksChanged -= OnBuffStacksChanged;

            // Notify (VFX handling is done by subscribers)
            OnBuffRemoved?.Invoke(buff);
        }

        /// <summary>
        /// Process a tick effect (DoT/HoT)
        /// 데미지/힐 처리는 OnBuffTick 이벤트 구독자(Entity)에서 처리
        /// </summary>
        private void ProcessTick(BuffInstance buff)
        {
            if (buff.Data.tickDamage != 0)
            {
                float damage = buff.Data.tickDamage * buff.Stacks;

                // 이벤트로 데미지/힐 양 전달 (Entity에서 TakeDamage/Heal 파이프라인 처리)
                OnBuffTick?.Invoke(buff, damage);
            }
        }

        #endregion

        #region Remove Methods

        /// <summary>
        /// Remove a buff by data
        /// </summary>
        public bool RemoveBuff(BuffData data)
        {
            if (data == null) return false;
            return RemoveBuff(data.buffId);
        }

        /// <summary>
        /// Remove a buff by ID (removes newest stack)
        /// </summary>
        public bool RemoveBuff(string buffId)
        {
            if (!activeBuffs.TryGetValue(buffId, out var list) || list.Count == 0)
                return false;

            var buff = list[list.Count - 1]; // Remove newest
            buff.Expire();
            return true;
        }

        /// <summary>
        /// Remove all stacks of a buff
        /// </summary>
        public int RemoveAllStacks(string buffId)
        {
            if (!activeBuffs.TryGetValue(buffId, out var list))
                return 0;

            int count = list.Count;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                list[i].Expire();
            }
            return count;
        }

        /// <summary>
        /// Remove all buffs from a specific source
        /// </summary>
        public int RemoveBuffsFromSource(object source)
        {
            int count = 0;
            for (int i = allBuffs.Count - 1; i >= 0; i--)
            {
                if (allBuffs[i].Source == source)
                {
                    allBuffs[i].Expire();
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Remove all dispellable debuffs
        /// </summary>
        public int DispelDebuffs(int maxCount = int.MaxValue)
        {
            int count = 0;
            for (int i = allBuffs.Count - 1; i >= 0 && count < maxCount; i--)
            {
                var buff = allBuffs[i];
                if (buff.Data.isDebuff && buff.Data.dispellable)
                {
                    buff.Expire();
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Remove all dispellable buffs (steal/purge)
        /// </summary>
        public int PurgeBuffs(int maxCount = int.MaxValue)
        {
            int count = 0;
            for (int i = allBuffs.Count - 1; i >= 0 && count < maxCount; i--)
            {
                var buff = allBuffs[i];
                if (!buff.Data.isDebuff && buff.Data.dispellable)
                {
                    buff.Expire();
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Clear all buffs
        /// </summary>
        public void ClearAllBuffs()
        {
            for (int i = allBuffs.Count - 1; i >= 0; i--)
            {
                allBuffs[i].Expire();
            }
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Check if entity has a buff
        /// </summary>
        public bool HasBuff(string buffId)
        {
            return activeBuffs.ContainsKey(buffId) && activeBuffs[buffId].Count > 0;
        }

        /// <summary>
        /// Check if entity has a buff
        /// </summary>
        public bool HasBuff(BuffData data)
        {
            return data != null && HasBuff(data.buffId);
        }

        /// <summary>
        /// Get a buff instance by ID
        /// </summary>
        public BuffInstance GetBuff(string buffId)
        {
            if (activeBuffs.TryGetValue(buffId, out var list) && list.Count > 0)
            {
                return list[0];
            }
            return null;
        }

        /// <summary>
        /// Get total stacks of a buff
        /// </summary>
        public int GetBuffStacks(string buffId)
        {
            if (!activeBuffs.TryGetValue(buffId, out var list))
                return 0;

            int total = 0;
            foreach (var buff in list)
            {
                total += buff.Stacks;
            }
            return total;
        }

        /// <summary>
        /// Check if entity has a status effect
        /// </summary>
        public bool HasStatusEffect(StatusEffect effect)
        {
            return (ActiveStatusEffects & effect) != 0;
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Notify that entity took damage (for OnHit removal condition)
        /// </summary>
        public void NotifyDamageTaken()
        {
            for (int i = allBuffs.Count - 1; i >= 0; i--)
            {
                allBuffs[i].OnTakeDamage();
            }
        }

        #endregion

        #region Internal

        /// <summary>
        /// Recalculate combined status effects from all active buffs
        /// </summary>
        private void RecalculateStatusEffects()
        {
            StatusEffect oldEffects = cachedStatusEffects;
            cachedStatusEffects = StatusEffect.None;

            foreach (var buff in allBuffs)
            {
                cachedStatusEffects |= buff.Data.statusEffects;
            }

            statusEffectsDirty = false;

            if (oldEffects != cachedStatusEffects)
            {
                OnStatusEffectsChanged?.Invoke(oldEffects, cachedStatusEffects);
            }
        }

        private void OnBuffExpired(BuffInstance buff)
        {
            // Will be handled in Update loop
        }

        private void OnBuffStacksChanged(BuffInstance buff, int oldStacks, int newStacks)
        {
            buff.RefreshModifiers(statContainer);
        }

        #endregion
    }
}
