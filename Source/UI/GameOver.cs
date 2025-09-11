using Godot;
using System;

public partial class GameOver : Control
{
	[Export] AnimationPlayer Animation;
	public override void _Ready()
	{
		base._Ready();
		Animation.AnimationFinished += OnAnimationFinished;
	}
	public void Refresh() {
		Animation.Play("show");
	}
	void OnAnimationFinished(StringName animname) {
		Main.Instance.LoadIntroMap();
	}
}
