using System;
using System.Collections.Generic;
using UnityEngine;
using MirrorRPG.Stat;
using Combat;

namespace MirrorRPG.Buff
{
    /// <summary>
    /// Component that manages active buffs on an entity
    /// </summary>
    public class BuffHandler : MonoBehaviour
    {
        [Header("Effect Spawn Point")]
        [SerializeField] private Transform effectSpawnPoint;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        // Active buffs
        private Dictionary<string, List<BuffInstance>> activeBuffs = new Dictionary<string, List<BuffInstance>>();
        private List<BuffInstance> allBuffs = new List<BuffInstance>();

        // Cached status effects
        private StatusEffect cachedStatusEffects = StatusEffect.None;
        private bool statusEffectsDirty = true;

        // Owner reference (IBuffable provides StatContainer)
        private IBuffable buffableOwner;

        /// <summary>
        /// StatContainer from IBuffable owner
        /// </summary>
        private StatContainer StatContainer => buffableOwner?.StatContainer;

        /// <summary>
        /// All active buff instances
        /// </summary>
        public IReadOnlyList<BuffInstance> ActiveBuffs => allBuffs;

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

        private void Awake()
        {
            buffableOwner = GetComponent<IBuffable>();

            if (buffableOwner == null)
            {
                Debug.LogError($"[BuffHandler] {name}: IBuffable owner not found! BuffHandler requires an IBuffable component.");
            }

            if (effectSpawnPoint == null)
            {
                effectSpawnPoint = transform;
            }
        }

        private void Update()
        {
            UpdateBuffs(Time.deltaTime);
        }

        /// <summary>
        /// Update all active buffs
        /// </summary>
        private void UpdateBuffs(float deltaTime)
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

        /// <summary>
        /// Apply a buff to this entity
        /// </summary>
        public BuffInstance ApplyBuff(BuffData data, object source = null)
        {
            if (data == null) return null;

            // Check if immune
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
                        existing.RefreshModifiers(StatContainer);
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
            buff.ApplyModifiers(StatContainer);

            // Mark status effects dirty
            if (buff.Data.statusEffects != StatusEffect.None)
            {
                statusEffectsDirty = true;
            }

            // Spawn effect
            if (buff.Data.effectPrefab != null)
            {
                buff.EffectInstance = Instantiate(buff.Data.effectPrefab, effectSpawnPoint);
            }

            // Spawn apply effect
            if (buff.Data.applyEffectPrefab != null)
            {
                var applyEffect = Instantiate(buff.Data.applyEffectPrefab, effectSpawnPoint.position, Quaternion.identity);
                Destroy(applyEffect, 3f);
            }

            // Subscribe to buff events
            buff.OnExpired += OnBuffExpired;
            buff.OnStacksChanged += OnBuffStacksChanged;

            // Notify
            OnBuffApplied?.Invoke(buff);
            buffableOwner?.OnBuffApplied(buff);
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
            buff.RemoveModifiers(StatContainer);

            // Mark status effects dirty
            if (buff.Data.statusEffects != StatusEffect.None)
            {
                statusEffectsDirty = true;
            }

            // Destroy effect
            if (buff.EffectInstance != null)
            {
                Destroy(buff.EffectInstance);
            }

            // Spawn remove effect
            if (buff.Data.removeEffectPrefab != null)
            {
                var removeEffect = Instantiate(buff.Data.removeEffectPrefab, effectSpawnPoint.position, Quaternion.identity);
                Destroy(removeEffect, 3f);
            }

            // Unsubscribe
            buff.OnExpired -= OnBuffExpired;
            buff.OnStacksChanged -= OnBuffStacksChanged;

            // Notify
            OnBuffRemoved?.Invoke(buff);
            buffableOwner?.OnBuffRemoved(buff);
        }

        /// <summary>
        /// Process a tick effect
        /// </summary>
        private void ProcessTick(BuffInstance buff)
        {
            if (buff.Data.tickDamage != 0 && StatContainer != null)
            {
                float damage = buff.Data.tickDamage * buff.Stacks;

                if (damage > 0)
                {
                    // Damage
                    StatContainer.TakeDamage(damage);
                }
                else
                {
                    // Healing (negative damage)
                    StatContainer.Heal(-damage);
                }

                OnBuffTick?.Invoke(buff, damage);
            }
        }

        /// <summary>
        /// Remove a buff by data
        /// </summary>
        public bool RemoveBuff(BuffData data)
        {
            if (data == null) return false;
            return RemoveBuff(data.buffId);
        }

        /// <summary>
        /// Remove a buff by ID
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
        /// Remove all buffs from a source
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
        /// Get a buff instance
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

        /// <summary>
        /// Notify that entity took damage (for OnHit removal)
        /// </summary>
        public void NotifyDamageTaken()
        {
            for (int i = allBuffs.Count - 1; i >= 0; i--)
            {
                allBuffs[i].OnTakeDamage();
            }
        }

        /// <summary>
        /// Recalculate combined status effects
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
                buffableOwner?.OnStatusEffectsChanged(oldEffects, cachedStatusEffects);
            }
        }

        private void OnBuffExpired(BuffInstance buff)
        {
            // Will be handled in Update loop
        }

        private void OnBuffStacksChanged(BuffInstance buff, int oldStacks, int newStacks)
        {
            buff.RefreshModifiers(StatContainer);
        }

        #region Debug

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(320, 10, 300, 500));
            GUILayout.Label($"=== {name} Buffs ===");
            GUILayout.Label($"Status: {ActiveStatusEffects}");

            foreach (var buff in allBuffs)
            {
                string stackStr = buff.Data.stackable ? $" x{buff.Stacks}" : "";
                string durationStr = buff.IsPermanent ? "∞" : $"{buff.RemainingDuration:F1}s";
                string typeStr = buff.Data.isDebuff ? "[D]" : "[B]";
                GUILayout.Label($"{typeStr} {buff.Data.displayName}{stackStr} ({durationStr})");
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
