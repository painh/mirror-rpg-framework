namespace MirrorRPG.Equipment
{
    /// <summary>
    /// Interface for entities that can equip items
    /// </summary>
    public interface IEquipmentOwner
    {
        /// <summary>
        /// The equipment container
        /// </summary>
        EquipmentContainer Equipment { get; }
    }
}
