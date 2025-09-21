using Godot;
using System;

public partial class InventoryGrid : GridContainer
{
    [Export] GridContainer inventoryGrid;
    [Export] InvSlotButton invSlotTemplate;
    [Export] AInventoryScreen ParentInventory;
    InvSlotButton[] _spawnedSlots;
    public override void _Ready()
    {
        base._Ready();
        invSlotTemplate.Visible = false;
    }

    internal void GrabFocus(int v)
    {
        _spawnedSlots[v].GrabFocus();
    }
    public void RefreshGrid()
    {
        var size = Main.Instance.State.InventorySize;
        inventoryGrid.Columns = size.X;
        if (_spawnedSlots != null) foreach (var s in _spawnedSlots) s.QueueFree();
        _spawnedSlots = new InvSlotButton[size.X * size.Y];
        for (int i = 0; i < _spawnedSlots.Length; i++)
        {
            _spawnedSlots[i] = invSlotTemplate.Duplicate() as InvSlotButton;
            _spawnedSlots[i].Visible = true;
            _spawnedSlots[i].Setup(null);
            _spawnedSlots[i].Index = -1;
            _spawnedSlots[i].GridPosition = new Vector2I(i % size.X, i / size.X);
            _spawnedSlots[i].ParentInventory = ParentInventory;
            inventoryGrid.AddChild(_spawnedSlots[i]);
        }
        UIUtils.SetupGridList(_spawnedSlots, size.X);
    }
    public void RefreshSlots()
    {
		var size = Main.Instance.State.InventorySize;
		// Clear all slots
		ParentInventory.SetCombine(null);
		foreach (var slot in _spawnedSlots) {
			slot.Index = -1;
			slot.Setup(null);
		}
		// Set each inventory item to their location.
		var entries = Main.Instance.State.GetInventoryEntries();
		for (int idx = 0; idx < entries.Count; idx++) {
			var entry = entries[idx];
			var item = BaseItem.Get(entry.itemID);
			for (int x = 0; x < item.SlotSize.X; x++) {
				for (int y = 0; y < item.SlotSize.Y; y++)
				{
					bool ignoreVisuals = !(x == 0 && y == 0); // Only top-left corner
					var idxSlot = entry.posX + x + (entry.posY + y) * size.X;
					_spawnedSlots[idxSlot].Setup(entry, ignoreVisuals);
					_spawnedSlots[idxSlot].Index = idx;
				}
			}
		}
	}
}
