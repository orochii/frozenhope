using Godot;
using System;

public partial class WeaponGraphic : Node3D
{
	[Export] AnimationPlayer Animator;
	[Export] public Node3D SpawnPoint;
	[ExportGroup("Sound Effects")]
	[Export] public AudioStreamPlayer Fire;
	[Export] public AudioStreamPlayer Action;
	[Export] public AudioStreamPlayer Reload;
	
	public void Play(string animation) {
		if (Animator != null) Animator.Play(animation);
		if (Fire != null) Fire.Play();
	}
}
