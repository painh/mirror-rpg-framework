using System;

namespace MirrorRPG.Buff
{
    /// <summary>
    /// Status effect flags for controlling entity behavior
    /// </summary>
    [Flags]
    public enum StatusEffect
    {
        None = 0,

        // Movement impairments
        Slow = 1 << 0,          // Reduced movement speed
        Root = 1 << 1,          // Cannot move

        // Action impairments
        Stun = 1 << 2,          // Cannot act at all
        Silence = 1 << 3,       // Cannot use skills
        Disarm = 1 << 4,        // Cannot use basic attacks

        // Defensive states
        Invincible = 1 << 5,    // Cannot take damage
        Immune = 1 << 6,        // Cannot receive debuffs

        // Special states
        Invisible = 1 << 7,     // Cannot be targeted
        Taunt = 1 << 8,         // Must attack taunter
        Fear = 1 << 9,          // Runs away from source
        Charm = 1 << 10,        // Attacks allies

        // Damage over time markers
        Burning = 1 << 11,
        Poisoned = 1 << 12,
        Bleeding = 1 << 13,

        // Healing over time markers
        Regenerating = 1 << 14,
    }

    /// <summary>
    /// Stack behavior when same buff is applied again
    /// </summary>
    public enum StackBehavior
    {
        /// <summary>
        /// Refresh duration, don't add stacks
        /// 지속시간만 갱신, 스택 유지
        /// </summary>
        [UnityEngine.InspectorName("지속시간 갱신")]
        RefreshDuration,

        /// <summary>
        /// Add to remaining duration
        /// 남은 지속시간에 추가
        /// </summary>
        [UnityEngine.InspectorName("지속시간 누적")]
        AddDuration,

        /// <summary>
        /// Add new stack (up to max), each with own duration
        /// 각 스택이 개별 지속시간 보유
        /// </summary>
        [UnityEngine.InspectorName("독립 스택")]
        Independent,

        /// <summary>
        /// Add stack and refresh all stack durations
        /// 스택 추가 및 전체 지속시간 갱신
        /// </summary>
        [UnityEngine.InspectorName("스택 추가 + 갱신")]
        StackAndRefresh
    }

    /// <summary>
    /// Condition for removing the buff
    /// </summary>
    [Flags]
    public enum RemoveCondition
    {
        /// <summary>
        /// Removed when duration expires
        /// </summary>
        Time = 1 << 0,

        /// <summary>
        /// Removed when entity takes damage
        /// </summary>
        OnHit = 1 << 1,

        /// <summary>
        /// Removed after N uses (for triggered effects)
        /// </summary>
        OnUseCount = 1 << 2,

        /// <summary>
        /// Only removed manually
        /// </summary>
        Manual = 1 << 3,

        /// <summary>
        /// Removed on death
        /// </summary>
        OnDeath = 1 << 4
    }
}
