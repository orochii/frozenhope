using Godot;
using System;

public partial class Inventory : Control
{
	[Export] GridContainer inventoryGrid;
	[Export] InvSlotButton invSlotTemplate;
	[Export] InfoColumn InfoColumn;
	[Export] Control CombineObj;
	[Export] TextureRect CombineIcon;
	[Export] RichTextLabel InstructionsLabel;
	InvSlotButton[] _spawnedSlots;
	private InvSlotButton currentCombineSlot;
	private InvSlotButton lastFocused;
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
			_spawnedSlots[i].Index = -1;
			_spawnedSlots[i].GridPosition = new Vector2I(i%size.X, i/size.X);
			_spawnedSlots[i].ParentInventory = this;
			inventoryGrid.AddChild(_spawnedSlots[i]);
		}
		UIUtils.SetupGridList(_spawnedSlots, size.X);
	}
	public void RefreshSlots() {
		var size = Main.Instance.State.InventorySize;
		// Clear all slots
		SetCombine(null);
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
				for (int y = 0; y < item.SlotSize.Y; y++) {
					bool ignoreVisuals = !(x==0 && y==0); // Only top-left corner
					var idxSlot = entry.posX + x + (entry.posY + y) * size.X;
					_spawnedSlots[idxSlot].Setup(entry, ignoreVisuals);
					_spawnedSlots[idxSlot].Index = idx;
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
			if (slot != lastFocused) {
				lastFocused = slot;
				AudioManager.PlaySystemSound("cursor");
				InfoColumn.Setup(slot.Item);
				// Reposition combine overlay thing
				CombineObj.GlobalPosition = slot.GlobalPosition;
			}
			// Get input for combine.
			if (Input.IsActionJustPressed("aim")) {
				if(currentCombineSlot == null) {
					SetCombine(slot);
					AudioManager.PlaySystemSound("decision");
				}
			}
		}
		// Inputs.
		if (Input.IsActionJustPressed("cancel")) {
			if (currentCombineSlot != null) {
				SetCombine(null);
				AudioManager.PlaySystemSound("cancel");
			}
			else {
				Main.Instance.UI.Gameplay.CloseMenu();
				AudioManager.PlaySystemSound("cancel");
			}
		}
	}
	public void SetCombine(InvSlotButton button) {
		if (currentCombineSlot != null) currentCombineSlot.SetCombine(false);
		currentCombineSlot = button;
		if (button == null || button.Item==null) {
			CombineObj.Visible = false;
			currentCombineSlot = null;
		}
		else {
			button.SetCombine(true);
			CombineObj.Visible = true;
			// Set icon
			CombineIcon.Texture = button.Item.Icon;
			// Resize container
			float sizeX = Math.Max(32, button.Item.SlotSize.X * 32);
			float sizeY = Math.Max(32, button.Item.SlotSize.Y * 32);
			// Set position
			CombineObj.GlobalPosition = button.GlobalPosition;
		}
		RefreshInstructions();
	}
	public InvSlotButton GetCombine() {
		return currentCombineSlot;
	}
	private void RefreshInstructions() {
		if (currentCombineSlot == null) {
			InstructionsLabel.Text = "[color=#fd8](Ok):[/color] Use/Equip [color=#fd8](Aim):[/color] Move/Combine [color=#fd8](Back):[/color] Close menu";
		}
		else {
			InstructionsLabel.Text = "[color=#fd8](Ok):[/color] Place/Combine [color=#fd8](Back):[/color] Cancel";
		}
	}
}
