using Godot;
using System;

public partial class InvSlotButton : TextureButton
{
	[Export] Control Container;
	[Export] TextureRect Icon;
	[Export] Label amount;
	GameState.ItemEntry CurrentEntry;
	public BaseItem Item = null;
	public void Setup(GameState.ItemEntry entry, bool ignoreVisuals=false) {
		CurrentEntry = entry;
		Item = (entry==null) ? null : BaseItem.Get(entry.itemID);
		if (entry == null || ignoreVisuals) {
			Icon.Texture = null;
			Container.Visible = false;
		} else {
			Icon.Texture = Item.Icon;
			float sizeX = Math.Max(32, Item.SlotSize.X * 32);
			float sizeY = Math.Max(32, Item.SlotSize.Y * 32);
			Container.Size = new Vector2(sizeX, sizeY);
			Container.Visible = true;
			// if (entry.stackSize > 1)
			amount.Text = entry.stackSize.ToString();
		}
	}
}
