using Godot;
using System;

public partial class EvtDeadguy : Area3D
{
	[Export] string Flag;
	[Export] Node3D PlayerLocation;
	[Export] Camera3D Camera;
	[Export] AnimationPlayer Cutscene;
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }
    public void OnBodyEntered(Node3D body) {
		if (body is Player) {
			if (!Main.Instance.State.GetSwitch(Flag)) {
				Execute();
				Main.Instance.State.SetSwitch(Flag, true);
			}
		}
	}

	private async void Execute() {
		// Stop game
		Main.Instance.Busy = true;
		Camera3D previousCamera = GetViewport().GetCamera3D();
		// Relocate player for cutscene, set camera.
		Camera.Current = true;
		Player.Instance.GlobalPosition = PlayerLocation.GlobalPosition;
		Player.Instance.GlobalRotation = PlayerLocation.GlobalRotation;
		// Show the bars :O
		await Main.Instance.UI.Message.SetBars(true);
		await Main.Instance.UI.Message.SetText("Oh my god, this is horrible, what happened here?", false);
		// TODO: Show where the Wendy go.
		// Return to old camera, close message.
		previousCamera.Current = true;
		await Main.Instance.UI.Message.SetBars(false);
		Main.Instance.UI.Message.EndMessage();
		// Continue the game.
		Main.Instance.Busy = false;
	}
}
