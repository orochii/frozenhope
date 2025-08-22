using Godot;
using System;

public partial class Door : Area3D, Interactable
{
    [Export] public Label3D Interface;
    [Export] public float InteractAngle = 45f;
    [Export] public Camera3D DoorCamera;
    [Export] public int GoToScene;
    //Use GoToSceneAlt if you wish to input a target map's filepath directly
    [Export] public string GoToScenAlt;
    [Export] public Vector3 NewSceneXYZ;
    [Export] private bool MaintainRotation;
    [Export(PropertyHint.Range, "-360,360,5")]
    public Vector3 NewSceneRotate;
    private Database _data;
    private Player _playerCharacter;
    public bool Active
        { get; set; }
    public bool InterfaceVisible
        { get; set; }

    public override void _Ready()
    {
        Interface.Visible = false;
        _data = Database.Get();
    }

    public void _onPlayerEnter(Node3D body)
    {
        _playerCharacter = (Player)body;
        _playerCharacter.NearbyInteractables.Add(this);
    }

    public void _onPlayerLeft(Node3D body)
    {
        _playerCharacter.NearbyInteractables.Remove(this);
        Active = false;
        HideInterface();
    }

    public override void _Process(double delta)
    {
        if (Active)
        {
            var forward = _playerCharacter.Transform.Basis.Z;
            //Get relative position for maffs
            var selfPos = GlobalPosition;
            var selfPosRelative = selfPos - _playerCharacter.GlobalPosition;
            //Get the angle between player and item and compare it against InteractAngle
            float angle = selfPosRelative.Normalized().AngleTo(forward);
            if (angle < Mathf.DegToRad(InteractAngle))
            {
                ShowInterface();
            }
            else HideInterface();
        }
    }

    //Interface functions
    public Vector3 GetItemPosition(){
        return GlobalPosition;
    }

    public void ShowInterface() {
        if (!IsVisibleInTree()) return;
        Interface.Visible = true;
        InterfaceVisible = true;
    }

    public void HideInterface() {
        Interface.Visible = false;
        InterfaceVisible = false;
    }

    public void InteractItem() {
        if (!IsVisibleInTree()) return;
        //Check if the target transfers scene is within the available scenes array
        var length = _data.StartingScene.Length;
        if (GoToScene >= 0 && GoToScene < length)
        {
            // Stop game and freeze player
            GD.Print("Start Map Change");
            _playerCharacter.FreezeStatus();
            Main.Instance.Busy = true;
            //Move to specified scene
            var Empty = Vector3.Zero;
            if (NewSceneXYZ != Empty) Main.Instance.TransferVector = NewSceneXYZ;
            if (!MaintainRotation) Main.Instance.TransferRotate = NewSceneRotate;
            else Main.Instance.TransferRotate = _playerCharacter.GlobalRotationDegrees;
    
            if (GoToScenAlt == null) Main.Instance.ChangeMap(_data.StartingScene[GoToScene], true, MaintainRotation);
            else Main.Instance.ChangeMap(GoToScenAlt);
            
            //Unpause game
            GD.Print("End Map Change");
            Main.Instance.Busy = false;
        }
        else
        {
            GD.Print("Target map out of range.");
        }
    }

    //Override ToString
    public override string ToString(){
        return Name;
    }
}
