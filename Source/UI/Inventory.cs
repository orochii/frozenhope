using Godot;
using System;

public partial class Inventory : Control
{
	[Export] GridContainer inventoryGrid;
	[Export] InvSlotButton invSlotTemplate;
	[Export] InfoColumn InfoColumn;
	InvSlotButton[] _spawnedSlots;
	public override void _Ready()
	{
		Visible = false;
		invSlotTemplate.Visible = false;
	}
	public void Refresh() {
		RefreshGrid();
		RefreshSlots();
		InfoColumn.Setup(null);
		_spawnedSlots[0].GrabFocus();
	}
	public void RefreshGrid() {
		var size = Main.Instance.State.InventorySize;
		inventoryGrid.Columns = size.X;
		if (_spawnedSlots != null) foreach(var s in _spawnedSlots) s.QueueFree();
		_spawnedSlots = new InvSlotButton[size.X * size.Y];
		for (int i = 0; i < _spawnedSlots.Length; i++) {
			_spawnedSlots[i] = invSlotTemplate.Duplicate() as InvSlotButton;
			_spawnedSlots[i].Visible = true;
			_spawnedSlots[i].Setup(null);
			inventoryGrid.AddChild(_spawnedSlots[i]);
		}
		UIUtils.SetupGridList(_spawnedSlots, size.X);
	}
	public void RefreshSlots() {
		var size = Main.Instance.State.InventorySize;
		// Clear all slots
		foreach (var slot in _spawnedSlots) slot.Setup(null);
		// Set each inventory item to their location.
		var entries = Main.Instance.State.GetInventoryEntries();
		foreach (var entry in entries) {
			var item = BaseItem.Get(entry.itemID);
			for (int x = 0; x < item.SlotSize.X; x++) {
				for (int y = 0; y < item.SlotSize.Y; y++) {
					bool ignoreVisuals = !(x==0 && y==0); // Only top-left corner
					var idx = entry.posX + x + (entry.posY + y) * size.X;
					_spawnedSlots[idx].Setup(entry, ignoreVisuals);
				}
			}
		}
	}
	public override void _Process(double delta)
	{
		if (!IsVisibleInTree()) return;
		// Refresh selected item
		var focused = GetViewport().GuiGetFocusOwner();
		if (focused is InvSlotButton) {
			var slot = focused as InvSlotButton;
			InfoColumn.Setup(slot.Item);
		}
		// Inputs.
		if (Input.IsActionJustPressed("cancel")) {
			Main.Instance.UI.Gameplay.CloseMenu();
		}
	}
}
