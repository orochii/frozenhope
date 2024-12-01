using Godot;
using System;

public partial class EvtStart : Node
{
    [Export] string Flag = "gameStart";
    [Export] Control ControlScheme;
    [Signal] public delegate void InputReceivedEventHandler();
    public override void _Ready()
    {
        ControlScheme.Visible = false;
        if (Main.Instance.State.GetSwitch(Flag)) return;
        Main.Instance.State.SetSwitch(Flag, true);
        Execute();
    }
    public override void _Process(double delta)
    {
        if (!ControlScheme.IsVisibleInTree()) return;
        if (Input.IsActionJustPressed("interact")) {
            EmitSignal(SignalName.InputReceived);
        }
    }
    public async void Execute() {
        // Stop game
		Main.Instance.Busy = true;
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Main.Instance.UI.SetUIMode((int)UiParent.EModes.MESSAGE);
        // Show controls popup.
        ControlScheme.Modulate = Colors.Transparent;
        ControlScheme.Visible = true;
        var tween1 = CreateTween().TweenProperty(ControlScheme, "modulate", Colors.White, 1f);
        await ToSignal(tween1, Tween.SignalName.Finished);
        await ToSignal(this, SignalName.InputReceived);
        var tween2 = CreateTween().TweenProperty(ControlScheme, "modulate", Colors.Transparent, 1f);
        await ToSignal(tween2, Tween.SignalName.Finished);
        ControlScheme.Visible = false;
        // Continue the game.
		Main.Instance.Busy = false;
        Main.Instance.UI.SetUIMode((int)UiParent.EModes.GAMEPLAY);
    }
}
