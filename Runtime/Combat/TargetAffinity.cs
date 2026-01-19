namespace MirrorRPG.Combat
{
    /// <summary>
    /// Target affinity for combat effects
    /// Determines which entities can be affected
    /// </summary>
    public enum TargetAffinity
    {
        /// <summary>
        /// Only affects enemies
        /// </summary>
        [UnityEngine.InspectorName("적")]
        Enemy,

        /// <summary>
        /// Only affects allies (including self)
        /// </summary>
        [UnityEngine.InspectorName("아군")]
        Ally,

        /// <summary>
        /// Only affects self
        /// </summary>
        [UnityEngine.InspectorName("자신")]
        Self,

        /// <summary>
        /// Affects both enemies and allies
        /// </summary>
        [UnityEngine.InspectorName("전체")]
        All
    }

    /// <summary>
    /// Type of combat effect
    /// </summary>
    public enum CombatEffectType
    {
        /// <summary>
        /// Deals damage to target
        /// </summary>
        [UnityEngine.InspectorName("데미지")]
        Damage,

        /// <summary>
        /// Heals target
        /// </summary>
        [UnityEngine.InspectorName("힐")]
        Heal,

        /// <summary>
        /// Only applies buffs/debuffs (no damage/heal)
        /// </summary>
        [UnityEngine.InspectorName("버프만")]
        BuffOnly
    }
}
