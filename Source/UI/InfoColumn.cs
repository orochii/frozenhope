using Godot;
using System;

public partial class InfoColumn : VBoxContainer
{
	[Export] Node3D ModelParent;
	[Export] RichTextLabel Description;
	[Export(PropertyHint.MultilineText)] string DescriptionFormat = "[color=#ee9]{0}[/color]{1}";
	Node3D _spawnedModel;
	BaseItem _cachedItem;
	public void Setup(BaseItem item) {
		if (_cachedItem == item) return;
		_cachedItem = item;
		// Cleanup first
		if (_spawnedModel != null) {
			_spawnedModel.QueueFree();
			_spawnedModel = null;
			Description.Text = "";
		}
		// Set up visuals.
		if (item != null) {
			_spawnedModel = item.Model.Instantiate<Node3D>();
			ModelParent.AddChild(_spawnedModel);
			Description.Text = string.Format(DescriptionFormat, item.DisplayName, item.DisplayDescription);
		}
	}
}
