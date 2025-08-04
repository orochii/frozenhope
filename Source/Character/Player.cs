using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

public partial class Player : CharacterBody3D, Targettable
{
	public static Player Instance;
	[Export] private CharacterMoveData[] moveStates;
	[Export] private CharacterGraphic Graphic;
	[Export] private Area3D DetectionArea;
	[Export] private Area3D ItemDetectionArea;
	[Export] private float RotateSpeed = 4;
	[Export] private float MaxFocusAngle = 45f;
	[Export] private Label3D InteractInterface;
	public int AimTimer;
	public int AimTimer2 = 0; //Electric Boogaloo
	private bool _aimMode = false;
	private List<Targettable> _nearbyTargets = new List<Targettable>();
	private Targettable _currentTarget;
	public Targettable CurrentTarget => _currentTarget;
	private List<Interactable> _nearbyInteractables = new List<Interactable>();
	private Interactable _closestInteractable;
	private float previousTargetRotation;
	private WorldItem NearbyItem;
	private WorldScenery NearbyScenery;
	private Door NearbyDoor;

	//Cache the inputs in order to save on memory by avoiding constant conversions from String to StringName
	StringName MoveLeft = "move_left";
	StringName MoveRight = "move_right";
	StringName MoveUp = "move_up";
	StringName MoveDown = "move_down";
	StringName Sprint = "run";
	StringName Interact = "interact";
	StringName Aim = "aim";
	StringName CycleLeft = "cycle_left";
	StringName CycleRight = "cycle_right";
	public bool Dead => Main.Instance.State.GetHealth() <= 0;
	private uint OriginalCollisionLayer;
	private uint OriginalCollisionMask;
	private Vector3 _empty = Vector3.Zero;
	private bool _frozen = false;

	//_Ready override
	public override void _Ready()
	{
		//First thing we do is freeze the player
		FreezeStatus();
		//We continue with the rest of the prep
		Instance = this;
		//Move and rotate player after transfer
		var transferVector3 = Main.Instance.TransferVector;
		var rotationVector3 = Main.Instance.TransferRotate;
		if (!transferVector3.Equals(_empty)) {
			GD.Print("We are working brotha");
			GlobalPosition = transferVector3;
			Main.Instance.TransferVector = _empty;
		}
		//Rotate player after transfer if rotation data is available
		if (!rotationVector3.Equals(_empty)) {
			GlobalRotationDegrees = rotationVector3;
			Main.Instance.TransferRotate = _empty;
		}
		// In case uh... revive? lol
		OriginalCollisionLayer = CollisionLayer;
		OriginalCollisionMask = CollisionMask;
		RefreshEquippedModel();
		DetectionArea.BodyEntered += OnBodyEntered;
		DetectionArea.BodyExited += OnbodyExited;
		//Interactable in range
		ItemDetectionArea.BodyEntered += OnItemInRange;
		ItemDetectionArea.BodyExited += OnItemOutOfRange;
	}
	public void RefreshEquippedModel() {
		var item = Main.Instance.State.GetEquippedItem();
		if (item != null && item is WeaponItem) {
			var weapon = item as WeaponItem;
			//We assign the equipped weapons AimTime to the player's AimTimer Timer
			AimTimer = weapon.AimTime;
			GD.Print("AimTimer Assigned: " + AimTimer);
			//When equiping weapon, we set the MaxFocusAngle to the equipped weapon's FocusAngle
			MaxFocusAngle = weapon.FocusAngle;
			// Orochii will explain this
			Graphic.SetWeaponModel(weapon.EquippedModel, weapon.WeaponBoneIdx);
			Graphic.SetVariationId(weapon.AnimationSet);
		} else {
			AimTimer = 0;
			//We set the MaxFocusAngle to 0 (I dunno, just feels right? -Ozzy)
			MaxFocusAngle = 0f;
			GD.Print("AimTimer Zero? " + AimTimer);
			Graphic.SetWeaponModel(null);
			Graphic.SetVariationId("");
		}
	}
	public override void _Process(double delta)
	{
		//If character isn't ready to move yet upon scene start, we return
		if (_frozen) return;
		//if player is dead, do nothing, duh
		if (Dead) return;
		// Shouldn't move if we're not in gameplay mode
		var canMove = Main.Instance.UI.Mode == (int)UiParent.EModes.GAMEPLAY;
		canMove = canMove && !Main.Instance.Busy;
		// Cast to float because working with doubles sucks when everything is using floats.
		var d = (float)delta;
		if (canMove) {
			var isIdling = Graphic.StateMachine.ActionState == EActionState.NONE;
			// Get input direction and dash
			var move = isIdling ? Input.GetVector(MoveLeft,MoveRight,MoveUp,MoveDown) : Vector2.Zero;
			var run = isIdling ? Input.IsActionPressed(Sprint) : false;
			var interact = isIdling ? Input.IsActionJustPressed(Interact) : false;
			var aim = isIdling ? Input.IsActionJustPressed(Aim) : false;
			var cycleLeft = isIdling ? Input.IsActionJustPressed(CycleLeft) : false;
			var cycleRight = isIdling ? Input.IsActionJustPressed(CycleRight) : false;
			// Doing it a toggle, can imagine holding the button could be a pain and uneccesary. Toggle between combat and movement.
			if (aim) {
				_aimMode = !_aimMode;
				if (_aimMode) {
					//Whenever we draw the weapon, we assign the equipped weapon's AimTimer to AimTimer2
					RefreshWeaponTimer();
					_currentTarget = PickClosestTarget();
					previousTargetRotation = Rotation.Y;
				} else {
					_currentTarget = null;
				}
			}

			//We reduce the AimTimer for damage calculation later on
			if (AimTimer2 > 0) {
				AimTimer2 -= 5;
			} else {
				AimTimer2 = 0;
			}
			
			/*Test code, to be delete or kept after testing*/
			if (_aimMode && _currentTarget == null) {
				RefreshWeaponTimer();
				_currentTarget = PickClosestTarget();
				previousTargetRotation = Rotation.Y;
			}

			//We cycle to a new target if the button is pressed
			if (_currentTarget != null) {
				var dir = cycleLeft ? -1 : cycleRight ? 1 : 0;
				var newTarget = PickNextTarget(dir);
				if (newTarget != null) _currentTarget = newTarget;
			}
			ProcessTankMove(d,move,run,_aimMode);
			// Execute attack.
			if (interact) {
				if (_aimMode) {
					// Attack with held weapon.
					ExecuteAttack();
				} else {
					// Interact with environment. Be it items, scenery, doors, etc (We use implied True/False boolean conditions)
					if (_closestInteractable != null && _closestInteractable.Active && _closestInteractable.InterfaceVisible) {
						_closestInteractable.InteractItem();
						if (_closestInteractable is WorldItem ) {
							_nearbyInteractables.Remove(_closestInteractable);
						}
						if (_nearbyInteractables.Count > 0 && Main.Instance.Busy == false) {
							RefreshInteractables();
						} else _closestInteractable = null; 
					}
					/*if (NearbyItem != null && NearbyItem.Active == true) NearbyItem.InteractItem();
					if (NearbyScenery != null && NearbyScenery.Active == true) NearbyScenery.InteractItem();
					if (NearbyDoor != null && NearbyDoor.Active == true) NearbyDoor.InteractItem();*/
				}
			}
			// Rotate towards target.
			if (_currentTarget != null) {
				Vector3 dir = (_currentTarget.GetPivotPosition() - GlobalPosition).Normalized();
				float angle = dir.SignedAngleTo(Vector3.Forward, Vector3.Up);
				float newRotation = Mathf.DegToRad(180) - angle;
				float r = Mathf.LerpAngle(previousTargetRotation, newRotation, 0.4f);
				Rotation = new Vector3(0, r, 0);
				previousTargetRotation = r;
			}
		}
		else {
			ProcessTankMove(d,Vector2.Zero,false,false);
		}
	}
	private void ExecuteAttack() {
		var equip = Main.Instance.State.GetEquippedItemEntry();
		var item = BaseItem.Get(equip.itemID);
		if (item != null && item is WeaponItem) {
			var wpn = item as WeaponItem;
			// Set Fire state.
			Graphic.StateMachine.ActionState = EActionState.ATTACK;
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
                    ExecuteHitscan(wpn, ammo);
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
    private void ExecuteHitscan(WeaponItem weapon, AmmoItem ammo)
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
				// Execute damage based on AimTimer2
				if (AimTimer2 > 0) {
					var dmg = Main.Instance.State.CalculateDamage(ammo.DamagePartial, weapon.DamageBaseVariance);
					hitTarget.Damage(ammo.HitscanDamageType, dmg);
				} else {
					var dmg = Main.Instance.State.CalculateDamage(ammo.HitscanDamage, weapon.DamageBaseVariance);
					hitTarget.Damage(ammo.HitscanDamageType, dmg);
					hitTarget.ForceActionState(EActionState.DAMAGE);
				}
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
	//Tank Move Processing where move = ("move_left","move_right","move_up","move_down")
	private void ProcessTankMove(float d, Vector2 move, bool run, bool aiming) {
		if (_nearbyInteractables.Count > 0 && Main.Instance.Busy == false) RefreshInteractables();
		// You can't run and aim, because I say so! (less animations :P)
		//Agreed (Ozzy)
		if (aiming==true) run = false;
		Graphic.StateMachine.ModeState = aiming ? EModeState.AIMING : EModeState.IDLE;
		// Get current move state properties
		var currMoveState = run ? moveStates[1] : moveStates[0];
		// Work over a copy, commit changes later.
		var _velocity = Velocity;
		// Apply gravity
		if (!IsOnFloor()) _velocity += GetGravity();
		// Quick check for if we're moving forward or not
		if (move.LengthSquared() > 0) {
			// Set character visuals
			Graphic.StateMachine.MoveState = (run && move.Y<0) ? EMoveState.RUN : EMoveState.WALK;
			var targetVelocity = (Transform.Basis.Z * -move.Y);
			// Are we in aim mode?
			if (aiming && _currentTarget!= null) {
				// When aiming and locked on a target, left/right strafe the character around the target instead.
				targetVelocity += (Transform.Basis.X * -move.X);
			} 
			else {
				// We rotate the character
				var RotationY = GlobalRotation.Y;
    			RotationY = Mathf.Wrap(RotationY + -move.X * d * RotateSpeed, 0, Mathf.Tau);
    			GlobalRotation = new Vector3(0, RotationY, 0);
			}
			// Move current velocity in the horizonal plane towards our target velocity, this means accelerate.
			targetVelocity = targetVelocity * currMoveState.Speed;
			var planarVelocity = new Vector3(_velocity.X, 0, _velocity.Z);
			planarVelocity = planarVelocity.MoveToward(targetVelocity, currMoveState.Acceleration * d);
			// Mix our two velocity vectors, replacing X and Z by our new velocity and keeping the original Y
			_velocity = new Vector3(planarVelocity.X, _velocity.Y, planarVelocity.Z);
		} else {
			// Set character visuals to not moving
			Graphic.StateMachine.MoveState = EMoveState.STAND;
			// Deaccelerate but only in the "horizontal plane", don't touch the vertical speed (gravity, etc)
			var planarVelocity = new Vector3(_velocity.X, 0, _velocity.Z);
			planarVelocity = planarVelocity.MoveToward(Vector3.Zero, currMoveState.Deacceleration * d);
			_velocity = new Vector3(planarVelocity.X, _velocity.Y, planarVelocity.Z);
		}
		// This single, built-in function does all the magic regarding collision, slopes, etc.
		Velocity = _velocity;
		MoveAndSlide();
	}
	
	//Default Move Processing, curretly depreciated
	private void ProcessMove(float d, Vector2 move, bool run, bool aiming) {
		// You can't run and aim, because I say so! (less animations :P)
		if (aiming==true) run = false;
		Graphic.StateMachine.ModeState = aiming ? EModeState.AIMING : EModeState.IDLE;
		// Get current move state properties
		var currMoveState = run ? moveStates[1] : moveStates[0];
		// Apply gravity
		var v = Velocity;
		v += GetGravity();
		Velocity = v;
		// Quick check for if we're moving or not
		if (move.LengthSquared() > 0) {
			// Set character visuals
			Graphic.StateMachine.MoveState = run ? EMoveState.RUN : EMoveState.WALK;
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
			Graphic.StateMachine.MoveState = EMoveState.STAND;
			// Deaccelerate but only in the "horizontal plane", don't touch the vertical speed (gravity, etc)
			var planarVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
			planarVelocity = planarVelocity.MoveToward(Vector3.Zero, currMoveState.Deacceleration * d);
			Velocity = new Vector3(planarVelocity.X, Velocity.Y, planarVelocity.Z);
		}
		// This single, built-in function does all the magic regarding collision, slopes, etc.
		MoveAndSlide();
	}
	
	//We iterate over the enemies stored in the _nearbyTargets List and return the one that computes as closest to the player
	private Targettable PickClosestTarget() {
		Targettable closest = null;
		float closestDst = 0;
		var forward = Transform.Basis.Z;
		foreach (var target in _nearbyTargets) {
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

	//Check if we have an active target, we then switch to the next target in the _nearbyTargets list
	private Targettable PickNextTarget(int dir) {
		if (_currentTarget == null) return null;
		Targettable closest = null;
		float closestDst = 0;
		Vector3 forward = _currentTarget.GetPivotPosition() - GlobalPosition;
		foreach (var target in _nearbyTargets) {
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

	//If a Targettable entity enters the player's range, add it to the _nearbyTargets list if it isn't in there already.
	private void OnBodyEntered(Node3D body) {
		if (body == this) return;
		if (body is Targettable) {
			var target = body as Targettable;
			if (!_nearbyTargets.Contains(target)) _nearbyTargets.Add(target);
			//In case enemy enters Player's detection area while aimMode is true, set _currentTarget to closests enemy
			if (_aimMode) _currentTarget = PickClosestTarget();
		}
	}

	//If a targettable entity leaves the player's range, remove it from the _nearbyTargets list
	private void OnbodyExited(Node3D body) {
		if (body == this) return;
		if (body is Targettable) {
			var target = body as Targettable;
			if (_nearbyTargets.Contains(target)) _nearbyTargets.Remove(target);
		}
	}
	
	//Processing for when items are in range of the player
	private void OnItemInRange(Node3D body) {
		//Print to console for debugging
		GD.Print(string.Format("Interactable {0} Entered", body.ToString()));
		GD.Print("List has ", _nearbyInteractables.Count, " elements");
		
		if (body is Interactable)
		{
			var item = body as Interactable;
			if (!_nearbyInteractables.Contains(item)) _nearbyInteractables.Add(item);
			if (Main.Instance.Busy == false) RefreshInteractables();
			GD.Print("List has ", _nearbyInteractables.Count, " elements");
		}
		//Main function processing
		/*var evaluator = body;
		switch (evaluator) {
			case WorldItem:
				var itemObject = body as WorldItem;
				itemObject.Active = true;
				NearbyItem = itemObject;
				break;
			case WorldScenery:
				var flavorObject = body as WorldScenery;
				flavorObject.Active = true;
				NearbyScenery = flavorObject;
				break;
			case Door:
				var doorObject = body as Door;
				doorObject.Active = true;
				NearbyDoor = doorObject;
				break;
		}*/
	}

	private void OnItemOutOfRange(Node3D body) {
		//Print to console for debugging
		GD.Print(string.Format("Interactable {0} Left", body.ToString()));
		/*START Temp Code*/
		if (body is Interactable) {
			var item = body as Interactable;
			item.Active = false;
			if (_nearbyInteractables.Contains(item)) _nearbyInteractables.Remove(item);
			if (_nearbyInteractables.Count > 0 && Main.Instance.Busy == false)  RefreshInteractables();
			else { 
				item.Active = false;
				item.HideInterface();
				_closestInteractable = null;
			}
		}
		/*END Temp Code*/
		//Actual function processing
		/*var evaluator = body;
		switch (evaluator) {
			case WorldItem:
				var itemObject = body as WorldItem;
				itemObject.Active = false;
				itemObject.HideInterface();
				NearbyItem = null;
				break;
			case WorldScenery:
				var flavorObject = body as WorldScenery;
				flavorObject.Active = false;
				flavorObject.HideInterface();
				NearbyScenery = null;
				break;
			case Door:
				var doorObject = body as Door;
				doorObject.Active = false;
				doorObject.HideInterface();
				NearbyDoor = null;
				break;
		}*/
	}

	//Iterates over the Interactable inside of the _nearbyInteractables list and returns the closest one.
	public Interactable GetClosestInteract() {
		Interactable closest = null;
		float closestDst = 0;
		foreach (var item in _nearbyInteractables) {
			// Get relative position
			Vector3 targetPos = item.GetItemPosition();
			Vector3 targetRelativePos = targetPos - GlobalPosition;
			// Check if it's the closest item.
			float dst = targetRelativePos.Length();
			if (closest == null || dst < closestDst) {
				closest = item;
				closestDst = dst;
			}
		}
		return closest;
	}

	// Refresh nearest Interactable items
	public void RefreshInteractables()
	{
		_closestInteractable = GetClosestInteract();
		foreach (var itemOverdue in _nearbyInteractables) itemOverdue.Active = false;
		_closestInteractable.Active = true;
	}

	// Reticle Processing
    public Vector3 GetPivotPosition()
    {
        return GlobalPosition;
    }
    public Vector3 GetReticlePosition()
    {
        return GlobalPosition + new Vector3(0, 2, 0);
    }
    public bool CanBleed()
    {
        return true;
    }
    public void Damage(EDamageType damageType, int damage)
    {
        ChangeHealth(damage);
		Main.Instance.UI.Gameplay.SpawnPopup(this, damage);
    }
	private void ChangeHealth(int v) {
		bool _dead = Dead;
		Main.Instance.State.ChangeHealth(-v);
		if (Dead != _dead) {
			if (Dead) {
				Die();
			} else {
				Revive();
			}
		}
	}
	public void Die() {
		// Disable movement/collision.
		CollisionLayer = 0;
		CollisionMask = 0;
		// Show animation.
		Graphic.StateMachine.ActionState = EActionState.DEATH;
		// Change to gameover.
		Main.Instance.UI.SetUIMode((int)UiParent.EModes.GAMEOVER);
	}
	public void Revive() {
		// Disable movement/collision.
		CollisionLayer = OriginalCollisionLayer;
		CollisionMask = OriginalCollisionMask;
		// Show animation.
		Graphic.StateMachine.ActionState = EActionState.REVIVE;
	}
	public void ForceActionState(EActionState state) {
		if (Dead) return;
		//
		Graphic.StateMachine.ActionState = state;
	}
	public ETargetFaction GetTargetFaction() {
		return ETargetFaction.PLAYER;
	}

	private void RefreshWeaponTimer() {
		AimTimer2 = AimTimer;
	}

	//Signal functions
	public void FreezeStatus() {
		if (_frozen) {
			_frozen = false;
			GD.Print("Player unfrozen");
		} else {
			_frozen = true;
			GD.Print("Player frozen.");
		}
	}
}
