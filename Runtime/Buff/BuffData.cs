using UnityEngine;
using System.Collections.Generic;
using Combat;

namespace MirrorRPG.Buff
{
    /// <summary>
    /// ScriptableObject defining a buff/debuff
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuff", menuName = "MirrorRPG/Buff/Buff Data")]
    public class BuffData : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this buff")]
        public string buffId;

        [Tooltip("Display name shown in UI")]
        public string displayName;

        [TextArea(2, 4)]
        public string description;

        [Tooltip("Icon for UI display")]
        public Sprite icon;

        [Tooltip("Is this a debuff (negative effect)?")]
        public bool isDebuff;

        [Header("Visual Effects")]
        [Tooltip("Effect prefab to spawn when buff is active")]
        public GameObject effectPrefab;

        [Tooltip("Effect prefab to spawn on buff apply")]
        public GameObject applyEffectPrefab;

        [Tooltip("Effect prefab to spawn on buff remove")]
        public GameObject removeEffectPrefab;

        [Tooltip("Fade in/out effect when apply/remove VFX is not set")]
        public BuffFadeType fadeType = BuffFadeType.None;

        [Tooltip("Duration of fade effect in seconds")]
        public float fadeDuration = 0.5f;

        [Header("Duration")]
        [Tooltip("Duration in seconds (0 = permanent until manually removed)")]
        public float duration = 10f;

        [Tooltip("Conditions for removing this buff")]
        public RemoveCondition removeCondition = RemoveCondition.Time;

        [Tooltip("Number of uses before removal (if OnUseCount)")]
        public int maxUseCount = 1;

        [Header("Stacking")]
        [Tooltip("Can this buff stack?")]
        public bool stackable = false;

        [Tooltip("Maximum number of stacks")]
        public int maxStacks = 1;

        [Tooltip("Behavior when same buff is applied again")]
        public StackBehavior stackBehavior = StackBehavior.RefreshDuration;

        [Header("Stat Modifiers")]
        [Tooltip("Stat modifications applied by this buff")]
        public List<BuffStatModifier> statModifiers = new List<BuffStatModifier>();

        [Header("Status Effects")]
        [Tooltip("Status effects applied by this buff")]
        public StatusEffect statusEffects = StatusEffect.None;

        [Header("Tick Effect (DoT/HoT)")]
        [Tooltip("Does this buff have a periodic tick effect?")]
        public bool hasTick = false;

        [Tooltip("Time between ticks in seconds")]
        public float tickInterval = 1f;

        [Tooltip("Damage per tick (negative for healing)")]
        public float tickDamage = 0f;

        [Tooltip("Damage type for tick damage")]
        public DamageType tickDamageType = DamageType.None;

        [Header("Special")]
        [Tooltip("Can this buff be dispelled?")]
        public bool dispellable = true;

        [Tooltip("Priority for buff sorting (higher = displayed first)")]
        public int priority = 0;

        [Tooltip("Tags for categorization")]
        public List<string> tags = new List<string>();

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(buffId))
            {
                buffId = name;
            }

            if (maxStacks < 1) maxStacks = 1;
            if (tickInterval < 0.1f) tickInterval = 0.1f;
        }

        /// <summary>
        /// Check if this buff has a specific tag
        /// </summary>
        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }

        /// <summary>
        /// Get description with values filled in
        /// </summary>
        public string GetFormattedDescription(int stacks = 1)
        {
            string result = description;

            // Replace placeholders with actual values
            result = result.Replace("{duration}", duration.ToString("F1"));
            result = result.Replace("{stacks}", stacks.ToString());

            foreach (var mod in statModifiers)
            {
                string valueStr = mod.modifierType == Stat.ModifierType.Flat
                    ? (mod.value * stacks).ToString("F0")
                    : ((mod.value * stacks) * 100f).ToString("F0") + "%";

                result = result.Replace($"{{{mod.statId}}}", valueStr);
            }

            if (hasTick)
            {
                result = result.Replace("{tickDamage}", (tickDamage * stacks).ToString("F0"));
                result = result.Replace("{tickInterval}", tickInterval.ToString("F1"));
            }

            return result;
        }
    }
}
