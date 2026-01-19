using UnityEngine;
using System.Collections.Generic;
using System;

namespace MirrorRPG.Stat
{
    /// <summary>
    /// Defines a set of stats for an entity type (e.g., Player, Monster, Destructible)
    /// </summary>
    [CreateAssetMenu(fileName = "NewStatSet", menuName = "MirrorRPG/Stats/Stat Set")]
    public class StatSetDefinition : ScriptableObject
    {
        [Tooltip("Name of this stat set")]
        public string setName;

        [Tooltip("Stats included in this set")]
        public List<StatEntry> stats = new List<StatEntry>();

        /// <summary>
        /// Get a stat entry by ID
        /// </summary>
        public StatEntry GetEntry(string statId)
        {
            return stats.Find(e => e.definition != null && e.definition.statId == statId);
        }

        /// <summary>
        /// Check if this set contains a stat
        /// </summary>
        public bool HasStat(string statId)
        {
            return GetEntry(statId) != null;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(setName))
            {
                setName = name;
            }
        }
    }

    /// <summary>
    /// Entry for a stat in a stat set
    /// </summary>
    [Serializable]
    public class StatEntry
    {
        [Tooltip("The stat definition")]
        public StatDefinition definition;

        [Tooltip("Is this a resource stat (has Current/Max)?")]
        public bool isResource = false;

        [Tooltip("Override the default value from definition")]
        public bool overrideDefaultValue = false;

        [Tooltip("Custom default value (if override is true)")]
        public float customDefaultValue;

        /// <summary>
        /// Get the effective default value
        /// </summary>
        public float GetDefaultValue()
        {
            if (overrideDefaultValue)
                return customDefaultValue;
            return definition?.defaultValue ?? 0f;
        }
    }
}
