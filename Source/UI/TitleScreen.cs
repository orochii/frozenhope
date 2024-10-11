using Godot;
using System;

public partial class TitleScreen : Control
{
	[Export] Button[] buttonList;
    public override void _Ready()
    {
		UIUtils.SetupVBoxList(buttonList);
        buttonList[0].GrabFocus();
    }
	public void StartGame() {
		Main.Instance.StartGame();
	}
	public void LoadGame() {
		GD.Print("Not yet!");
	}
	public void EndGame() {
		GetTree().Quit();
	}
}
