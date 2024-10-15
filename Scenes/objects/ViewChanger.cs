using Godot;
using System;

public partial class ViewChanger : Area3D
{
    [Export] private Camera3D Camera = null;

    public void _OnPlayerEnter(Node3D body){
        Camera.Current = true;
    }
}
