using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class WorldScenery : StaticBody3D, Interactable
{
    [Export] public Label3D Interface;
    [Export] public float InteractAngle = 45f; //Currently unused
    [Export] public Player Character;
    [Export(PropertyHint.MultilineText)] private string FlavorText;

    public bool Active
        { get; set;}

    public override void _Ready() {
        Interface.Visible = false;
    }

    public override void _Process(double delta) {
        if (Active) {
            GD.Print("It is active");
            var forward = Character.Transform.Basis.Z;
            var selfPos = GlobalPosition;
            var selfPosRelative = selfPos - Character.GlobalPosition;
            float angle = selfPosRelative.Normalized().AngleTo(forward);
            GD.Print("Angle is:", angle);
            if (angle < Mathf.DegToRad(InteractAngle)) {
                ShowInterface();
            } else HideInterface();
        }
    }

    public void ShowInterface() {
        if (!IsVisibleInTree()) return;
        Interface.Visible = true;
    }

    public void HideInterface() {
        Interface.Visible = false;
    }

    public async void InteractItem() {
        if (!IsVisibleInTree()) return;
        // Stop game
        Main.Instance.Busy = true;
        //Assign text to a local string
        string str = FlavorText;
        await Main.Instance.UI.Message.SetText(str, false);
        // Call this on end of message, this just returns the UI mode back to whatever it was (usually gameplay).
        // Needed certain things from messages to stay, like the bars up/down for cool "in-level" cutscenes :vaccabayt:
        Main.Instance.UI.Message.EndMessage();
        // Unpause game
        Main.Instance.Busy = false;
    }
}
