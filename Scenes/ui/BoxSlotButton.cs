using Godot;
using System;

public partial class BoxSlotButton : TextureButton
{
    [Export] Control boxContainer;
	[Export] TextureRect boxIcon;
	[Export] RichTextLabel boxAmmo;
	[Export] Label boxQuantity;
    public int boxIndex;
}
