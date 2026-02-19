using Godot;
using System;

public partial class InvSlotButton : TextureButton
{
	[Export] Control Container;
	[Export] TextureRect Icon;
	[Export] RichTextLabel Ammo;
	[Export] Label Quantity;
	GameState.ItemEntry CurrentEntry;
	public int Index;
	public Vector2I GridPosition;
	public BaseItem Item = null;
	public Inventory ParentInventory;
	public bool SubMenuProcess = false;

	/// <summary>
	/// Receives an entry of type ItemEntry and a bool
	/// It then loads the reference to the entry into Item and does work on it.
	/// </summary>
	/// <param name="entry"></param>
	/// <param name="ignoreVisuals"></param>
	public void Setup(GameState.ItemEntry entry, bool ignoreVisuals=false) {
		CurrentEntry = entry;
		Item = (CurrentEntry==null) ? null : BaseItem.Get(CurrentEntry.itemID);
		if (CurrentEntry == null || ignoreVisuals) {
			// Unset
			Icon.Texture = null;
			Container.Visible = false;
		} else {
			// Set icon
			Icon.Texture = Item.Icon;
			// Resize container
			float sizeX = Math.Max(32, Item.SlotSize.X * 32);
			float sizeY = Math.Max(32, Item.SlotSize.Y * 32);
			Container.Size = new Vector2(sizeX, sizeY);
			Container.Visible = true;
			// Show amount only if over 1
			var item = BaseItem.Get(CurrentEntry.itemID);
			if (item.MaxStack > 1) Quantity.Text = CurrentEntry.stackSize.ToString();
			else Quantity.Text = "";
			// Show ammo if there's any (might want to show the ammo's icon or something)
			if (CurrentEntry.ammoId.Length > 0) {
				var iconLine = "[img]res://Graphics/textures/icons/ammo_bullet.png[/img]";
				var ammoItem = BaseItem.Get(CurrentEntry.ammoId) as AmmoItem;
				if (ammoItem != null && ammoItem.AmmoIcon != null) {
					iconLine = string.Format("[img]{0}[/img]", ammoItem.AmmoIcon.ResourcePath);
				}
				Ammo.Text = iconLine + CurrentEntry.ammoQty.ToString();
			}
			else {
				Ammo.Text = "";
			}
		}
	}

    public override void _Ready()
    {
		base._Ready();
        Pressed += OnInventorySelected;
    }

	/// <summary>
	/// Signal invoced method:
	/// Open the submenu or finish submenu processing
	/// </summary>
	private void OnInventorySelected()
	{
		// Call SubMenu
		var subMenu = ParentInventory.SubMenu;
		var combine = ParentInventory.GetCombine();
		if (!subMenu.Combining && !subMenu.Moving && combine == null && Index != -1)
		{
			string context;
			if (Item is WeaponItem) context = "Equip";
			else context = "Use";
			
			ParentInventory.SubMenu.OpenSubMenu(Item, ParentInventory, this, context);
			return;
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

	/// <summary>
	/// Equip the selected item onto the player, changing the player visuals as well assigned all the stats.
	/// </summary>
	public void EquipItem()
	{
		Main.Instance.State.SetEquippedItem(Index);
		Player.Instance.RefreshEquippedModel();
		ParentInventory.RefreshSlots();
		AudioManager.PlaySystemSound("decision");
	}

	/// <summary>
	/// Use the item in the appropriate context, whether it is to heal the player or use the item on the
	/// envionrment around them.
	/// </summary>
	/// <returns> True if the item can interact with the environment. </returns>
	public bool UseItem()
	{
		AudioManager.PlaySystemSound("decision");
		//Check if use of item closes the menu to interact with the environment
		var interactable = Player.Instance.ClosestInteractable;
        if ((interactable is WorldScenery || interactable is Door) && interactable.CanInteract)
        {
            interactable.InteractItem(Item.DisplayName);
			return true;
        }
		return false;
	}

	/// <summary>
	/// Check if the player can heal and reduce healing item by 1 if yes.
	/// </summary>
	/// <returns>Whether healing was possible or not.</returns>
	public bool UseHealItem()
	{
		var item = Item as HealItem;
		var state = Main.Instance.State;
		if (state.GetHealth() < state.GetMaxHealth())
		{
			state.ChangeHealth(item.HealAmount);
			state.RemoveFromSlot(GridPosition, 1);
			ParentInventory.RefreshSlots();
			AudioManager.PlaySystemSound("decision");
			return true;
		}
		AudioManager.PlaySystemSound("cancel");
		return false;
	}

	public void CombineItem()
	{
		ParentInventory.SetCombine(this);
		AudioManager.PlaySystemSound("decision");
	}

	/// <summary>
	/// Check the item in the Slot and reveal any hidden information as well as allow the player to rotate and
	/// investigate the 3D model of the item in question.
	/// </summary>
	public void CheckItem()
	{
		Main.Instance.State.SetSwitch(Item.DisplayName, true);
		ParentInventory.RefreshSlots();
		ParentInventory.InfoColumn.SetupDescription(Item);
	}


	public void MoveItem()
	{
		ParentInventory.SetCombine(this);
		AudioManager.PlaySystemSound("decision");
	}

	/// <summary>
	/// Method that gets a boolean and then sets this slot's Visible to the opposite.
	/// This is done for code iintuitivity. If a slot's combine status is set to "true"
	/// the slot becomes invisible and vice versa. Orochii you'rea genius.
	/// </summary>
	/// <param name="v"></param>
	public void IsCombining(bool v) {
		Icon.Visible = !v;
		Ammo.Visible = !v;
		Quantity.Visible = !v;
	}
}
