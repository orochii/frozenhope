using Godot;
using System;

public partial class Enemy : CharacterBody3D, Targettable
{
	[Export] private CharacterGraphic Graphic;
	[Export] private Node3D TargetPivot;
	[Export] private int MaxHealth;
	private int CurrentHealth;
    public override void _Ready()
    {
        base._Ready();
		CurrentHealth = MaxHealth;
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
		GD.Print(Name," HEALTH:",CurrentHealth,"/",MaxHealth);
	}
	public void ChangeHealth(int amount) {
		CurrentHealth = Math.Clamp(CurrentHealth+amount, 0, MaxHealth);
	}
}
