using System;
using System.Collections.Generic;
using Godot;

public class GameState {
    [System.Serializable]
    public class ItemEntry {
        public string itemID;
        public int stackSize;
        public int posX;
        public int posY;
    }
    [System.Serializable]
    public class PersistentData {
        public string currentMap;
        public string markerId;
        public int health = 100;
        public Vector2I inventorySize = new Vector2I(2,4);
        public List<ItemEntry> inventory = new List<ItemEntry>();
        public List<bool> switches = new List<bool>();
        public int[,] GetInvMatrix() {
            // Create inventory matrix
            int[,] matrix = new int[inventorySize.X,inventorySize.Y];
            // Set all as empty first
            for (int x = 0; x < inventorySize.X; x++) {
                for (int y = 0; y < inventorySize.Y; y++) {
                    matrix[x,y] = -1;
                }
            }
            // Now mark each item at its corresponding slot.
            for (int i = 0; i < inventory.Count; i++) {
                var entry = inventory[i];
                var data = BaseItem.Get(entry.itemID);
                int sizeX = Math.Max(data.SlotSize.X, 1);
                int sizeY = Math.Max(data.SlotSize.Y, 1);
                for (int x = entry.posX; x < entry.posX+sizeX; x++) {
                    for (int y = entry.posY; y < entry.posY+sizeY; y++) {
                        matrix[x,y] = i;
                    }
                }
            }
            return matrix;
        }
    }
    PersistentData persistentData;
    public GameState() {
        persistentData = new PersistentData();
    }
    #region Character status
    public int GetMaxHealth() {
        return 100;
    }
    public void ChangeHealth(int v) {
        persistentData.health = Math.Clamp(persistentData.health + v, 0, GetMaxHealth());
    }
    public int GetHealth() {
        return persistentData.health;
    }
    #endregion
    #region Inventory
    public bool IsSpaceAvailable(BaseItem item, Vector2I position) {
        var matrix = persistentData.GetInvMatrix();
        return IsThereEnoughSpace(matrix, position.X, position.Y, item.SlotSize.X, item.SlotSize.Y);
    }
    public bool AddItem(BaseItem item, int amount) {
        
        // First, look for existing stacks
        foreach (var entry in persistentData.inventory) {
            if (entry.itemID == item.ID) {
                int remainingSpace = item.MaxStack - entry.stackSize;
                entry.stackSize += amount;
                if (entry.stackSize > item.MaxStack) entry.stackSize = item.MaxStack;
                amount -= remainingSpace;
                if (amount < 0) amount = 0;
            }
        }
        if (amount <= 0) return true;
        // If we still need to place more items.
        while (amount > 0) {
            var matrix = persistentData.GetInvMatrix();
            // Find available space.
            var found = FindAvailableSpace(matrix, item.SlotSize.X, item.SlotSize.Y, out var position);
            if (found) {
                ItemEntry newEntry = new ItemEntry();
                newEntry.itemID = item.ID;
                newEntry.stackSize = Math.Min(amount, item.MaxStack);
                amount -= newEntry.stackSize;
                newEntry.posX = position.X;
                newEntry.posY = position.Y;
                persistentData.inventory.Add(newEntry);
            } else {
                return false;
            }
        }
        return true;
    }
    private bool FindAvailableSpace(int[,] matrix, int w, int h, out Vector2I position) {
        for (int x = 0; x < matrix.GetLength(0); x++) {
            for (int y = 0; y < matrix.GetLength(1); y++) {
                // We found an empty slot, however...
                if (matrix[x,y] == -1) {
                    bool validSpace = IsThereEnoughSpace(matrix, x,y, w,h);
                    if (validSpace) {
                        position = new Vector2I(x,y);
                        return true;
                    }
                }
            }
        }
        position = new Vector2I();
        return false;
    }
    private bool IsThereEnoughSpace(int[,] matrix, int x, int y, int w, int h) {
        // Check if there's enough space around
        for (int xx = 0; xx < w; xx++) {
            for (int yy = 0; yy < h; yy++) {
                if (matrix[x+xx,y+yy] != -1) {
                    return false;
                }
            }
        }
        return true;
    }
    public string ListAllItems() {
        string retval = "";
        foreach (var entry in persistentData.inventory) {
            var data = BaseItem.Get(entry.itemID);
            if (retval.Length > 0) retval += ", ";
            retval += data.DisplayName;
        }
        return retval;
    }
    public List<ItemEntry> GetInventoryEntries() {
        return persistentData.inventory;
    }
    public string MapName {
        get {
            return persistentData.currentMap;
        }
        set {
            persistentData.currentMap = value;
        }
    }
    #endregion
}