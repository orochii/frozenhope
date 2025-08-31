using Godot;
using System;
using System.Linq;

public partial class ChaseCamera3D : Camera3D
{
    [Export] private bool TrackPosition = false;
    [ExportSubgroup("Positional Tracking")]
    [Export] private bool Follow_X = false;
    [Export] private float X_offset = 0.0f;
    [Export] private bool Follow_Y = false;
    [Export] private float Y_offset = 0.0f;
    [Export] private bool Follow_Z = false;
    [Export] private float Z_offset = 0.0f;
    [ExportSubgroup("Rotation Tracking")]
    [Export] private bool TrackRotation = false;
    [ExportSubgroup("Face Player overwrite")]
    [Export] private bool FacePlayer = false;
    private Player _playerChar;
    //[MARK REMOVE]
    //Unused property
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
        if (TrackPosition && Current)
            ChasePlayer();
        if (TrackRotation)
            LookAtPlayer();
        if (FacePlayer)
            LookAt(_playerChar.GlobalPosition);
    }

    //Method that updates the camera's position according to the player's position based on a predefined offset
    private void ChasePlayer()
    {
        var CamPosition = GlobalPosition;
        
        if (Follow_X)
            CamPosition.X = _playerChar.GlobalPosition.X - X_offset;
        if (Follow_Y)
            CamPosition.Y = _playerChar.GlobalPosition.Y - Y_offset;
        if (Follow_Z)
            CamPosition.Z = _playerChar.GlobalPosition.Z - Z_offset;
        GlobalPosition = CamPosition;
    }

    //Rotate the camera towards the player around the Camera's Y axis
    private void LookAtPlayer()
    {
        var CamRotation = GlobalRotation;
        var CamPos = new Vector2(GlobalPosition.X, GlobalPosition.Z);
        var PlayerPos = new Vector2(_playerChar.GlobalPosition.X, _playerChar.GlobalPosition.Z);
        var Direction = CamPos - PlayerPos;
        CamRotation.Y = Mathf.Atan2(Direction.X, Direction.Y);
        GlobalRotation = CamRotation;
    }

    //Get the Player node for the camera via the PlayerNode global group
    private void FindPlayerForCamera()
    {
        _playerChar = (Player)GetTree().GetFirstNodeInGroup("PlayerNode");
        if (_playerChar != null)
            GD.Print(Name + " found PlayerNode");
        else
            GD.Print(Name + " couldn't find PlayerNode!");
    }

}
