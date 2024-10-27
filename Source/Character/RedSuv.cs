using Godot;
using System;

public partial class RedSuv : Node3D
{
    [ExportCategory("Car Pieces")]
    [Export] private Node3D LeftDoor;
    [Export] private Node3D RightDoor;
    [Export] private Node3D FR_Wheel;
    [Export] private Node3D FL_Wheel;
    [Export] private Node3D RR_Wheel;
    [Export] private Node3D RL_Wheel;
}
