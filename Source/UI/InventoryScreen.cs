using Godot;
using System;

public partial class InventoryScreen : AInventoryScreen
{
	[Export] InventoryGrid InventoryGrid;
	[Export] public InfoColumn InfoColumn;
	[Export] Control CombineObj;
	[Export] TextureRect CombineIcon;
	[Export] RichTextLabel InstructionsLabel;
	[Export] public SubMenu SubMenu;
	private InvSlotButton currentCombineSlot;
	private InvSlotButton lastFocused;
	public override void _Ready()
	{
		base._Ready();
		Visible = false;
	}
	public void Refresh() {
		InventoryGrid.RefreshGrid();
		InventoryGrid.RefreshSlots();
		InfoColumn.Setup(null);
		InventoryGrid.GrabFocus(0);
	}
	public override void RefreshSlots()
	{
		InventoryGrid.RefreshSlots();
	}
	public override void _Process(double delta)
	{
		if (!IsVisibleInTree()) return;
		// Refresh selected item
		var focused = GetViewport().GuiGetFocusOwner();
		if (focused is InvSlotButton)
		{
			var slot = focused as InvSlotButton;
			if (slot != lastFocused)
			{
				lastFocused = slot;
				AudioManager.PlaySystemSound("cursor");
				InfoColumn.Setup(slot.Item);
				// Reposition combine overlay thing
				CombineObj.GlobalPosition = slot.GlobalPosition;
			}
			// Get input for combine.
			if (Input.IsActionJustPressed(Main.Aim))
			{
				if (currentCombineSlot == null)
				{
					SetCombine(slot);
					AudioManager.PlaySystemSound("decision");
				}
				else
				{
					ItemSelected(slot);
				}
			}
		}
		// Inputs.
		if (Input.IsActionJustPressed("cancel"))
		{
			if (currentCombineSlot != null)
			{
				SetCombine(null);
				AudioManager.PlaySystemSound("cancel");
			}
			else
			{
				Main.Instance.UI.Gameplay.CloseMenu();
				AudioManager.PlaySystemSound("cancel");
			}
		}
	}
	public override void SetCombine(InvSlotButton button) {
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
	public override void ItemSelected(InvSlotButton invSlotButton)
	{
		var combine = GetCombine();
		// Use item
		if (combine != null)
		{
			// If target is empty.
			if (invSlotButton.Index == -1)
			{
				if (Main.Instance.State.MoveToSlot(combine.Index, invSlotButton.GridPosition))
				{
					RefreshSlots();
					AudioManager.PlaySystemSound("decision");
				}
				else
				{
					AudioManager.PlaySystemSound("cancel");
				}
			}
			else if (invSlotButton.Index == combine.Index)
			{
				RefreshSlots();
				AudioManager.PlaySystemSound("decision");
			}
			else if (Main.Instance.State.CombineSlots(invSlotButton.Index, combine.Index))
			{
				Player.Instance.RefreshEquippedModel();
				RefreshSlots();
				AudioManager.PlaySystemSound("decision");
			}
			else
			{
				// Play buzzer.
				AudioManager.PlaySystemSound("cancel");
			}
		}
		// Use item
		else
		{
			//ParentInventory.SubMenu.MakeVisible(Item, Index, ParentInventory);
			//await ToSignal(ParentInventory.SubMenu, "sub_menu_closed");
			switch (invSlotButton.Item)
			{
				case WeaponItem:
					Main.Instance.State.SetEquippedItem(invSlotButton.Index);
					Player.Instance.RefreshEquippedModel();
					RefreshSlots();
					AudioManager.PlaySystemSound("decision");
					//ParentInventory.SubMenu.Visible = false;
					break;
				case UseableItem:
					//Temporary way to change an items fake description to real description
					Main.Instance.State.SetSwitch(invSlotButton.Item.DisplayName, true);
					RefreshSlots();
					InfoColumn.SetupDescription(invSlotButton.Item);
					AudioManager.PlaySystemSound("decision");
					//Check if use of item closes the menu to interact with the environment
					var interactable = Player.Instance.CloestInteractable;
					if ((interactable is WorldScenery || interactable is Door) && interactable.CanInteract)
					{
						interactable.InteractItem(invSlotButton.Item.DisplayName);
						Main.Instance.UI.Gameplay.CloseMenu();
					}
					break;
			}
		}
		InfoColumn.Setup(invSlotButton.Item);
	}
}
