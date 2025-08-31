using Godot;
using System;

public partial class ChaseCamera3D : Camera3D
{
    [Export] private bool Tracking = false;
    [Export] private bool Follow_X = false;
    [Export] private float X_offset = 0.0f;
    [Export] private bool Follow_Y = false;
    [Export] private float Y_offset = 0.0f;
    [Export] private bool Follow_Z = false;
    [Export] private float Z_offset = 0.0f;
    private Player _playerChar;
    public Player PlayerChar
    {
        get { return _playerChar; }
        set { _playerChar = value; }
    }

    public override void _Ready()
    {
        FindPlayerForCamera();
    }

    public override void _Process(double delta)
    {
        if (Tracking && this.Current)
        {
            ChasePlayer();
        }
    }

    //Method that updates the camera's position according to the player's position based on a predefined offset
    private void ChasePlayer()
    {
        var CamPosition = this.GlobalPosition;
        if (Follow_X)
            CamPosition.X = _playerChar.GlobalPosition.X - X_offset;
        if (Follow_Y)
            CamPosition.Y = _playerChar.GlobalPosition.Y - Y_offset;
        if (Follow_Z)
            CamPosition.Z = _playerChar.GlobalPosition.Z - Z_offset;
        this.GlobalPosition = CamPosition;
    }

    //Search for the player node amongst the children of the parent node 2 grades above.
    //Important, always assign the camera node as a nephew Node of the Player node.
    private void FindPlayerForCamera()
    {
        var ParentScene = this.GetParent().GetParent();
        _playerChar = ParentScene.FindChild("Player") as Player;
        if (_playerChar != null)
            GD.Print("Found Playder node for Camera");
    }

}
