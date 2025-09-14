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
    private Camera3D _stashedCamera;
    private Player _playerCharacter;
    public bool Active
    { get; set; }
    public bool InterfaceVisible
        { get; set; }

    //#Signals
    [Signal]
    public delegate void player_interactedEventHandler();

    //Overriden Ready function
    public override void _Ready()
    {
        base._Ready();
        Interface.Visible = false;
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

    public async void InteractItem() {
        if (!IsVisibleInTree()) return;
        // Stop game
        var tempAngle = InteractAngle;
        InteractAngle = 0f;
        Main.Instance.Busy = true;
        //Check if Camera is attached to this object
        if (SceneCamera != null) { CameraSetup(); }
        //Set UI mode to cutscene mode!
        await Main.Instance.UI.Message.SetBars(true);
        //Emit the interacted signal in case any events need to listen for it.
        if (EmitInteractedSignal && !Main.Instance.State.GetSwitch(Name+"Triggered"))
        {
            EmitSignal(SignalName.player_interacted);
            await ToSignal(TargetEvent, "event_over");
            // Needed certain things from messages to stay, like the bars up/down for cool "in-level" cutscenes :vaccabayt:
            Main.Instance.UI.Message.EndMessage();
            //Reset to StashedCamera if necessary
            if (SceneCamera != null) { CameraReset(); }
            //Create and store a switch so the event bound to this interactable can't be triggered again
            Main.Instance.State.SetSwitch(Name+"Triggered", true);
            // Unpause game
            InteractAngle = tempAngle;
            Main.Instance.Busy = false;
            return;
        }
        //Assign text to a local string
        string str = FlavorText;
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
