using Godot;
using System;

public partial class Enemy : CharacterBody3D, Targettable
{
	[Export] private CharacterGraphic Graphic;
	[Export] private Node3D TargetPivot;

    public Vector3 GetPivotPosition()
    {
        return GlobalPosition;
    }

    public Vector3 GetReticlePosition()
    {
        return TargetPivot.GlobalPosition;
    }
}
