namespace MirrorRPG.Stat
{
    /// <summary>
    /// Modifier type determines how the value is applied
    /// Calculation order: (Base + Flat) * (1 + PercentAdd) * PercentMult
    /// </summary>
    public enum ModifierType
    {
        /// <summary>
        /// Added directly to base value
        /// Example: +20 Attack from sword
        /// </summary>
        Flat = 0,

        /// <summary>
        /// Added as percentage increase (stacks additively with other PercentAdd)
        /// Example: +30% Attack buff → value = 0.3
        /// </summary>
        PercentAdd = 1,

        /// <summary>
        /// Multiplied after all other calculations (stacks multiplicatively)
        /// Example: x0.8 debuff → value = 0.8
        /// </summary>
        PercentMult = 2
    }
}
