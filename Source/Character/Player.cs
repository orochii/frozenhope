using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export] CharacterMoveData[] moveStates;
	[Export] private CharacterGraphic Graphic;
	private bool holsterMode = false;
	public override void _Ready()
	{
		RefreshEquippedModel();
	}
	private void RefreshEquippedModel() {
		var item = Main.Instance.State.GetEquippedItem();
		if (item != null && item is WeaponItem) {
			var wpn = item as WeaponItem;
			Graphic.SetWeaponModel(wpn.EquippedModel);
			Graphic.SetVariationId(wpn.AnimationSet);
		} else {
			Graphic.SetWeaponModel(null);
			Graphic.SetVariationId("");
		}
	}
	public override void _Process(double delta)
	{
		var canMove = Main.Instance.UI.Mode == (int)UiParent.EModes.GAMEPLAY;
		GD.Print("canMove=",canMove);
		// Cast to float because working with doubles sucks when everything is using floats.
		var d = (float)delta;
		if (canMove) {
			// Get input direction and dash
			var move = Input.GetVector("move_left","move_right","move_up","move_down");
			var run = Input.IsActionPressed("run");
			var holster = Input.IsActionJustPressed("holster");
			// Doing it a toggle, can imagine holding the button could be a pain and uneccesary. Toggle between combat and movement.
			if (holster) holsterMode = !holsterMode;
			ProcessMove(d,move,run,holsterMode);
		}
		else {
			//
			ProcessMove(d,Vector2.Zero,false,false);
		}
	}
	private void ProcessMove(float d, Vector2 move, bool run, bool holstering) {
		// You can't run and holster, because I say so! (less animations :P)
		if (holstering==true) run = false;
		Graphic.StateMachine.ModeState = holstering ? CharacterAnimState.EModeState.HOLSTER : CharacterAnimState.EModeState.IDLE;
		// Get current move state properties
		var currMoveState = run ? moveStates[1] : moveStates[0];
		// Apply gravity
		var v = Velocity;
		v += GetGravity();
		Velocity = v;
		// Quick check for if we're moving or not
		if (move.LengthSquared() > 0) {
			// Set character visuals
			Graphic.StateMachine.MoveState = run ? CharacterAnimState.EMoveState.RUN : CharacterAnimState.EMoveState.WALK;
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
			Graphic.StateMachine.MoveState = CharacterAnimState.EMoveState.STAND;
			// Deaccelerate but only in the "horizontal plane", don't touch the vertical speed (gravity, etc)
			var planarVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
			planarVelocity = planarVelocity.MoveToward(Vector3.Zero, currMoveState.Deacceleration * d);
			Velocity = new Vector3(planarVelocity.X, Velocity.Y, planarVelocity.Z);
		}
		// This single, built-in function does all the magic regarding collision, slopes, etc.
		MoveAndSlide();
	}
}
