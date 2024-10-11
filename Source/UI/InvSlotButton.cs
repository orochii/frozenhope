using Godot;
using System;

public partial class InvSlotButton : TextureButton
{
	[Export] TextureRect Icon;
	public void Setup() {
		Icon.Texture = null;
	}
}
