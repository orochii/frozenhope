using Godot;
/// <summary>
/// A Global Class containing the move data of Character objects
/// </summary>
[GlobalClass]
public partial class CharacterMoveData : Resource {
    [Export] public float Speed;
    [Export] public float Acceleration;
    [Export] public float Deacceleration;
}