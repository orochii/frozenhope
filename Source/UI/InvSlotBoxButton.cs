using Godot;
using System;

public partial class InvSlotBoxButton : InvSlotButton
{
	[Export] Control Container;
	[Export] TextureRect Icon;
	[Export] RichTextLabel Ammo;
	[Export] Label Quantity;
	GameState.ItemEntry CurrentEntry;

    public override void _Ready()
    {
		base._Ready();
        Pressed += OnInventorySelected;
    }

	/// <summary>
	/// Signal invoked method:
	/// Open the submenu or finish submenu processing
	/// </summary>
	private void OnInventorySelected()
	{
		// Call SubMenu
		var subMenu = GetSubMenu();
		//var subMenu = ParentInventory.SubMenu;
		GD.Print("SubMenu is: " + subMenu);
		InvSlotButton combine = null;
		if (ParentInventory != null) combine = ParentInventory.GetCombine();
		if (combine != null) GD.Print("combine is: " + combine);
		else GD.Print("Combine is null");
		if (!subMenu.Combining && !subMenu.Moving && combine == null && Index != -1)
		{
			string context = "Take";
			ParentInventory.SubMenu.OpenSubMenu(Item, ParentInventory, this, context);
			return;
		}
		if (Main.Instance.UI.GetMenuState() == UiParent.EMenus.BOX)
		{
			ParentInventory.boxSibling.FocusFirstSlot();
		}

		// Use item
		if (combine != null)
		{
			// If target is empty.
			if (Index == -1)
			{
				if (Main.Instance.State.MoveToSlot(combine.Index, GridPosition))
				{
					ParentInventory.RefreshSlots();
					ParentInventory.InfoColumn.Setup(Item);
					AudioManager.PlaySystemSound("decision");
				}
				else
				{
					AudioManager.PlaySystemSound("cancel");
				}
			}
			else if (Main.Instance.State.CombineSlots(Index, combine.Index))
			{
				Player.Instance.RefreshEquippedModel();
				ParentInventory.RefreshSlots();
				AudioManager.PlaySystemSound("decision");
			}
			else
			{
				// Play buzzer.
				AudioManager.PlaySystemSound("cancel");
			}
		}
		subMenu.DeactivateSubMenu();
		//subMenu.ResetCursor();
	}
}
