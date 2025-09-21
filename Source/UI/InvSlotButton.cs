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
	public AInventoryScreen ParentInventory;
	public void Setup(GameState.ItemEntry entry, bool ignoreVisuals=false) {
		CurrentEntry = entry;
		Item = (entry==null) ? null : BaseItem.Get(entry.itemID);
		if (entry == null || ignoreVisuals) {
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
			if (entry.stackSize > 1) Quantity.Text = entry.stackSize.ToString();
			else Quantity.Text = "";
			// Show ammo if there's any (might want to show the ammo's icon or something)
			if (entry.ammoId.Length > 0) {
				var iconLine = "[img]res://Graphics/textures/icons/ammo_bullet.png[/img]";
				var ammoItem = BaseItem.Get(entry.ammoId) as AmmoItem;
				if (ammoItem != null && ammoItem.AmmoIcon != null) {
					iconLine = string.Format("[img]{0}[/img]", ammoItem.AmmoIcon.ResourcePath);
				}
				Ammo.Text = iconLine + entry.ammoQty.ToString();
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
	private void OnInventorySelected()
	{
		ParentInventory.ItemSelected(this);
	}
	public void SetCombine(bool v) {
		Icon.Visible = !v;
	}
}
