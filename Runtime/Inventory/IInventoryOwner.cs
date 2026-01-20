namespace MirrorRPG.Inventory
{
    /// <summary>
    /// Interface for entities that own an inventory
    /// </summary>
    public interface IInventoryOwner
    {
        /// <summary>
        /// The inventory container
        /// </summary>
        InventoryContainer Inventory { get; }
    }
}
