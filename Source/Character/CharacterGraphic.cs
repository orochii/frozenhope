using Godot;
using System;

public partial class CharacterGraphic : Node3D
{
	[Export] AnimationTree Animator;
	private float move = 0;
	private float moveSpeed = 0;
	public float Move {
		get {
			return move;
		}
		set {
			move = Math.Clamp(value, -1, 1);
			Animator.Set("parameters/Move/blend_amount", move);
		}
	}
	public float MoveSpeed {
		get {
			return moveSpeed;
		}
		set {
			moveSpeed = Math.Clamp(value, 0, 1);
			Animator.Set("parameters/MoveSpeed/blend_amount", moveSpeed);
		}
	}
}
