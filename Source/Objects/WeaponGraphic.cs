using Godot;
using System;

public partial class WeaponGraphic : Node3D
{
	[Export] AnimationPlayer Animator;
	[Export] public AudioStreamPlayer SoundEffect;
	[Export] public Node3D SpawnPoint;
	public void Play(string animation) {
		Animator.Play(animation);
		SoundEffect.Play();
	}
}