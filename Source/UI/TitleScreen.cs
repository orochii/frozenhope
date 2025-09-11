using Godot;
using System;

public partial class TitleScreen : Control
{
	[Export] Button[] buttonList;
	Control lastFocused;
    public override void _Ready()
    {
		base._Ready();
		UIUtils.SetupVBoxList(buttonList);
    }
    public override void _Process(double delta)
    {
		if (!IsVisibleInTree()) return;
        var focused = GetViewport().GuiGetFocusOwner();
		if (lastFocused != focused && IsOnList(focused)) {
			AudioManager.PlaySystemSound("cursor");
			lastFocused = focused;
		}
    }
	private bool IsOnList(Control f) {
		foreach (var b in buttonList) {
			if (b == f) return true;
		}
		return false;
	}
    public void Refresh() {
		buttonList[0].GrabFocus();
	}
    public void StartGame() {
		AudioManager.PlaySystemSound("startGame");
		// This after intro scene.
		//Main.Instance.StartGame();
		// Ask intro scene to start and run.
		// Upon end, intro scene will throw into start of game. :3
		var scene = Main.Instance.CurrentScene;
		if (scene is Cutscene) {
			var cutscene = scene as Cutscene;
			cutscene.Play("new_animation");
		}
		Main.Instance.UI.SetUIMode((int)UiParent.EModes.CUTSCENE);
	}
	public void LoadGame() {
		AudioManager.PlaySystemSound("startGame");
		GD.Print("Not yet!");
	}
	public void EndGame() {
		AudioManager.PlaySystemSound("decision");
		GetTree().Quit();
	}
}
