using Godot;
using System;

public partial class DmgPopUp : Control
{
	[Export] Label DamageLabel;
	[Export] AnimationPlayer Animator;
	[Export] Color HealingColor;
	private Node3D _target = null;
	private Vector3 _offset;
	public void Setup(Node3D target, Vector3 offset, int damage) {
		_target = target;
		_offset = offset;
		if (damage < 0) {
			damage = -damage;
			DamageLabel.SelfModulate = HealingColor;
		}
		DamageLabel.Text = string.Format("{0}", damage);
	}
	public override void _Ready()
	{
		Animator.AnimationFinished += OnAnimationFinished;
	}

	private void OnAnimationFinished(StringName name) {
		_target = null;
	}
	public override void _Process(double delta)
	{
		if (IsInstanceValid(_target) && _target != null) {
			var destinationPos = _target.GlobalPosition + _offset;
			var cam = GetViewport().GetCamera3D();
			Position = cam.UnprojectPosition(destinationPos);
		}
		else {
			QueueFree();
		}
	}
}
