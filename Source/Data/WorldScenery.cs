using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class WorldScenery : StaticBody3D, Interactable
{
    [Export] public Label3D Interface;
    [Export] public float InteractAngle = 45f;
    [Export(PropertyHint.MultilineText)] private string FlavorText;

    public override void _Ready() {
        Interface.Visible = false;
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

    /*private bool ValidInteractAngle() {
		var forward = Transform.Basis.Z;
        //Get current Position of interactable item
        Vector3 selfPosition = GlobalPosition;
        //We check if the player is in front of the item
        float angle = selfPosition.Normalized().AngleTo(forward);
        if (angle < Mathf.DegToRad(InteractAngle)) {
            return true;
        }
        return false;
	}*/
}
