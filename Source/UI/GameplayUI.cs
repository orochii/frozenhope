using Godot;
using System;

public partial class GameplayUI : Control
{
	[Export] AnimationPlayer OverlayEffect;
	[Export] Inventory Inventory;
	public void Refresh() {
		// Run when UI mode is set to this.
		OverlayEffect.Play("RESET");
	}
	public void OpenMenu() {
		OverlayEffect.Play("showMenu");
		Inventory.Visible = true;
		Inventory.Refresh();
		GetTree().Paused = true;
	}
	public void CloseMenu() {
		OverlayEffect.Play("hideMenu");
		Inventory.Visible = false;
		GetTree().Paused = false;
	}

    public override void _Process(double delta)
    {
        if (!IsVisibleInTree()) return;
		// Open menu.
		if (Input.IsActionJustPressed("menu")) {
			if (!Inventory.Visible) OpenMenu();
		}
    }
}
