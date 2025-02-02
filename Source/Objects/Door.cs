using Godot;
using System;

public partial class Door : StaticBody3D, Interactable
{
    [Export] public Label3D Interface;
    [Export] public float InteractAngle = 45f;
    [Export] public Player Character;
    [Export] public int GoToScene;
    [Export] public Vector3 NewSceneXYZ;
    [Export] public Vector3 NewSceneRotate;
    public bool Active
        { get; set; }
    private Database Data;

    public override void _Ready() {
        Interface.Visible = false;
        Data = Database.Get();
    }


    public override void _Process(double delta) {
        if (Active) {
            var forward = Character.Transform.Basis.Z;
            //Get relative position for maffs
            var selfPos = GlobalPosition;
            var selfPosRelative = selfPos - Character.GlobalPosition;
            //Get the angle between player and item and compare it against InteractAngle
            float angle = selfPosRelative.Normalized().AngleTo(forward);
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

    public void InteractItem() {
        if (!IsVisibleInTree()) return;
        // Stop game
        GD.Print("Start Map Change");
        Main.Instance.Busy = true;
        //Move to specified scene
        Main.Instance.TransferVector = NewSceneXYZ;
        Main.Instance.TransferRotate = NewSceneRotate;
        Main.Instance.ChangeMap(Data.StartingScene[GoToScene]);

        //Unpause game
        GD.Print("End Map Change");
        Main.Instance.Busy = false;
    }

    //Override ToString
    public override string ToString(){
        return Name;
    }
}
