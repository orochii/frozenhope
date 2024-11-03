using Godot;
using System;

public partial class TitleScreen : Control
{
	[Export] Button[] buttonList;
    public override void _Ready()
    {
		UIUtils.SetupVBoxList(buttonList);
    }
	public void Refresh() {
		buttonList[0].GrabFocus();
	}
    public void StartGame() {
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
		GD.Print("Not yet!");
	}
	public void EndGame() {
		GetTree().Quit();
	}
}
