using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export] CharacterMoveData[] moveStates;
	[Export] private CharacterGraphic Graphic;
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		var d = (float)delta;
		var move = Input.GetVector("move_left","move_right","move_up","move_down");
		var dash = Input.IsActionPressed("dash");
		var currMoveState = dash ? moveStates[1] : moveStates[0];
		if (move.LengthSquared() > 0) {
			Graphic.MoveSpeed = dash ? 1 : 0;
			Graphic.Move = 1;
			var targetVelocity = new Vector3(move.X, 0, move.Y) * currMoveState.Speed;
			var planarVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
			planarVelocity = planarVelocity.MoveToward(targetVelocity, currMoveState.Acceleration * d);
			Velocity = new Vector3(planarVelocity.X, Velocity.Y, planarVelocity.Z);
			//
			var dir = new Vector3(move.X, 0, -move.Y);
			float angle = dir.SignedAngleTo(Vector3.Forward, Vector3.Up);
			Rotation = new Vector3(0, angle, 0);
		} else {
			Graphic.Move = 0;
			Graphic.MoveSpeed = 0;
			var planarVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
			planarVelocity = planarVelocity.MoveToward(Vector3.Zero, currMoveState.Deacceleration * d);
			Velocity = new Vector3(planarVelocity.X, Velocity.Y, planarVelocity.Z);
		}
		MoveAndSlide();
	}
}
