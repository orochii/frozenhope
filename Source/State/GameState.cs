using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Godot;

public class GameState {
    [System.Serializable]
    public class ItemEntry {
        public string itemID;
        public int stackSize;
        public int posX;
        public int posY;
        public string ammoId = "";
        public int ammoQty = 0;
    }
    [System.Serializable]
    public class PersistentData {
        public string currentMap;
        public string markerId;
        public Dictionary<string,bool> switches = new Dictionary<string,bool>();
        public int health = 100;
        public int level = 1;
        public int exp = 0;
        public int inventorySizeX = 2;
        public int inventorySizeY = 4;
        public int EquippedItem = -1;
        public List<ItemEntry> inventory = new List<ItemEntry>();
        public List<ItemEntry> boxInventory = new List<ItemEntry>(); //List of items inside the item box
        public int[,] GetInvMatrix()
        {
            // Create inventory matrix
            int[,] matrix = new int[inventorySizeX, inventorySizeY];
            // Set all as empty first
            for (int x = 0; x < inventorySizeX; x++)
            {
                for (int y = 0; y < inventorySizeY; y++)
                {
                    matrix[x, y] = -1;
                }
            }
            // Now mark each item at its corresponding slot.
            for (int i = 0; i < inventory.Count; i++)
            {
                var entry = inventory[i];
                var data = BaseItem.Get(entry.itemID);
                int sizeX = Math.Max(data.SlotSize.X, 1);
                int sizeY = Math.Max(data.SlotSize.Y, 1);
                for (int x = entry.posX; x < entry.posX + sizeX; x++)
                {
                    for (int y = entry.posY; y < entry.posY + sizeY; y++)
                    {
                        matrix[x, y] = i;
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
    #region Game
    public string MapName {
        get {
            return persistentData.currentMap;
        }
        set {
            persistentData.currentMap = value;
        }
    }
    public string MarkerId {
        get {
            return persistentData.markerId;
        }
        set {
            persistentData.markerId = value;
        }
    }
    public bool GetSwitch(string id) {
        if (persistentData.switches.TryGetValue(id, out var sw)) {
            return sw;
        }
        return false;
    }
    public void SetSwitch(string id, bool value) {
        persistentData.switches[id] = value;
    }
    #endregion

    #region Character status
    const int HEALTH_BASE = 100;
    const int HEALTH_INCREASE = 10;
    const float EXP_LVLGAIN_RATIO = 2.4f;
    const int EXP_PER_LEVEL = 100;
    const int VARIANCE_REDUCTION_LEVEL = 5;
    public void AddExp(int gain) {
        int reduction = (int)Math.Pow(persistentData.level, EXP_LVLGAIN_RATIO);
        int gainAdjusted = gain - reduction;
        if (gainAdjusted < 1) gainAdjusted = 1;
        persistentData.exp += gainAdjusted;
        while (persistentData.exp > NextLevelExp()) persistentData.level++;
    }
    public int NextLevelExp() {
        return persistentData.level * EXP_PER_LEVEL;
    }
    public int NextLevelRemainder() {
        return EXP_PER_LEVEL - (persistentData.exp % EXP_PER_LEVEL);
    }
    public int GetMaxHealth() {
        return HEALTH_BASE + (persistentData.level - 1) * HEALTH_INCREASE;
    }
    public int GetDamageVariance(int baseVariance) {
        int varianceDecrease = VARIANCE_REDUCTION_LEVEL * (persistentData.level - 1);
        int varianceTotal = Math.Max(baseVariance - varianceDecrease, 0);
        return varianceTotal;
    }
    public int CalculateDamage(int baseDmg, int baseVariance) {
        var variance = GetDamageVariance(baseVariance);
        var minDmgPerc = Math.Max(10, 100 - variance);
        var minDmg = (baseDmg * minDmgPerc) / 100;
        return GD.RandRange(minDmg, baseDmg);
    }
    public void ChangeHealth(int v) {
        persistentData.health = Math.Clamp(persistentData.health + v, 0, GetMaxHealth());
    }
    public int GetHealth() {
        return persistentData.health;
    }
    public float HealthPerc {
        get {
            return GetHealth() * 1f / GetMaxHealth();
        }
    }
    public void SetEquippedItem(int idx) {
        persistentData.EquippedItem = idx;
    }
    public BaseItem GetEquippedItem() {
        var entry = GetEquippedItemEntry();
        if (entry == null) return null;
        return BaseItem.Get(entry.itemID);
    }
    public ItemEntry GetEquippedItemEntry() {
        if (persistentData.EquippedItem < 0) return null;
        if (persistentData.EquippedItem >= persistentData.inventory.Count) return null;
        return persistentData.inventory[persistentData.EquippedItem];
    }
    #endregion

    #region Inventory
    public Vector2I InventorySize
    {
        get
        {
            return new Vector2I(persistentData.inventorySizeX, persistentData.inventorySizeY);
        }
        set
        {
            persistentData.inventorySizeX = value.X;
            persistentData.inventorySizeY = value.Y;
        }
    }
    public bool IsSpaceAvailable(BaseItem item, Vector2I position, int ignore=-2) {
        var matrix = persistentData.GetInvMatrix();
        return IsThereEnoughSpace(matrix, position.X, position.Y, item.SlotSize.X, item.SlotSize.Y, ignore);
    }
    public bool AddItem(ItemAddEntry item) {
        if (item==null) return false;
        if (item.Item==null) return false;
        if (item.AmmoInside != null)
            return AddItem(item.Item,item.Amount, item.AmmoInside.ID, item.AmmoQty);
        return AddItem(item.Item,item.Amount);
    }
    public bool AddItem(BaseItem item, int amount, string ammoId="", int ammo=0) {
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
                if (item is WeaponItem && ammo > 0) {
                    var weapon = item as WeaponItem;
                    if (weapon.IsCompatibleWithAmmo(ammoId)) {
                        newEntry.ammoId = ammoId;
                        newEntry.ammoQty = ammo;
                    }
                }
            } else {
                return false;
            }
        }
        return true;
    }
    public void RemoveItem(BaseItem item, int amount) {
        List<ItemEntry> _toDelete = new List<ItemEntry>();
        // Remove amounts from stacks that correspond.
        foreach (var entry in persistentData.inventory) {
            if (entry.itemID == item.ID) {
                amount -= entry.stackSize;
                entry.stackSize = -amount;
                // - Register stacks of 0 or under.
                if (entry.stackSize <= 0) _toDelete.Add(entry);
                if (amount <= 0) break;
            }
        }
        // Remove empty stacks.
        foreach (var e in _toDelete) persistentData.inventory.Remove(e);
    }
    public void RemoveFromSlot(Vector2I pos, int amount) {
        // Invalid positions.
        if (pos.X < 0 || pos.X >= persistentData.inventorySizeX) return;
        if (pos.Y < 0 || pos.Y >= persistentData.inventorySizeY) return;
        // Get matrix
        var matrix = persistentData.GetInvMatrix();
        // Cross-ref to slot
        int idx = matrix[pos.X, pos.Y];
        // Remove amount from that slot alone.
        if (idx < 0) return;
        persistentData.inventory[idx].stackSize -= amount;
        if (persistentData.inventory[idx].stackSize <= 0) persistentData.inventory.RemoveAt(idx);
    }
    private bool FindAvailableSpace(int[,] matrix, int w, int h, out Vector2I position) {
        for (int y = 0; y < matrix.GetLength(1); y++) {
            for (int x = 0; x < matrix.GetLength(0); x++) {
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
    private bool IsThereEnoughSpace(int[,] matrix, int x, int y, int w, int h, int ignore=-2) {
        // Check if there's enough space around
        for (int xx = 0; xx < w; xx++) {
            for (int yy = 0; yy < h; yy++) {
                if (matrix[x+xx,y+yy] != -1) {
                    if (ignore != matrix[x+xx,y+yy]) return false;
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
            retval += data.GetItemName();
        }
        return retval;
    }
    public List<ItemEntry> GetInventoryEntries() {
        return persistentData.inventory;
    }
    public bool MoveToSlot(int index, Vector2I position) {
        // Get item.
        var itemEntry = persistentData.inventory[index];
        var item = BaseItem.Get(itemEntry.itemID);
        // Check if there's space for it somewhere else
        if (IsSpaceAvailable(item, position, index)) {
            itemEntry.posX = position.X;
            itemEntry.posY = position.Y;
            return true;
        }
        // Return we failed :')
        return false;
    }
    public bool CombineSlots(int index1, int index2) {
        if (index1 == index2) return false;
        // Get items.
        var itemEntry1 = persistentData.inventory[index1];
        var item1 = BaseItem.Get(itemEntry1.itemID);
        var itemEntry2 = persistentData.inventory[index2];
        var item2 = BaseItem.Get(itemEntry2.itemID);
        // Step1: Try combine stacks.
        if (itemEntry1.itemID == itemEntry2.itemID) {
            var remainingSpace = item2.MaxStack - itemEntry2.stackSize;
            var moveStack = Math.Min(remainingSpace, itemEntry1.stackSize);
            RemoveFromSlot(new Vector2I(itemEntry1.posX, itemEntry1.posY), moveStack);
            itemEntry2.stackSize += moveStack;
            return true;
        }
        // Step2: Try reload (one must be a weapon, the other must be valid ammo).
        var wpn = item1 is WeaponItem ? (item1 as WeaponItem) : (item2 is WeaponItem ? item2 as WeaponItem : null);
        var wpE = wpn==item1 ? itemEntry1 : wpn==item2 ? itemEntry2 : null;
        var amm = item1 is AmmoItem ? (item1 as AmmoItem) : (item2 is AmmoItem ? item2 as AmmoItem : null);
        var amE = amm==item1 ? itemEntry1 : amm==item2 ? itemEntry2 : null;
        if (wpn != null && amm != null && wpn.IsCompatibleWithAmmo(amE.itemID)) {
            return ReloadWithAmmo(wpn,wpE,amm,amE);
        }
        // Step3: Try combining (check if can do). --not for now.
        // TODO.
        // Else: no can't do :(.
        return false;
    }
    private bool ReloadWithAmmo(WeaponItem wpn, ItemEntry wpE, AmmoItem amm, ItemEntry amE) {
        if (wpn.IsCompatibleWithAmmo(amE.itemID)) {
            if (wpE.ammoId != amE.itemID) {
                AddItem(amm, wpE.ammoQty);
                wpE.ammoId = "";
                wpE.ammoQty = 0;
            } 
            wpE.ammoId = amE.itemID;
            var remainingSpace = wpn.AmmoMax - wpE.ammoQty;
            var addAmmo = Math.Min(remainingSpace, wpE.ammoQty);
            if (addAmmo <= 0) return false;
            RemoveFromSlot(new Vector2I(amE.posX, amE.posY), addAmmo);
            wpE.ammoQty += addAmmo;
            return true;
        }
        return false;
    }
    #endregion
}