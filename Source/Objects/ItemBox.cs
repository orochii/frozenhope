using Godot;
using System;

public partial class ItemBox : Area3D, Interactable
{
    [Export] public AnimationPlayer BoxAnimation;
    [Export] public float InteractAngle = 45.0f;
    private Player _playerCharacter;
    public bool Active
    { get; set; }
    public bool CanInteract
        { get; set; }
    public bool InterfaceVisible
        { get; set; }

    public override void _Ready()
    {
        base._Ready();
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
        else HideInterface();
    }

    //Interface Functions
    public void _on_player_entered(Node3D body)
    {
        GD.Print("Enter!");
        _playerCharacter = (Player)body;
        _playerCharacter.NearbyInteractables.Add(this);
    }

    public void _on_player_left(Node3D body)
    {
        _playerCharacter.NearbyInteractables.Remove(this);
        Active = false;
        HideInterface();
    }

    public Vector3 GetItemPosition()
    {
        return GlobalPosition;
    }

    public void ShowInterface()
    {
        if (!IsVisibleInTree()) return;
        InterfaceVisible = true;
        CanInteract = true;
    }

    public void HideInterface()
    {
        InterfaceVisible = false;
        CanInteract = false;
    }
    public async void InteractItem(string item = "empty")
    {
        //Stop processing if AnimationPlayer is empty
        if (BoxAnimation == null)
        {
            GD.Print("No Animation Assigned to Box");
            return;
        }

        //Pause the game
        Main.Instance.Busy = true;
        //Animate Box!
        BoxAnimation.Play("Lid_Open");
        await ToSignal(BoxAnimation, AnimationPlayer.SignalName.AnimationFinished);
        Main.Instance.UI.Gameplay.OpenMenu();
        await ToSignal(Main.Instance.UI.Gameplay, "menu_closed");
        BoxAnimation.Play("Lid_Close");
        await ToSignal(BoxAnimation, AnimationPlayer.SignalName.AnimationFinished);
        //Unpause the game
        Main.Instance.Busy = false;
    }
}
