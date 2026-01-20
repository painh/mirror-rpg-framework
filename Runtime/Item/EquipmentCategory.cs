namespace MirrorRPG.Item
{
    /// <summary>
    /// Equipment category for items (determines which slot types can equip this item)
    /// </summary>
    public enum EquipmentCategory
    {
        None = 0,
        Hand,       // Can be equipped in RightHand or LeftHand
        Helmet,
        Armor,
        Gloves,
        Boots,
        Ring,       // Can be equipped in Ring1, Ring2
        Necklace,
    }
}
