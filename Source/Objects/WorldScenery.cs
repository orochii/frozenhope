using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class WorldScenery : Area3D, Interactable
{
    [Export] public Label3D Interface;
    [Export] public float InteractAngle = 45f;
    [Export] public Camera3D SceneCamera;
    [Export] public bool EmitInteractedSignal = false;
    [Export(PropertyHint.MultilineText)] private string FlavorText;
    [Export] public Node3D TargetEvent;
    [Export] public bool RepeatEvent;
    [Export] public string[] Keys = new string[0];
    private Camera3D _stashedCamera;
    private Player _playerCharacter;
    private bool _keyPassover;
    public bool Active
    { get; set; }
    public bool CanInteract
    { get; set; }
    public bool InterfaceVisible
    { get; set; }

    //#Signals
    [Signal]
    public delegate void player_interactedEventHandler(string itemName);

    //Overriden Ready function
    public override void _Ready()
    {
        base._Ready();
        Interface.Visible = false;
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

    public void ItemChecker(string nameToCheck)
    {
        GD.Print("Called ItemChecker");
        for (int i = 0; i < Keys.Length; i++)
        {
            if (Keys[i] == nameToCheck)
            {
                _keyPassover = true;
            }
        }
    }

    //Interface functions
    public void _on_player_entered(Node3D body)
    {
        _playerCharacter = (Player)body;
        _playerCharacter.NearbyInteractables.Add(this);
        GD.Print("Player enter");
    }

    public void _on_player_left(Node3D body)
    {
        _playerCharacter.NearbyInteractables.Remove(this);
        Active = false;
        HideInterface();
        GD.Print("Player exit");
    }
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

    public async void InteractItem(string itemName) {
        if (!IsVisibleInTree()) return;
        // Stop game
        var tempAngle = InteractAngle;
        InteractAngle = 0f;
        Main.Instance.Busy = true;
        //Check if a non-default name was given and then check against accepted key values
        if (itemName != "empty") ItemChecker(itemName);
        //Check if Camera is attached to this object
        if (SceneCamera != null) { CameraSetup(); }
        //Set UI mode to cutscene mode!
        await Main.Instance.UI.Message.SetBars(true);
        
        //Emit the interacted signal in case any events need to listen for it.
        if (EmitInteractedSignal && !Main.Instance.State.GetSwitch(Name + "Triggered") && _keyPassover)
        {
            EmitSignal(SignalName.player_interacted, itemName);
            await ToSignal(TargetEvent, "event_over");
            // Needed certain things from messages to stay, like the bars up/down for cool "in-level" cutscenes :vaccabayt:
            Main.Instance.UI.Message.EndMessage();
            //Reset to StashedCamera if necessary
            if (SceneCamera != null) { CameraReset(); }
            //Create and store a switch so the event bound to this interactable can't be triggered again
            if (!RepeatEvent) Main.Instance.State.SetSwitch(Name + "Triggered", true);
            // Unpause game and reset some things
            _keyPassover = false;
            InteractAngle = tempAngle;
            Main.Instance.Busy = false;
            return;
        }

        //Assign text to a local string
        string str = FlavorText;
        if (itemName != "empty" && !_keyPassover) str = "I can't use this here.";
        await Main.Instance.UI.Message.SetText(str, false);
        // Call this on end of message, this just returns the UI mode back to whatever it was (usually gameplay).
        // Needed certain things from messages to stay, like the bars up/down for cool "in-level" cutscenes :vaccabayt:
        Main.Instance.UI.Message.EndMessage();
        //Reset to StashedCamera if necessary
        if (SceneCamera != null && SceneCamera.Current == true) { CameraReset(); }
        // Unpause game
        InteractAngle = tempAngle;
        Main.Instance.Busy = false;
    }

    public void CameraSetup()
    {
        _stashedCamera = GetViewport().GetCamera3D();
        _playerCharacter.Visible = false;
        SceneCamera.Current = true;
    }

    public void CameraReset()
    {
        SceneCamera.Current = false;
        _playerCharacter.Visible = true;
        _stashedCamera.Current = true;
    }
    
    //Overridden ToString
    public override string ToString()
    {
        return Name;
    }
}
