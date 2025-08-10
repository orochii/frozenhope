using Godot;
using System;

public partial class ViewChanger : Area3D
{
    [Export] private Camera3D Camera = null;
    [Export] public Boolean Active = true;
    [Export] public ViewChanger[] Deactivate = new ViewChanger[0];
    [Export] public ViewChanger[] Activate = new ViewChanger[0];
    [Export] public StaticBody3D[] HideBodies = new StaticBody3D[0];
    [Export] public StaticBody3D[] ShowBodies = new StaticBody3D[0];

    public void _OnPlayerEnter(Node3D body)
    {
        //Ignore if message is running
        if (Main.Instance.Busy) return;
        //If ViewChanger is active, changes to its assigned camera
        if (Active) Camera.Current = true;
        //Iterates over the Deactivate array if it isn't empty, turning off assigned ViewChangers
        if (Deactivate.Length > 0)
        {
            for (int i = 0; i < Deactivate.Length; i++)
            {
                Deactivate[i].Active = false;
            }
        }
        //Iterates over the Activate array if it isn't empty, turning on assigned ViewChangers
        if (Activate.Length > 0)
        {
            for (int i = 0; i < Activate.Length; i++)
            {
                Activate[i].Active = true;
            }
        }
        //Iterates over the HideBodies array if it isn't empty, making assigned objects invisible
        if (HideBodies.Length > 0)
        {
            for (int i = 0; i < HideBodies.Length; i++)
            {
                HideBodies[i].Visible = false;
            }
        }
        //Iterates over the HideBodies array if it isn't empty, making assigned objects visible
        if (ShowBodies.Length > 0)
        {
            for (int i = 0; i < ShowBodies.Length; i++)
            {
                ShowBodies[i].Visible = true;
            }
        }
    }
}