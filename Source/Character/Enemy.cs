using Godot;
using System;

public partial class Enemy : CharacterBody3D, Targettable
{
	[Export] private CharacterGraphic Graphic;
	[Export] private Node3D TargetPivot;
	[Export] private int MaxHealth;
	private int CurrentHealth;
	private uint OriginalCollisionLayer;
	private uint OriginalCollisionMask;
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
	public void Die() {
		// Disable movement/collision.
		CollisionLayer = 0;
		CollisionMask = 0;
		// Show animation.
		Graphic.StateMachine.ActionState = CharacterAnimState.EActionState.DEATH;
	}
	public void Revive() {
		// Disable movement/collision.
		CollisionLayer = OriginalCollisionLayer;
		CollisionMask = OriginalCollisionMask;
		// Show animation.
		Graphic.StateMachine.ActionState = CharacterAnimState.EActionState.REVIVE;
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
}
