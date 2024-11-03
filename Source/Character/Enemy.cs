using Godot;
using System;
using System.Collections.Generic;
using System.Transactions;

public partial class Enemy : CharacterBody3D, Targettable
{
	[Export] private CharacterMoveData[] moveStates;
	[Export] private float RotateSpeed = 4;
	[Export] private CharacterGraphic Graphic;
	[Export] private Node3D TargetPivot;
	[Export] private int MaxHealth;
	[ExportGroup("Navigation Setup")]
	[Export] private NavigationAgent3D Navigator;
	[Export] private Node3D[] PatrolPoints;
	[ExportGroup("Detection and pursuit")]
	[Export] Area3D DetectionArea;
	[Export] float DetectionAngle = 135f;
	[Export] float PursueWalkTowardsTargetsWithinDegrees = 15f;
	[Export] float PursueSteerWhenHigherThan = 2f;
	[Export] Godot.Collections.Array<ETargetFaction> DetectedFactions = new Godot.Collections.Array<ETargetFaction> {ETargetFaction.PLAYER};
	[Export] private float MemoryCooldown = 1f;
	[ExportGroup("Attack Patterns")]
	[Export] AttackArea[] AttackAreas;
	[Export] EnemyAttackPattern[] AttackPatterns;
	private int CurrentHealth;
	private uint OriginalCollisionLayer;
	private uint OriginalCollisionMask;
	private List<Targettable> detectedTargets = new List<Targettable>();
	private Targettable currentTarget;
	private float currentTargetMemoryTimer;
	private Vector3 currentTargetMemoryLastPosition;
	private int currentPatrolPoint;
	private int currentAttackPatternIdx;
	private float currentAttackTimer = 0;
	// Will look as a property but it's just like a getter method. 
	// Usually do these for very common conditions, like checking if character is dead.
	public bool Dead => CurrentHealth <= 0;
    public override void _Ready()
    {
        base._Ready();
		CurrentHealth = MaxHealth;
		// In case uh... revive? lol
		OriginalCollisionLayer = CollisionLayer;
		OriginalCollisionMask = CollisionMask;
		// Behavior helper object setup.
		DetectionArea.BodyEntered += OnDetectedBodyEnter;
		DetectionArea.BodyExited += OnDetectedBodyExit;
    }
    public override void _Process(double delta)
    {
		if (Dead) return;
		var isIdling = Graphic.StateMachine.ActionState == EActionState.NONE;
		var isAttacking = Graphic.StateMachine.ActionState == EActionState.ATTACK;
		Vector2 move = new Vector2();
		bool run = false;
		if (isIdling) {
			// If is not paying attention to something specific.
			if (currentTarget == null) {
				// Check detected bodies.
				foreach (var target in detectedTargets) {
					// Is seeing target?
					if (CanSeeTarget(target)) {
						currentTarget = target;
					}
				}
				// If no target was found.
				if (currentTarget == null) {
					// Patrol?
					if (PatrolPoints.Length > 0) {
						var currPoint = PatrolPoints[currentPatrolPoint];
						var dst = currPoint.GlobalPosition.DistanceSquaredTo(GlobalPosition);
						if (dst < 0.01f) {
							currentPatrolPoint = (currentPatrolPoint + 1) % PatrolPoints.Length;
							Navigator.TargetPosition = PatrolPoints[currentPatrolPoint].GlobalPosition;
						}
						else {
							var pos = Navigator.GetNextPathPosition();
							move = ComputeMovementTowards(pos);
						}
					}
				}
				// If target found
				else {
					RerollNewAttackPattern();
				}
			}
			// If has its attention focused.
			else {
				run = true;
				// Can see target?
				if (CanSeeTarget(currentTarget)) {
					currentTargetMemoryLastPosition = currentTarget.GetPivotPosition();
					currentTargetMemoryTimer = MemoryCooldown;
				}
				// Pursue target.
				Navigator.TargetPosition = currentTargetMemoryLastPosition;
				var pos = Navigator.GetNextPathPosition();
				move = ComputeMovementTowards(pos);
				// If target is within range, attack!
				if (TargetWithinRange(currentAttackPatternIdx)) {
					ExecuteAttack(currentAttackPatternIdx);
				} else {
					currentAttackTimer -= (float)delta;
					if (currentAttackTimer < 0) {
						RerollNewAttackPattern();
					}
				}
				// Should forget target?
				currentTargetMemoryTimer -= (float)delta;
				if (currentTargetMemoryTimer < 0) {
					currentTarget = null;
				}
			}
		} else if (isAttacking) {
			// Update attack state.
			UpdateAttackState((float)delta);
		}
        // Execute move
		ProcessTankMove((float)delta, move, run, false);
    }
	private void RerollNewAttackPattern() {
		currentAttackPatternIdx = GD.RandRange(0, AttackPatterns.Length-1);
		currentAttackTimer = 1f;
	}
	private bool TargetWithinRange(int idx) {
		if (currentTarget == null) return false;
		EnemyAttackPattern pattern = AttackPatterns[idx];
		AttackArea area = AttackAreas[pattern.AttackAreaIdx];
		return area.BodiesInsideArea.Contains(currentTarget);
	}
	private void ExecuteAttack(int idx) {
		currentAttackPatternIdx = idx;
		EnemyAttackPattern pattern = AttackPatterns[currentAttackPatternIdx];
		Graphic.StateMachine.ActionState = EActionState.ATTACK;
		Graphic.StateMachine.VariationId = pattern.AnimationVariationId;
		currentAttackTimer = 0;
	}
	private void UpdateAttackState(float d) {
		// Advance timer
		var prevTimer = currentAttackTimer;
		currentAttackTimer += d;
		// Get pattern
		EnemyAttackPattern pattern = AttackPatterns[currentAttackPatternIdx];
		// Compute if this is an attack frame.
		foreach (var f in pattern.DamageFrames) {
			if (f > prevTimer && f <= currentAttackTimer) {
				// It is a valid attack frame at this time.
				AttackArea area = AttackAreas[pattern.AttackAreaIdx];
				// Apply damage to all targets in area.
				foreach (var b in area.BodiesInsideArea) {
					b.Damage(EDamageType.NONE, pattern.Damage);
					if (pattern.TargetActionState != EActionState.NONE) {
						b.ForceActionState(pattern.TargetActionState);
					}
				}
			}
		}
	}
	private void ProcessTankMove(float d, Vector2 move, bool run, bool aiming) {
		// You can't run and aim, because I say so! (less animations :P)
		//Agreed (Ozzy)
		if (aiming==true) run = false;
		Graphic.StateMachine.ModeState = aiming ? EModeState.AIMING : EModeState.IDLE;
		// Get current move state properties
		var currMoveState = run ? moveStates[1] : moveStates[0];
		// Apply gravity
		var v = Velocity;
		v += GetGravity();
		Velocity = v;
		// Quick check for if we're moving forward or not
		if (move.X != 0 || move.Y != 0) {
			// Set character visuals
			Graphic.StateMachine.MoveState = (run && move.Y>0) ? EMoveState.RUN : EMoveState.WALK;
			var targetVelocity = Transform.Basis.Z * -move.Y;
			// Are we in aim mode?
			if (aiming) {
				// When aiming, left/right strafe the character around the target instead.
				targetVelocity += Transform.Basis.X * -move.X;
			} else {
				// We rotate the character
				var RotationY = GlobalRotation.Y;
    			RotationY = Mathf.Wrap(RotationY + -move.X * d * RotateSpeed, 0, Mathf.Tau);
    			GlobalRotation = new Vector3(0, RotationY, 0);
			}
			// Move current velocity in the horizonal plane towards our target velocity, this means accelerate.
			targetVelocity = targetVelocity * currMoveState.Speed;
			var planarVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
			planarVelocity = planarVelocity.MoveToward(targetVelocity, currMoveState.Acceleration * d);
			// Mix our two velocity vectors, replacing X and Z by our new velocity and keeping the original Y
			Velocity = new Vector3(planarVelocity.X, Velocity.Y, planarVelocity.Z);
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
	private Vector2 ComputeMovementTowards(Vector3 target) {
		Vector2 move = new Vector2();
		float angle = GetSignedYAngle(GlobalPosition, target);
		float absAngle = Math.Abs(angle);
		// Move forward when target is within 15 degrees (should be in front enough to pursue, hopefully).
		if (absAngle < PursueWalkTowardsTargetsWithinDegrees) move.Y = -1;
		// If angle is higher than 2 degrees, try to steer enemy towards the target.
		if (absAngle > PursueSteerWhenHigherThan) {
			move.X = -Math.Sign(angle);
		}
		return move;
	}
	private bool CanSeeTarget(Targettable t) {
		var origin = GetReticlePosition();
		var target = t.GetPivotPosition() + new Vector3(0,1,0);
		// Check if target is within enemy's angle of vision.
		float angleDegrees = GetYAngle(origin, target);
		if (angleDegrees > DetectionAngle) return false;
		// Check if there's something between enemy and target.
		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(origin, target);
        query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
        var result = spaceState.IntersectRay(query);
		if (result.Count > 0) {
			var hitObject = result["collider"].As<Node3D>();
            if (hitObject is Targettable) {
				if (hitObject as Targettable == t) return true;
			}
		}
		// Return false if not seeing target.
		return false;
	}
	// Helper function for getting flat Y angle from the target towards the 
	private float GetYAngle(Vector3 origin, Vector3 target) {

		Vector3 forward = Transform.Basis.Z;
		Vector3 flattenedTarget = new Vector3(target.X - origin.X, 0, target.Z - origin.Z).Normalized();
		return Mathf.RadToDeg(forward.AngleTo(flattenedTarget));
	}
	private float GetSignedYAngle(Vector3 origin, Vector3 target) {

		Vector3 forward = Transform.Basis.Z;
		Vector3 flattenedTarget = new Vector3(target.X - origin.X, 0, target.Z - origin.Z).Normalized();
		return Mathf.RadToDeg(forward.SignedAngleTo(flattenedTarget, Vector3.Up));
	}
    public Vector3 GetPivotPosition()
    {
        return GlobalPosition;
    }
    public Vector3 GetReticlePosition()
    {
        return TargetPivot.GlobalPosition;
    }
	public bool CanBleed() {
		return true;
	}
	public void Damage(EDamageType damageType, int damage) {
		ChangeHealth(-damage);
	}
	public void ForceActionState(EActionState state) {
		//
		Graphic.StateMachine.ActionState = state;
	}
	public ETargetFaction GetTargetFaction() {
		return ETargetFaction.ENEMY;
	}
	public void Die() {
		// Disable movement/collision.
		CollisionLayer = 0;
		CollisionMask = 0;
		// Show animation.
		Graphic.StateMachine.ActionState = EActionState.DEATH;
	}
	public void Revive() {
		// Disable movement/collision.
		CollisionLayer = OriginalCollisionLayer;
		CollisionMask = OriginalCollisionMask;
		// Show animation.
		Graphic.StateMachine.ActionState = EActionState.REVIVE;
	}
	public void ChangeHealth(int amount) {
		bool _dead = Dead;
		CurrentHealth = Math.Clamp(CurrentHealth+amount, 0, MaxHealth);
		if (Dead != _dead) {
			if (Dead) {
				Die();
			} else {
				Revive();
			}
		}
	}
	private void OnDetectedBodyEnter(Node3D body) {
		if (body is Targettable) {
			var t = body as Targettable;
			if (DetectedFactions.Contains(t.GetTargetFaction())) {
				if (!detectedTargets.Contains(t)) detectedTargets.Add(t);
			}
		}
	}
	private void OnDetectedBodyExit(Node3D body) {
		if (body is Targettable) {
			var t = body as Targettable;
			if (detectedTargets.Contains(t)) detectedTargets.Remove(t);
		}
	}
}
