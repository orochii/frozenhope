using Godot;
using System;
using System.Runtime.InteropServices;

public partial class EvtValveTurned : Node3D
{
    [Export] public AnimationPlayer Animation;
    [Export] public GpuParticles3D Steam;
    [Export] public CollisionShape3D Blocker;
    [Signal]
    public delegate void event_overEventHandler();

    public override void _Ready()
    {
        if (Main.Instance.State.GetSwitch(Name + "Switch"))
        {
            Steam.Emitting = false;
            Blocker.Disabled = true;
        }
    }

    public async void _on_player_interacted(string itemName)
    {
        /*var Items = Main.Instance.State.GetInventoryEntries();
        string KeyCheck = "";
        foreach (var entry in Items)
        {
            var data = BaseItem.Get(entry.itemID);
            if (data.DisplayName == "hexvalve") KeyCheck = data.DisplayName;
        }
        //If KeyCheck is valid, execute the animation, otherwise stop
        if (KeyCheck != "hexvalve") return;
        //DELETE THIS LATER
        //This is just test code, please delete it later
        Main.Instance.State.SetSwitch("hexvalve", true);
        //DELETE ENDS HERE*/
        if (itemName != "hexvalve") return;
        GD.Print("Valve has been interact, lets turn on/off steam");
        if (!Main.Instance.State.GetSwitch(Name + "Switch"))
        {
            GD.Print("Play animation!");
            Animation.Play("ValveTurnOff");
            await ToSignal(Animation, AnimationPlayer.SignalName.AnimationFinished);
            Main.Instance.State.SetSwitch(Name + "Switch", true);
            GD.Print("Animation over");
        }
        else
        {
            GD.Print("Play animation!");
            Animation.Play("ValveTurnOn");
            await ToSignal(Animation, AnimationPlayer.SignalName.AnimationFinished);
            Main.Instance.State.SetSwitch(Name + "Switch", false);
            GD.Print("Animation over");
        }
        EmitSignal(SignalName.event_over);
    }

}

