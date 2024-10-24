using Godot;
using System;

[GlobalClass]
public partial class CharacterAnimStateEntry : Resource
{
    [Export] public string Id;
    [Export] public string AnimationId;
}
