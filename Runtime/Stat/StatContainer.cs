using System;
using System.Collections.Generic;

namespace MirrorRPG.Stat
{
    /// <summary>
    /// Holds and manages stats for an entity (pure class, not MonoBehaviour)
    /// </summary>
    [Serializable]
    public class StatContainer
    {
        // Runtime stats
        private Dictionary<string, Stat> stats = new Dictionary<string, Stat>();
        private StatSetDefinition statSet;
        private bool isInitialized = false;

        // Built-in stat IDs (constants for common stats)
        public static class StatIds
        {
            public const string Health = "Health";
            public const string Stamina = "Stamina";
            public const string Mana = "Mana";
            public const string Attack = "Attack";
            public const string Defense = "Defense";
            public const string Speed = "Speed";
            public const string CritRate = "CritRate";
            public const string CritDamage = "CritDamage";

            // Stagger (경직 저항)
            public const string StaggerResistance = "StaggerResistance";

            // Resistance stats
            public const string PhysicalHitResist = "PhysicalHitResist";
            public const string PhysicalSlashResist = "PhysicalSlashResist";
            public const string FireResist = "FireResist";
            public const string IceResist = "IceResist";
            public const string LightningResist = "LightningResist";
            public const string LightResist = "LightResist";
            public const string DarknessResist = "DarknessResist";
            public const string PoisonResist = "PoisonResist";
            public const string BleedingResist = "BleedingResist";
        }

        /// <summary>
        /// All stats in this container
        /// </summary>
        public IReadOnlyDictionary<string, Stat> Stats => stats;

        /// <summary>
        /// The stat set definition
        /// </summary>
        public StatSetDefinition StatSet => statSet;

        /// <summary>
        /// Is initialized?
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// Event fired when any stat value changes
        /// </summary>
        public event Action<string, Stat> OnStatChanged;

        /// <summary>
        /// Event fired when a resource stat's current value changes
        /// </summary>
        public event Action<string, ResourceStat> OnResourceChanged;

        /// <summary>
        /// Event fired when a resource stat is depleted
        /// </summary>
        public event Action<string, ResourceStat> OnResourceDepleted;

        /// <summary>
        /// Default constructor
        /// </summary>
        public StatContainer() { }

        /// <summary>
        /// Constructor with stat set (auto-initializes)
        /// </summary>
        public StatContainer(StatSetDefinition statSet)
        {
            Initialize(statSet);
        }

        /// <summary>
        /// Initialize with a stat set
        /// </summary>
        public void Initialize(StatSetDefinition newStatSet)
        {
            statSet = newStatSet;
            isInitialized = false;
            stats.Clear();
            InitializeStats();
        }

        /// <summary>
        /// Initialize stats from the stat set
        /// </summary>
        private void InitializeStats()
        {
            if (isInitialized) return;
            if (statSet == null) return;

            stats.Clear();

            foreach (var entry in statSet.stats)
            {
                if (entry.definition == null) continue;

                string statId = entry.definition.statId;
                float defaultValue = entry.GetDefaultValue();

                Stat stat;
                if (entry.isResource)
                {
                    var resourceStat = new ResourceStat(entry.definition, defaultValue);
                    resourceStat.OnCurrentChanged += (s) => OnResourceChanged?.Invoke(statId, s);
                    resourceStat.OnDepleted += (s) => OnResourceDepleted?.Invoke(statId, s);
                    stat = resourceStat;
                }
                else
                {
                    stat = new Stat(entry.definition, defaultValue);
                }

                stat.OnValueChanged += (s) => OnStatChanged?.Invoke(statId, s);
                stats[statId] = stat;
            }

            isInitialized = true;
        }

        #region Stat Access

        /// <summary>
        /// Check if this container has a stat
        /// </summary>
        public bool HasStat(string statId)
        {
            return stats.ContainsKey(statId);
        }

        /// <summary>
        /// Get a stat by ID
        /// </summary>
        public Stat GetStat(string statId)
        {
            stats.TryGetValue(statId, out var stat);
            return stat;
        }

        /// <summary>
        /// Get a resource stat by ID
        /// </summary>
        public ResourceStat GetResourceStat(string statId)
        {
            return GetStat(statId) as ResourceStat;
        }

        /// <summary>
        /// Get the current value of a stat
        /// </summary>
        public float GetStatValue(string statId)
        {
            var stat = GetStat(statId);
            return stat?.Value ?? 0f;
        }

        /// <summary>
        /// Get the base value of a stat
        /// </summary>
        public float GetStatBaseValue(string statId)
        {
            var stat = GetStat(statId);
            return stat?.BaseValue ?? 0f;
        }

        /// <summary>
        /// Set the base value of a stat
        /// </summary>
        public void SetStatBaseValue(string statId, float value)
        {
            var stat = GetStat(statId);
            if (stat != null)
            {
                stat.BaseValue = value;
            }
        }

        #endregion

        #region Resource Stat Helpers

        /// <summary>
        /// Get current value of a resource stat
        /// </summary>
        public float GetCurrentValue(string statId)
        {
            var resource = GetResourceStat(statId);
            return resource?.CurrentValue ?? 0f;
        }

        /// <summary>
        /// Get max value of a resource stat
        /// </summary>
        public float GetMaxValue(string statId)
        {
            var resource = GetResourceStat(statId);
            return resource?.MaxValue ?? 0f;
        }

        /// <summary>
        /// Get percentage (0-1) of a resource stat
        /// </summary>
        public float GetPercent(string statId)
        {
            var resource = GetResourceStat(statId);
            return resource?.Percent ?? 0f;
        }

        /// <summary>
        /// Reduce a resource stat's current value
        /// </summary>
        public float ReduceResource(string statId, float amount)
        {
            var resource = GetResourceStat(statId);
            return resource?.Reduce(amount) ?? 0f;
        }

        /// <summary>
        /// Restore a resource stat's current value
        /// </summary>
        public float RestoreResource(string statId, float amount)
        {
            var resource = GetResourceStat(statId);
            return resource?.Restore(amount) ?? 0f;
        }

        /// <summary>
        /// Fill a resource stat to max
        /// </summary>
        public void FillResource(string statId)
        {
            var resource = GetResourceStat(statId);
            resource?.Fill();
        }

        /// <summary>
        /// Set current value directly (for network sync)
        /// </summary>
        public void SetResourceCurrent(string statId, float value)
        {
            var resource = GetResourceStat(statId);
            resource?.SetCurrent(value);
        }

        #endregion

        #region Modifier Helpers

        /// <summary>
        /// Add a modifier to a stat
        /// </summary>
        public void AddModifier(string statId, StatModifier modifier)
        {
            var stat = GetStat(statId);
            stat?.AddModifier(modifier);
        }

        /// <summary>
        /// Remove a modifier from a stat
        /// </summary>
        public bool RemoveModifier(string statId, StatModifier modifier)
        {
            var stat = GetStat(statId);
            return stat?.RemoveModifier(modifier) ?? false;
        }

        /// <summary>
        /// Remove all modifiers from a source across all stats
        /// </summary>
        public int RemoveModifiersFromSource(object source)
        {
            int total = 0;
            foreach (var stat in stats.Values)
            {
                total += stat.RemoveModifiersFromSource(source);
            }
            return total;
        }

        /// <summary>
        /// Clear all modifiers from all stats
        /// </summary>
        public void ClearAllModifiers()
        {
            foreach (var stat in stats.Values)
            {
                stat.ClearModifiers();
            }
        }

        #endregion

        #region Health Shortcuts (for common use)

        /// <summary>
        /// Shortcut: Current health
        /// </summary>
        public float CurrentHealth => GetCurrentValue(StatIds.Health);

        /// <summary>
        /// Shortcut: Max health
        /// </summary>
        public float MaxHealth => GetMaxValue(StatIds.Health);

        /// <summary>
        /// Shortcut: Health percentage (0-1)
        /// </summary>
        public float HealthPercent => GetPercent(StatIds.Health);

        /// <summary>
        /// Shortcut: Is health depleted?
        /// </summary>
        public bool IsHealthDepleted => GetResourceStat(StatIds.Health)?.IsDepleted ?? true;

        /// <summary>
        /// Shortcut: Reduce health
        /// </summary>
        public float TakeDamage(float damage) => ReduceResource(StatIds.Health, damage);

        /// <summary>
        /// Shortcut: Restore health
        /// </summary>
        public float Heal(float amount) => RestoreResource(StatIds.Health, amount);

        #endregion

        #region Stamina Shortcuts

        /// <summary>
        /// Shortcut: Current stamina
        /// </summary>
        public float CurrentStamina => GetCurrentValue(StatIds.Stamina);

        /// <summary>
        /// Shortcut: Max stamina
        /// </summary>
        public float MaxStamina => GetMaxValue(StatIds.Stamina);

        /// <summary>
        /// Shortcut: Stamina percentage (0-1)
        /// </summary>
        public float StaminaPercent => GetPercent(StatIds.Stamina);

        /// <summary>
        /// Shortcut: Use stamina
        /// </summary>
        public float UseStamina(float amount) => ReduceResource(StatIds.Stamina, amount);

        /// <summary>
        /// Shortcut: Restore stamina
        /// </summary>
        public float RestoreStamina(float amount) => RestoreResource(StatIds.Stamina, amount);

        #endregion

        #region Stagger Shortcuts

        /// <summary>
        /// Shortcut: Stagger resistance (임계값)
        /// </summary>
        public float StaggerResistance => GetStatValue(StatIds.StaggerResistance);

        #endregion

        #region Network Sync Support

        /// <summary>
        /// Get all stats that should be network synced
        /// </summary>
        public IEnumerable<KeyValuePair<string, Stat>> GetNetworkSyncStats()
        {
            foreach (var kvp in stats)
            {
                if (kvp.Value.Definition != null && kvp.Value.Definition.networkSync)
                {
                    yield return kvp;
                }
            }
        }

        /// <summary>
        /// Serialize stats for network sync (only networkSync=true stats)
        /// </summary>
        public Dictionary<string, StatSyncData> GetSyncData()
        {
            var data = new Dictionary<string, StatSyncData>();
            foreach (var kvp in GetNetworkSyncStats())
            {
                var syncData = new StatSyncData
                {
                    baseValue = kvp.Value.BaseValue,
                    currentValue = (kvp.Value is ResourceStat rs) ? rs.CurrentValue : kvp.Value.Value
                };
                data[kvp.Key] = syncData;
            }
            return data;
        }

        /// <summary>
        /// Apply sync data from network
        /// </summary>
        public void ApplySyncData(Dictionary<string, StatSyncData> data)
        {
            foreach (var kvp in data)
            {
                var stat = GetStat(kvp.Key);
                if (stat != null)
                {
                    stat.BaseValue = kvp.Value.baseValue;
                    if (stat is ResourceStat rs)
                    {
                        rs.SetCurrent(kvp.Value.currentValue);
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Data structure for network sync
    /// </summary>
    [Serializable]
    public struct StatSyncData
    {
        public float baseValue;
        public float currentValue;
    }
}
