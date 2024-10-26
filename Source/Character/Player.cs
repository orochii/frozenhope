using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody3D
{
	[Export] private CharacterMoveData[] moveStates;
	[Export] private CharacterGraphic Graphic;
	[Export] private Area3D DetectionArea;
	[Export] private float MaxFocusAngle = 45f;
	private bool holsterMode = false;
	private List<Targettable> nearbyTargets = new List<Targettable>();
	private Targettable currentTarget;
	private float previousTargetRotation;
	public override void _Ready()
	{
		RefreshEquippedModel();
		DetectionArea.BodyEntered += OnBodyEntered;
		DetectionArea.BodyExited += OnbodyExited;
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
		// Cast to float because working with doubles sucks when everything is using floats.
		var d = (float)delta;
		if (canMove) {
			var isIdling = Graphic.StateMachine.ActionState == CharacterAnimState.EActionState.NONE;
			// Get input direction and dash
			var move = isIdling ? Input.GetVector("move_left","move_right","move_up","move_down") : Vector2.Zero;
			var run = isIdling ? Input.IsActionPressed("run") : false;
			var interact = isIdling ? Input.IsActionJustPressed("interact") : false;
			var holster = isIdling ? Input.IsActionJustPressed("holster") : false;
			var cycleLeft = isIdling ? Input.IsActionJustPressed("cycle_left") : false;
			var cycleRight = isIdling ? Input.IsActionJustPressed("cycle_right") : false;
			// Doing it a toggle, can imagine holding the button could be a pain and uneccesary. Toggle between combat and movement.
			if (holster) {
				holsterMode = !holsterMode;
				if (holsterMode) {
					currentTarget = PickClosestTarget();
					previousTargetRotation = Rotation.Y;
				} else {
					currentTarget = null;
				}
			}
			if (currentTarget != null) {
				var dir = cycleLeft ? -1 : cycleRight ? 1 : 0;
				var newTarget = PickNextTarget(dir);
				if (newTarget != null) currentTarget = newTarget;
			}
			ProcessMove(d,move,run,holsterMode);
			// Execute attack.
			if (interact) {
				if (holsterMode) {
					// Attack with held weapon.
					ExecuteAttack();
				} else {
					// Interact with environment.
				}
			}
			// Rotate towards target.
			if (currentTarget != null) {
				Vector3 dir = (currentTarget.GetPivotPosition() - GlobalPosition).Normalized();
				float angle = dir.SignedAngleTo(Vector3.Forward, Vector3.Up);
				float newRotation = Mathf.DegToRad(180) - angle;
				float r = Mathf.LerpAngle(previousTargetRotation, newRotation, 0.4f);
				Rotation = new Vector3(0, r, 0);
				previousTargetRotation = r;
			}
		}
		else {
			//
			ProcessMove(d,Vector2.Zero,false,false);
		}
	}
	private void ExecuteAttack() {
		var equip = Main.Instance.State.GetEquippedItemEntry();
		var item = BaseItem.Get(equip.itemID);
		if (item != null && item is WeaponItem) {
			var wpn = item as WeaponItem;
			// Set Fire state.
			Graphic.StateMachine.ActionState = CharacterAnimState.EActionState.ATTACK;
			// Check for ammo.
			if (equip.ammoQty > 0 && equip.ammoId.Length > 0) {
				// Get ammo item
				var ammo = BaseItem.Get(equip.ammoId) as AmmoItem;
				// Muzzle in weapon.
				Graphic.SetWeaponAnimation("muzzle");
				// Check if should do hitscan or spawn projectile.
				if (ammo.Projectile != null) {
					// Spawn projectile.
					// TODO.
				} else
                {
                    // Execute hitscan.
                    ExecuteHitscan(ammo);
                }
                // Spend ammo
                equip.ammoQty -= 1;
				if (equip.ammoQty == 0) equip.ammoId = "";
			} else {
				// Attack for no ammo (if possible).
				// TODO.
			}
		}
	}

    private void ExecuteHitscan(AmmoItem ammo)
    {
        var origin = Graphic.GetWeaponSpawnPoint().GlobalPosition;
        var target = origin + (GlobalTransform.Basis.Z * 10f);
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(origin, target);
        query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
        var result = spaceState.IntersectRay(query);
        if (result.Count > 0)
        {
            var hitPoint = result["position"].As<Vector3>();
            var hitNormal = result["normal"].As<Vector3>();
            var hitObject = result["collider"].As<Node3D>();
            if (hitObject is Targettable)
            {
                var hitTarget = hitObject as Targettable;
				// Spawn hit spark.
                var sparkSrc = hitTarget.CanBleed() ? ammo.HitSparkBlood : ammo.HitSparkObject;
                SpawnHitSpark(sparkSrc, hitPoint);
				// Execute damage.
				hitTarget.Damage(ammo.HitscanDamageType, ammo.HitscanDamage);
            }
            else
            {
				// Spawn hit spark.
                SpawnHitSpark(ammo.HitSparkObject, hitPoint);
            }
        }
    }

    private void SpawnHitSpark(PackedScene src, Vector3 pos) {
		if (src==null) return;
		var hitSpark = src.Instantiate<Node3D>();
		GetParent().AddChild(hitSpark);
		hitSpark.GlobalPosition = pos;
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
	private Targettable PickClosestTarget() {
		Targettable closest = null;
		float closestDst = 0;
		var forward = Transform.Basis.Z;
		foreach (var target in nearbyTargets) {
			// Get relative position
			Vector3 targetPos = target.GetPivotPosition();
			Vector3 targetRelativePos = targetPos - GlobalPosition;
			// Check if it's in front of player
			float angle = targetRelativePos.Normalized().AngleTo(forward);
			if (angle < Mathf.DegToRad(MaxFocusAngle)) {
				// Check if it's the closest enemy.
				float dst = targetRelativePos.Length();
				if (closest == null || dst < closestDst) {
					closest = target;
					closestDst = dst;
				}
			}
		}
		return closest;
	}
	private Targettable PickNextTarget(int dir) {
		if (currentTarget == null) return null;
		Targettable closest = null;
		float closestDst = 0;
		Vector3 forward = currentTarget.GetPivotPosition() - GlobalPosition;
		foreach (var target in nearbyTargets) {
			// Get relative position
			Vector3 targetPos = target.GetPivotPosition();
			Vector3 targetRelativePos = targetPos - GlobalPosition;
			// Check angle
			float angle = targetRelativePos.Normalized().SignedAngleTo(forward, Vector3.Up);
			bool valid = (dir>0 && angle>0) || (dir<0 && angle<0);
			if (valid) {
				// Check if it's the closest enemy.
				float dst = targetRelativePos.Length();
				if (closest == null || dst < closestDst) {
					closest = target;
					closestDst = dst;
				}
			}
		}
		return closest;
	}
	private void OnBodyEntered(Node3D body) {
		if (body is Targettable) {
			var target = body as Targettable;
			if (!nearbyTargets.Contains(target)) nearbyTargets.Add(target);
		}
	}
	private void OnbodyExited(Node3D body) {
		if (body is Targettable) {
			var target = body as Targettable;
			if (nearbyTargets.Contains(target)) nearbyTargets.Remove(target);
		}
	}
}
