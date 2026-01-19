using UnityEngine;

namespace MirrorRPG.Stat
{
    /// <summary>
    /// Defines a single stat type (e.g., Health, Strength, Speed)
    /// </summary>
    [CreateAssetMenu(fileName = "NewStat", menuName = "MirrorRPG/Stats/Stat Definition")]
    public class StatDefinition : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this stat")]
        public string statId;

        [Tooltip("Display name shown in UI")]
        public string displayName;

        [TextArea(1, 2)]
        public string description;

        [Header("Values")]
        [Tooltip("Default value when stat is created")]
        public float defaultValue = 100f;

        [Tooltip("Minimum allowed value")]
        public float minValue = 0f;

        [Tooltip("Maximum allowed value (-1 for no limit)")]
        public float maxValue = -1f;

        [Header("Behavior")]
        [Tooltip("Category for organization")]
        public StatCategory category = StatCategory.Resource;

        [Tooltip("Should this stat be synced over network?")]
        public bool networkSync = false;

        [Tooltip("Is this a percentage-based stat? (0-1 range typically)")]
        public bool isPercentage = false;

        /// <summary>
        /// Clamp value to min/max range
        /// </summary>
        public float ClampValue(float value)
        {
            if (maxValue < 0)
                return Mathf.Max(minValue, value);
            return Mathf.Clamp(value, minValue, maxValue);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(statId))
            {
                statId = name;
            }
        }
    }
}
