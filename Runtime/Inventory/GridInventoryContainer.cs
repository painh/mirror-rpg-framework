using System;
using System.Collections.Generic;
using MirrorRPG.Item;

namespace MirrorRPG.Inventory
{
    /// <summary>
    /// Placed item in the grid inventory
    /// </summary>
    public struct GridPlacement
    {
        public ItemInstance Item;
        public int GridX;
        public int GridY;
        public int Width;
        public int Height;

        public GridPlacement(ItemInstance item, int x, int y, int width, int height)
        {
            Item = item;
            GridX = x;
            GridY = y;
            Width = width;
            Height = height;
        }

        public bool IsEmpty => Item == null;

        /// <summary>
        /// Check if a cell is within this placement
        /// </summary>
        public bool ContainsCell(int x, int y)
        {
            return x >= GridX && x < GridX + Width &&
                   y >= GridY && y < GridY + Height;
        }
    }

    /// <summary>
    /// 2D grid-based inventory container (Diablo-style)
    /// Items can occupy multiple cells based on their size
    /// </summary>
    public class GridInventoryContainer
    {
        private readonly int gridWidth;
        private readonly int gridHeight;

        // Grid cells - stores placement ID (-1 if empty)
        private readonly int[,] grid;

        // All placed items
        private readonly Dictionary<int, GridPlacement> placements;

        // Next placement ID
        private int nextPlacementId = 0;

        /// <summary>
        /// Grid width in cells
        /// </summary>
        public int GridWidth => gridWidth;

        /// <summary>
        /// Grid height in cells
        /// </summary>
        public int GridHeight => gridHeight;

        /// <summary>
        /// Total number of cells
        /// </summary>
        public int TotalCells => gridWidth * gridHeight;

        /// <summary>
        /// Number of occupied cells
        /// </summary>
        public int OccupiedCells
        {
            get
            {
                int count = 0;
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (grid[x, y] >= 0) count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Number of items in the inventory
        /// </summary>
        public int ItemCount => placements.Count;

        /// <summary>
        /// Is the inventory completely full?
        /// </summary>
        public bool IsFull => OccupiedCells >= TotalCells;

        /// <summary>
        /// Is the inventory empty?
        /// </summary>
        public bool IsEmpty => placements.Count == 0;

        #region Events

        /// <summary>
        /// Fired when a placement changes (item added, removed, or moved)
        /// </summary>
        public event Action<GridPlacement> OnPlacementChanged;

        /// <summary>
        /// Fired when an item is added
        /// </summary>
        public event Action<ItemInstance, int, int> OnItemAdded;

        /// <summary>
        /// Fired when an item is removed
        /// </summary>
        public event Action<ItemInstance, int, int> OnItemRemoved;

        /// <summary>
        /// Fired when an item is moved
        /// </summary>
        public event Action<ItemInstance, int, int, int, int> OnItemMoved;

        #endregion

        #region Constructor

        public GridInventoryContainer(int width, int height)
        {
            gridWidth = Math.Max(1, width);
            gridHeight = Math.Max(1, height);

            grid = new int[gridWidth, gridHeight];
            placements = new Dictionary<int, GridPlacement>();

            // Initialize grid with -1 (empty)
            ClearGrid();
        }

        #endregion

        #region Grid Operations

        /// <summary>
        /// Clear the entire grid
        /// </summary>
        private void ClearGrid()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    grid[x, y] = -1;
                }
            }
        }

        /// <summary>
        /// Check if a position is valid
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }

        /// <summary>
        /// Check if an item can fit at the given position
        /// </summary>
        public bool CanFit(int x, int y, int itemWidth, int itemHeight, int ignorePlacementId = -1)
        {
            // Check bounds
            if (x < 0 || y < 0) return false;
            if (x + itemWidth > gridWidth) return false;
            if (y + itemHeight > gridHeight) return false;

            // Check if all cells are free
            for (int ix = x; ix < x + itemWidth; ix++)
            {
                for (int iy = y; iy < y + itemHeight; iy++)
                {
                    int cellId = grid[ix, iy];
                    if (cellId >= 0 && cellId != ignorePlacementId)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Check if an item data can fit at the given position
        /// </summary>
        public bool CanFit(int x, int y, IItemData itemData, int ignorePlacementId = -1)
        {
            if (itemData == null) return false;
            return CanFit(x, y, itemData.GridWidth, itemData.GridHeight, ignorePlacementId);
        }

        /// <summary>
        /// Find first position where an item can fit
        /// </summary>
        public (int x, int y)? FindFitPosition(int itemWidth, int itemHeight)
        {
            // Scan from top-left to bottom-right
            for (int y = 0; y <= gridHeight - itemHeight; y++)
            {
                for (int x = 0; x <= gridWidth - itemWidth; x++)
                {
                    if (CanFit(x, y, itemWidth, itemHeight))
                    {
                        return (x, y);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find first position where an item data can fit
        /// </summary>
        public (int x, int y)? FindFitPosition(IItemData itemData)
        {
            if (itemData == null) return null;
            return FindFitPosition(itemData.GridWidth, itemData.GridHeight);
        }

        /// <summary>
        /// Get the placement ID at a cell
        /// </summary>
        public int GetPlacementIdAt(int x, int y)
        {
            if (!IsValidPosition(x, y)) return -1;
            return grid[x, y];
        }

        /// <summary>
        /// Get the placement at a cell
        /// </summary>
        public GridPlacement? GetPlacementAt(int x, int y)
        {
            int id = GetPlacementIdAt(x, y);
            if (id < 0) return null;
            if (!placements.TryGetValue(id, out var placement)) return null;
            return placement;
        }

        /// <summary>
        /// Get the item at a cell
        /// </summary>
        public ItemInstance GetItemAt(int x, int y)
        {
            var placement = GetPlacementAt(x, y);
            return placement?.Item;
        }

        #endregion

        #region Add Item

        /// <summary>
        /// Add item to a specific position
        /// </summary>
        public bool AddItem(ItemInstance item, int x, int y)
        {
            if (item == null) return false;

            int width = item.Data.GridWidth;
            int height = item.Data.GridHeight;

            if (!CanFit(x, y, width, height)) return false;

            // Create placement
            int placementId = nextPlacementId++;
            var placement = new GridPlacement(item, x, y, width, height);
            placements[placementId] = placement;

            // Mark cells
            for (int ix = x; ix < x + width; ix++)
            {
                for (int iy = y; iy < y + height; iy++)
                {
                    grid[ix, iy] = placementId;
                }
            }

            OnPlacementChanged?.Invoke(placement);
            OnItemAdded?.Invoke(item, x, y);

            return true;
        }

        /// <summary>
        /// Add item to first available position
        /// </summary>
        public bool AddItem(ItemInstance item)
        {
            if (item == null) return false;

            // Try to stack with existing items first
            if (item.IsStackable)
            {
                foreach (var kvp in placements)
                {
                    var existing = kvp.Value.Item;
                    if (existing.ItemId == item.ItemId && existing.CanStack)
                    {
                        int remaining = existing.AddQuantity(item.Quantity);
                        if (remaining == 0)
                        {
                            OnPlacementChanged?.Invoke(kvp.Value);
                            return true;
                        }
                        item.SetQuantity(remaining);
                    }
                }
            }

            // Find position
            var pos = FindFitPosition(item.Data);
            if (pos == null) return false;

            return AddItem(item, pos.Value.x, pos.Value.y);
        }

        /// <summary>
        /// Add item by data and quantity
        /// </summary>
        public int AddItem(IItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0) return 0;

            int remaining = quantity;
            int added = 0;

            // Try to stack with existing items
            if (itemData.IsStackable)
            {
                foreach (var kvp in placements)
                {
                    var existing = kvp.Value.Item;
                    if (existing.ItemId == itemData.ItemId && existing.CanStack)
                    {
                        int beforeAdd = existing.Quantity;
                        remaining = existing.AddQuantity(remaining);
                        int addedToSlot = existing.Quantity - beforeAdd;

                        if (addedToSlot > 0)
                        {
                            added += addedToSlot;
                            OnPlacementChanged?.Invoke(kvp.Value);
                        }

                        if (remaining == 0) return added;
                    }
                }
            }

            // Add new stacks for remaining quantity
            while (remaining > 0)
            {
                var pos = FindFitPosition(itemData);
                if (pos == null) break;

                int toAdd = Math.Min(remaining, itemData.MaxStackSize);
                var newItem = new ItemInstance(itemData, toAdd);

                if (AddItem(newItem, pos.Value.x, pos.Value.y))
                {
                    added += toAdd;
                    remaining -= toAdd;
                }
                else
                {
                    break;
                }
            }

            return added;
        }

        #endregion

        #region Remove Item

        /// <summary>
        /// Remove item at a specific cell
        /// </summary>
        public ItemInstance RemoveItemAt(int x, int y)
        {
            int placementId = GetPlacementIdAt(x, y);
            if (placementId < 0) return null;

            return RemoveByPlacementId(placementId);
        }

        /// <summary>
        /// Remove by placement ID
        /// </summary>
        private ItemInstance RemoveByPlacementId(int placementId)
        {
            if (!placements.TryGetValue(placementId, out var placement)) return null;

            // Clear cells
            for (int ix = placement.GridX; ix < placement.GridX + placement.Width; ix++)
            {
                for (int iy = placement.GridY; iy < placement.GridY + placement.Height; iy++)
                {
                    grid[ix, iy] = -1;
                }
            }

            placements.Remove(placementId);

            var emptyPlacement = new GridPlacement(null, placement.GridX, placement.GridY, placement.Width, placement.Height);
            OnPlacementChanged?.Invoke(emptyPlacement);
            OnItemRemoved?.Invoke(placement.Item, placement.GridX, placement.GridY);

            return placement.Item;
        }

        /// <summary>
        /// Remove item by ID and quantity
        /// </summary>
        public int RemoveItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return 0;

            int remaining = quantity;
            int removed = 0;

            var toRemove = new List<int>();

            foreach (var kvp in placements)
            {
                if (remaining <= 0) break;
                if (kvp.Value.Item.ItemId != itemId) continue;

                var item = kvp.Value.Item;
                int toRemoveQty = Math.Min(remaining, item.Quantity);
                int actualRemoved = item.RemoveQuantity(toRemoveQty);

                removed += actualRemoved;
                remaining -= actualRemoved;

                if (item.Quantity <= 0)
                {
                    toRemove.Add(kvp.Key);
                }
                else
                {
                    OnPlacementChanged?.Invoke(kvp.Value);
                }
            }

            // Remove empty placements
            foreach (var id in toRemove)
            {
                RemoveByPlacementId(id);
            }

            return removed;
        }

        #endregion

        #region Move Item

        /// <summary>
        /// Move item to a new position
        /// </summary>
        public bool MoveItem(int fromX, int fromY, int toX, int toY)
        {
            int placementId = GetPlacementIdAt(fromX, fromY);
            if (placementId < 0) return false;

            var placement = placements[placementId];

            // Check if can fit at new position (ignoring current placement)
            if (!CanFit(toX, toY, placement.Width, placement.Height, placementId))
            {
                return false;
            }

            // Clear old cells
            for (int ix = placement.GridX; ix < placement.GridX + placement.Width; ix++)
            {
                for (int iy = placement.GridY; iy < placement.GridY + placement.Height; iy++)
                {
                    grid[ix, iy] = -1;
                }
            }

            // Mark new cells
            for (int ix = toX; ix < toX + placement.Width; ix++)
            {
                for (int iy = toY; iy < toY + placement.Height; iy++)
                {
                    grid[ix, iy] = placementId;
                }
            }

            // Update placement
            var newPlacement = new GridPlacement(placement.Item, toX, toY, placement.Width, placement.Height);
            placements[placementId] = newPlacement;

            OnPlacementChanged?.Invoke(newPlacement);
            OnItemMoved?.Invoke(placement.Item, placement.GridX, placement.GridY, toX, toY);

            return true;
        }

        /// <summary>
        /// Swap two items
        /// </summary>
        public bool SwapItems(int x1, int y1, int x2, int y2)
        {
            int id1 = GetPlacementIdAt(x1, y1);
            int id2 = GetPlacementIdAt(x2, y2);

            if (id1 < 0 || id2 < 0) return false;
            if (id1 == id2) return false;

            var p1 = placements[id1];
            var p2 = placements[id2];

            // Check if they can swap positions
            // First, temporarily remove both
            for (int ix = p1.GridX; ix < p1.GridX + p1.Width; ix++)
            {
                for (int iy = p1.GridY; iy < p1.GridY + p1.Height; iy++)
                {
                    grid[ix, iy] = -1;
                }
            }
            for (int ix = p2.GridX; ix < p2.GridX + p2.Width; ix++)
            {
                for (int iy = p2.GridY; iy < p2.GridY + p2.Height; iy++)
                {
                    grid[ix, iy] = -1;
                }
            }

            // Check if item1 can fit at item2's position and vice versa
            bool canSwap = CanFit(p2.GridX, p2.GridY, p1.Width, p1.Height) &&
                           CanFit(p1.GridX, p1.GridY, p2.Width, p2.Height);

            if (!canSwap)
            {
                // Restore original positions
                for (int ix = p1.GridX; ix < p1.GridX + p1.Width; ix++)
                {
                    for (int iy = p1.GridY; iy < p1.GridY + p1.Height; iy++)
                    {
                        grid[ix, iy] = id1;
                    }
                }
                for (int ix = p2.GridX; ix < p2.GridX + p2.Width; ix++)
                {
                    for (int iy = p2.GridY; iy < p2.GridY + p2.Height; iy++)
                    {
                        grid[ix, iy] = id2;
                    }
                }
                return false;
            }

            // Perform swap
            var newP1 = new GridPlacement(p1.Item, p2.GridX, p2.GridY, p1.Width, p1.Height);
            var newP2 = new GridPlacement(p2.Item, p1.GridX, p1.GridY, p2.Width, p2.Height);

            // Mark new positions
            for (int ix = newP1.GridX; ix < newP1.GridX + newP1.Width; ix++)
            {
                for (int iy = newP1.GridY; iy < newP1.GridY + newP1.Height; iy++)
                {
                    grid[ix, iy] = id1;
                }
            }
            for (int ix = newP2.GridX; ix < newP2.GridX + newP2.Width; ix++)
            {
                for (int iy = newP2.GridY; iy < newP2.GridY + newP2.Height; iy++)
                {
                    grid[ix, iy] = id2;
                }
            }

            placements[id1] = newP1;
            placements[id2] = newP2;

            OnPlacementChanged?.Invoke(newP1);
            OnPlacementChanged?.Invoke(newP2);

            return true;
        }

        #endregion

        #region Query

        /// <summary>
        /// Check if inventory has item with quantity
        /// </summary>
        public bool HasItem(string itemId, int quantity = 1)
        {
            return GetItemCount(itemId) >= quantity;
        }

        /// <summary>
        /// Get total count of an item
        /// </summary>
        public int GetItemCount(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return 0;

            int count = 0;
            foreach (var kvp in placements)
            {
                if (kvp.Value.Item.ItemId == itemId)
                {
                    count += kvp.Value.Item.Quantity;
                }
            }
            return count;
        }

        /// <summary>
        /// Find first placement of an item
        /// </summary>
        public GridPlacement? FindItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            foreach (var kvp in placements)
            {
                if (kvp.Value.Item.ItemId == itemId)
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Get all placements
        /// </summary>
        public IEnumerable<GridPlacement> GetAllPlacements()
        {
            return placements.Values;
        }

        /// <summary>
        /// Can add item?
        /// </summary>
        public bool CanAddItem(IItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0) return false;

            int remaining = quantity;

            // Check stackable space
            if (itemData.IsStackable)
            {
                foreach (var kvp in placements)
                {
                    if (kvp.Value.Item.ItemId == itemData.ItemId && kvp.Value.Item.CanStack)
                    {
                        remaining -= kvp.Value.Item.StackSpace;
                        if (remaining <= 0) return true;
                    }
                }
            }

            // Check if we can place new stacks
            int stacksNeeded = (int)Math.Ceiling((double)remaining / itemData.MaxStackSize);
            int placeable = 0;

            // Clone grid for simulation
            var tempGrid = (int[,])grid.Clone();

            for (int i = 0; i < stacksNeeded; i++)
            {
                bool found = false;
                for (int y = 0; y <= gridHeight - itemData.GridHeight && !found; y++)
                {
                    for (int x = 0; x <= gridWidth - itemData.GridWidth && !found; x++)
                    {
                        if (CanFitInTempGrid(tempGrid, x, y, itemData.GridWidth, itemData.GridHeight))
                        {
                            // Mark as occupied
                            for (int ix = x; ix < x + itemData.GridWidth; ix++)
                            {
                                for (int iy = y; iy < y + itemData.GridHeight; iy++)
                                {
                                    tempGrid[ix, iy] = 99999; // Temporary marker
                                }
                            }
                            placeable++;
                            found = true;
                        }
                    }
                }
            }

            return placeable >= stacksNeeded;
        }

        private bool CanFitInTempGrid(int[,] tempGrid, int x, int y, int w, int h)
        {
            for (int ix = x; ix < x + w; ix++)
            {
                for (int iy = y; iy < y + h; iy++)
                {
                    if (tempGrid[ix, iy] >= 0) return false;
                }
            }
            return true;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Clear all items
        /// </summary>
        public void Clear()
        {
            var allPlacements = new List<GridPlacement>(placements.Values);

            placements.Clear();
            ClearGrid();

            foreach (var p in allPlacements)
            {
                OnItemRemoved?.Invoke(p.Item, p.GridX, p.GridY);
            }
        }

        /// <summary>
        /// Sort and compact the inventory
        /// </summary>
        public void Sort()
        {
            // Collect all items
            var items = new List<ItemInstance>();
            foreach (var kvp in placements)
            {
                items.Add(kvp.Value.Item);
            }

            // Sort by type, then rarity, then size
            items.Sort((a, b) =>
            {
                int typeCompare = a.Data.ItemType.CompareTo(b.Data.ItemType);
                if (typeCompare != 0) return typeCompare;

                int rarityCompare = b.Data.Rarity.CompareTo(a.Data.Rarity); // Descending
                if (rarityCompare != 0) return rarityCompare;

                // Larger items first
                int sizeCompare = (b.Data.GridWidth * b.Data.GridHeight).CompareTo(a.Data.GridWidth * a.Data.GridHeight);
                if (sizeCompare != 0) return sizeCompare;

                return string.Compare(a.Data.DisplayName, b.Data.DisplayName, StringComparison.Ordinal);
            });

            // Clear and re-add
            placements.Clear();
            ClearGrid();
            nextPlacementId = 0;

            foreach (var item in items)
            {
                AddItem(item);
            }
        }

        /// <summary>
        /// Get debug string representation of the grid
        /// </summary>
        public string GetGridDebugString()
        {
            var sb = new System.Text.StringBuilder();
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    int id = grid[x, y];
                    sb.Append(id >= 0 ? id.ToString("D2") : "..");
                    sb.Append(' ');
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        #endregion
    }
}
