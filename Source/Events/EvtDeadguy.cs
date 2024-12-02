using Godot;
using System;

public partial class EvtDeadguy : Area3D
{
	[Export] string Flag;
	[Export] Node3D PlayerLocation;
	[Export] Camera3D Camera;
	[Export] AnimationPlayer Animation;
	[Export] Enemy Wendigo;
	[Export] Node3D WendigoProp;
	[Export] Control EndMessage;
	[Export] Camera3D FinalCamera;
	[Signal] public delegate void InputReceivedEventHandler();
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
	public override void _Process(double delta)
    {
        if (!EndMessage.IsVisibleInTree()) return;
        if (Input.IsActionJustPressed("interact")) {
            EmitSignal(SignalName.InputReceived);
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
		// Play animation.
		Wendigo.Visible = false;
		WendigoProp.Visible = true;
		Animation.Play("sequence");
		await ToSignal(Animation, AnimationPlayer.SignalName.AnimationFinished);
		Wendigo.Visible = true;
		WendigoProp.Visible = false;
		// Return to old camera, close message.
		previousCamera.Current = true;
		await Main.Instance.UI.Message.SetBars(false);
		Main.Instance.UI.Message.EndMessage();
		// Continue the game.
		Main.Instance.Busy = false;
	}
	public void OnEnemyDeath(Node3D enemy) {
		ExecuteOnDeath();
	}
	public async void ExecuteOnDeath() {
		// Stop game again
		Main.Instance.Busy = true;
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Main.Instance.UI.SetUIMode((int)UiParent.EModes.MESSAGE);
		// Show bars
		await Main.Instance.UI.Message.SetBars(true);
		await Main.Instance.UI.Message.SetText("What is this monster?", false);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await Main.Instance.UI.Message.SetText("Thankfully, that body over there surely doesn't look like my brother but--", false);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await Main.Instance.UI.Message.SetText("I better find him, hopefully he's okay.", false);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		// Camera change to where the tracks are
		FinalCamera.Current = true;
		await Main.Instance.UI.Message.SetText("Hmm... I think I can see some vehicle tracks going through that opening in the fence.", false);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await Main.Instance.UI.Message.SetText("Maybe following those I'll find my brother, or someone that can give me some answers.", false);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		// Hide bars, end message
		await Main.Instance.UI.Message.SetBars(false);
		Main.Instance.UI.Message.EndMessage();
		// Show temporary game end here.
        EndMessage.Modulate = Colors.Transparent;
        EndMessage.Visible = true;
        var tween1 = CreateTween().TweenProperty(EndMessage, "modulate", Colors.White, 1f);
        await ToSignal(tween1, Tween.SignalName.Finished);
        await ToSignal(this, SignalName.InputReceived);
        var tween2 = CreateTween().TweenProperty(EndMessage, "modulate", Colors.Transparent, 1f);
        await ToSignal(tween2, Tween.SignalName.Finished);
        EndMessage.Visible = false;
		// Game end! :'D back to title.
		Main.Instance.Busy = false;
		Main.Instance.LoadIntroMap();
	}
}
