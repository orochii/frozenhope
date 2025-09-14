using Godot;
using System;
using System.Diagnostics.Tracing;

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
    [Export(PropertyHint.Range, "-360,360,5")] public Vector3 NewSceneRotate;
    [Export] private bool DoorLock = false;
    [Export(PropertyHint.MultilineText)] private string FlavorText;
    private Database _data;
    private Player _playerCharacter;
    private Camera3D _stashedCamera;
    private Vector3 _cameraRotationReset;
    public bool Active
    { get; set; }
    public bool InterfaceVisible
    { get; set; }
    //#Signals
    [Signal]
    public delegate void InteractedEventHandler();

    public override void _Ready()
    {
        base._Ready();
        Interface.Visible = false;
        if (DoorCamera != null) _cameraRotationReset = DoorCamera.Rotation;
        _data = Database.Get();
    }

    public void _on_player_entered(Node3D body)
    {
        _playerCharacter = (Player)body;
        _playerCharacter.NearbyInteractables.Add(this);
    }

    public void _on_player_left(Node3D body)
    {
        _playerCharacter.NearbyInteractables.Remove(this);
        Active = false;
        HideInterface();
    }

    public override void _Process(double delta)
    {
        float fDelta = (float)delta;
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
        if (Input.IsActionJustPressed("interact") && Main.Instance.Busy && Active)
        {
            GD.Print("Door Interact Signal Emited");
            EmitSignal(SignalName.Interacted);
        }
        if (DoorCamera != null && DoorCamera.Current == true) MoveCamera(fDelta);
    }

    //#Interface functions
    public Vector3 GetItemPosition()
    {
        return GlobalPosition;
    }

    public void ShowInterface()
    {
        if (!IsVisibleInTree()) return;
        Interface.Visible = true;
        InterfaceVisible = true;
    }

    public void HideInterface()
    {
        Interface.Visible = false;
        InterfaceVisible = false;
    }

    public async void InteractItem()
    {
        if (!IsVisibleInTree()) return;
        //Check if the door is locked, if yes, allow watching through the window if it has a camera assigned
        if (DoorLock)
        {
            //Pause the game
            Main.Instance.Busy = true;
            //Assign text to a local string
            await Main.Instance.UI.Message.SetBars(true);
            //Check if a Camera is attached to the door, if not, proceed to display text.
            if (DoorCamera != null)
            {
                _stashedCamera = GetViewport().GetCamera3D();
                _playerCharacter.Visible = false;
                DoorCamera.Current = true;
                //Wait for button input (I know, this is terrible lmao)
                await ToSignal(this, SignalName.Interacted);
                //Reset to StashedCamera if necessary
                DoorCamera.Current = false;
                _playerCharacter.Visible = true;
                _stashedCamera.Current = true;
                DoorCamera.Rotation = _cameraRotationReset;
            }
            //Set Text to display
            string str = FlavorText;
            await Main.Instance.UI.Message.SetText(str, false);
            // Call this on end of message, this just returns the UI mode back to whatever it was (usually gameplay).
            // Needed certain things from messages to stay, like the bars up/down for cool "in-level" cutscenes :vaccabayt:
            Main.Instance.UI.Message.EndMessage();
            // Unpause game
            Main.Instance.Busy = false;
            return;
        }

        //Check if the target transfers scene is within the available scenes array
        var length = _data.StartingScene.Length;
        if (GoToScene >= 0 && GoToScene < length)
        {
            // Stop game and freeze player
            GD.Print("Start Map Change");
            _playerCharacter.FreezeStatus();
            Main.Instance.Busy = true;
            //Move to specified scene
            var TransferRotation = Vector3.Zero;
            //Assign Rotatation based on the MaintainRotation boolean
            TransferRotation = MaintainRotation ? _playerCharacter.GlobalRotationDegrees : NewSceneRotate;
            //Move to map index if GoToScenAlt string is empty
            if (GoToScenAlt == null)
                Main.Instance.ChangeMap(_data.StartingScene[GoToScene], NewSceneXYZ, TransferRotation);
            else
                Main.Instance.ChangeMap(GoToScenAlt, NewSceneXYZ, TransferRotation);

            //Unpause game
            GD.Print("End Map Change");
            Main.Instance.Busy = false;
        }
        else
        {
            GD.Print("Target map out of range.");
        }
    }

    //#Custom Functions
    public void MoveCamera(float fDelta)
    {
        GD.Print("I was called!");
        var move = Input.GetVector(Main.MoveLeft, Main.MoveRight, Main.MoveUp, Main.MoveDown);
        var NewRotation = DoorCamera.Rotation;
        NewRotation.Y = Mathf.Clamp(NewRotation.Y + -move.X * fDelta, (float)-Math.PI/8, (float)Math.PI/8);
        NewRotation.X = Mathf.Clamp(NewRotation.X + -move.Y * fDelta, (float)-Math.PI/8, (float)Math.PI/8);
        DoorCamera.Rotation = NewRotation;
    }

    //#Override ToString
    public override string ToString()
    {
        return Name;
    }
}
