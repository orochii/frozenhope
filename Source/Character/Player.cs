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
		// Cast to float because working with doubles sucks when everything is using floats.
		var d = (float)delta;
		// Get input direction and dash
		var move = Input.GetVector("move_left","move_right","move_up","move_down");
		var dash = Input.IsActionPressed("dash");
		// Get current move state properties
		var currMoveState = dash ? moveStates[1] : moveStates[0];
		// Quick check for if we're moving or not
		if (move.LengthSquared() > 0) {
			// Set character visuals
			Graphic.MoveSpeed = dash ? 1 : 0;
			Graphic.Move = 1;
			// Move current velocity in the horizonal plane towards our target velocity, this means accelerate.
			var targetVelocity = new Vector3(move.X, 0, move.Y) * currMoveState.Speed;
			var planarVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
			planarVelocity = planarVelocity.MoveToward(targetVelocity, currMoveState.Acceleration * d);
			// Mix our two velocity vectors, replacing X and Z by our new velocity and keeping the original Y
			Velocity = new Vector3(planarVelocity.X, Velocity.Y, planarVelocity.Z);
			// Rotate character towards the direction we're moving to
			var dir = new Vector3(move.X, 0, -move.Y);
			float angle = dir.SignedAngleTo(Vector3.Forward, Vector3.Up);
			Rotation = new Vector3(0, angle, 0);
		} else {
			// Set character visuals to not moving
			Graphic.Move = 0;
			Graphic.MoveSpeed = 0;
			// Deaccelerate but only in the "horizontal plane", don't touch the vertical speed (gravity, etc)
			var planarVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
			planarVelocity = planarVelocity.MoveToward(Vector3.Zero, currMoveState.Deacceleration * d);
			Velocity = new Vector3(planarVelocity.X, Velocity.Y, planarVelocity.Z);
		}
		// This single, built-in function does all the magic regarding collision, slopes, etc.
		MoveAndSlide();
	}
}
