using Godot;

[GlobalClass]
public partial class CharacterMoveData : Resource {
    [Export] public float Speed;
    [Export] public float Acceleration;
    [Export] public float Deacceleration;
}