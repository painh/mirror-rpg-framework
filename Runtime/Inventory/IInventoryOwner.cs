namespace MirrorRPG.Inventory
{
    /// <summary>
    /// Interface for entities that own an inventory
    /// </summary>
    public interface IInventoryOwner
    {
        /// <summary>
        /// The grid-based inventory container (Diablo-style)
        /// </summary>
        GridInventoryContainer GridInventory { get; }
    }
}
