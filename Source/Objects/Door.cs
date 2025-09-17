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
    [Export] private bool _maintainRotation;
    [Export(PropertyHint.Range, "-360,360,5")] public Vector3 NewSceneRotate;
    [Export] public bool DoorLock = false;
    [Export] public string[] Keys = new string[0];
    [Export(PropertyHint.MultilineText)] private string FlavorText;
    [Export(PropertyHint.MultilineText)] private string UnlockText;
    private Database _data;
    private Player _playerCharacter;
    private Camera3D _stashedCamera;
    private Vector3 _cameraRotationReset;
    private bool _rightKey;
    public bool Active
    { get; set; }
    public bool CanInteract
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
        if (Main.Instance.State.GetSwitch(Name + "Unlocked")) DoorLock = false;
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
        CanInteract = true;
    }

    public void HideInterface()
    {
        Interface.Visible = false;
        InterfaceVisible = false;
        CanInteract = false;
    }

    public async void InteractItem(string itemName = "empty")
    {
        if (!IsVisibleInTree()) return;
        //Pause the game
        Main.Instance.Busy = true;
        if (itemName != "empty") _rightKey = ItemChecker(itemName);
        //Check if the door is locked, if yes, allow watching through the window if it has a camera assigned
        if (DoorLock)
        {
            //Pause the game
            Main.Instance.Busy = true;
            //Set UI mode to cutscene mode!
            await Main.Instance.UI.Message.SetBars(true);

            //Unlock the door if the right key was used
            if (_rightKey)
            {
                //Assign text to a local string
                await Main.Instance.UI.Message.SetText(UnlockText, false);
                Main.Instance.State.SetSwitch(Name + "Unlocked", true);
                DoorLock = false;
                // Call this on end of message, this just returns the UI mode back to whatever it was (usually gameplay).
                // Needed certain things from messages to stay, like the bars up/down for cool "in-level" cutscenes :vaccabayt:
                Main.Instance.UI.Message.EndMessage();
                // Unpause game
                Main.Instance.Busy = false;
                return;
            }

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
            //Assign text to a local string
            string stri = FlavorText;
            if (itemName != "empty" && !_rightKey) stri = "I can't use this here.";
            await Main.Instance.UI.Message.SetText(stri, false);
            // Call this on end of message, this just returns the UI mode back to whatever it was (usually gameplay).
            // Needed certain things from messages to stay, like the bars up/down for cool "in-level" cutscenes :vaccabayt:
            Main.Instance.UI.Message.EndMessage();
            // Unpause game
            Main.Instance.Busy = false;
            return;
        }

        //In case wrong item is used even after door is unlocked
        if (itemName != "empty")
        {
            //Pause the game
            Main.Instance.Busy = true;
            //Set UI mode to cutscene mode!
            await Main.Instance.UI.Message.SetBars(true);
            //Assign text to a local string
            string stri = "I already used this here.";
            if (!_rightKey) stri = "I can't use this here.";
            await Main.Instance.UI.Message.SetText(stri, false);
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
            TransferRotation = _maintainRotation ? _playerCharacter.GlobalRotationDegrees : NewSceneRotate;
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

    public bool ItemChecker(string nameToCheck)
    {
        GD.Print("Called ItemChecker");
        for (int i = 0; i < Keys.Length; i++)
        {
            if (Keys[i] == nameToCheck)
            {
                return true;
            }
        }
        return false;
    }

    //#Override ToString
    public override string ToString()
    {
        return Name;
    }
}
