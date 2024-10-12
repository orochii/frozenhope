using Godot;
using System;

public partial class InvSlotButton : TextureButton
{
	[Export] TextureRect Icon;
	[Export] Label amount;
	GameState.ItemEntry CurrentEntry;
	public BaseItem Item = null;
	public void Setup(GameState.ItemEntry entry, bool setAmount=false, bool ignoreVisuals=false) {
		CurrentEntry = entry;
		if (entry == null) {
			Icon.Texture = null;
			Item = null;
		} else {
			Item = BaseItem.Get(entry.itemID);
			if (!ignoreVisuals) {
				Icon.Texture = Item.Icon;
				float sizeX = Math.Max(32, Item.SlotSize.X * 32);
				float sizeY = Math.Max(32, Item.SlotSize.Y * 32);
				Icon.Size = new Vector2(sizeX, sizeY);
			} else {
				Icon.Texture = null;
			}
		}
		amount.Text = "";
		if (setAmount) {
			if (entry.stackSize > 1) amount.Text = entry.stackSize.ToString();
		}
	}
}
