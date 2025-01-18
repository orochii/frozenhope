using Godot;
using System;

public partial class Door : Node3D, Interactable
{
    [Export] public float InteractAngle = 45f;
    [Export] public Player Character;
    public bool Active
        { get; set;}
    private bool NearDoor = false;


    public override void _Process(double delta) {
        if (Active) {
            var forward = Character.Transform.Basis.Z;
            //Get relative position for maffs
            var selfPos = GlobalPosition;
            var selfPosRelative = selfPos - Character.GlobalPosition;
            //Get the angle between player and item and compare it against InteractAngle
            float angle = selfPosRelative.Normalized().AngleTo(forward);
            if (angle < Mathf.DegToRad(InteractAngle)) {
                NearDoor = true;
                GD.Print("Door on");
            } else {
                NearDoor = false;
                GD.Print("Door off");
            }
        }
    }

    //Not used in this node
    public void HideInterface() {
        throw new NotImplementedException();
    }
    //Not used in this Node
    public void ShowInterface() {
        throw new NotImplementedException();
    }

    public void InteractItem() {
        if (!IsVisibleInTree()) return;
        // Stop game
        Main.Instance.Busy = true;
        //Fade out the screen
        

        //Unpause game
        Main.Instance.Busy = false;
    }

    //Override ToString
    public override string ToString(){
        return Name;
    }



}
