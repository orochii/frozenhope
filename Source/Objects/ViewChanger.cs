using Godot;
using System;

public partial class ViewChanger : Area3D
{
    [Export] private Camera3D Camera = null;
    [Export] public Boolean Active = true;
    [Export] public ViewChanger[] Deactivate;
    [Export] public ViewChanger[] Activate;
    public void _OnPlayerEnter(Node3D body){
        //Ignore if message is running
        if (Main.Instance.Busy) return;
        //If ViewChanger is active, changes to its assigned camera
        if (Active) Camera.Current = true;
        //Iterates over the Deactivate array if it isn't empty, turning off assigned ViewChangers
        if (Deactivate.Length > 0) {
            for (int i = 0; i < Deactivate.Length; i++) {
                Deactivate[i].Active = false;
            }
        }
        //Iterates over the Activate array if it isn't empty, turning on assigned ViewChangers
        if (Activate.Length > 0) {
            for (int i = 0; i < Activate.Length; i++) {
                Activate[i].Active = true;
            }
        }
    }
}