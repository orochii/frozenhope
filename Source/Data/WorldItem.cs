using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class WorldItem : StaticBody3D, Interactable
{
    [Export] public BaseItem Container;
    [Export] public int Amount;
    [Export] public Label3D Interface;

    public override void _Ready() {
        Interface.Visible = false;
        GD.Print("Read: Hide Interface");
    }

    public void ShowInterface() {
        GD.Print("Show Interface");
        Interface.Visible = true;
    }

    public void HideInterface() {
        GD.Print("Hide Interface");
        Interface.Visible = false;
    }

    public void InteractItem() {
        //Remove Item and add to inventory

        //We remove the time (I suspect we'll need to add a permant removal flag)
        QueueFree();
    }
}
