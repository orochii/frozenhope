using Godot;
using System;

public partial class Cutscene : Node3D
{
	[Export] AnimationPlayer Animation;
	[Export] string NextScene;
	public override void _Ready()
	{
		Animation.AnimationFinished += OnAnimationFinished;
	}
	public void Play(string animationId) {
		if (Animation.HasAnimation(animationId)) {
			Animation.Play(animationId);
		}
	}
	private void OnAnimationFinished(StringName v){
		// Go to next
		if (NextScene==null || NextScene.Length == 0) {
			Main.Instance.StartGame();
		}
		else {
			Main.Instance.ChangeMap(NextScene, Vector3.Zero, Vector3.Zero);
		}
	}
}
