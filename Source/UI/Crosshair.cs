using Godot;
using System;
using System.Transactions;

public partial class Crosshair : Control
{
	[Export] public Sprite2D StartAim;
	[Export] public Sprite2D EndAim;
	public int Time;
	public override void _Ready()
	{
		base._Ready();
		EndAim.Visible = false;
		Visible = false;
	}
	public override void _Process(double delta)
	{
		if (IsInstanceValid(Player.Instance)) {
			var t = Player.Instance.CurrentTarget;
			//If the Crosshair is not visible, assign the Player's equipped weapon AimTimer
			if (Visible == false) Time = Player.Instance.AimTimer;
			if (Player.Instance.AimTimer2 == -10) {
				Time = Player.Instance.AimTimer;
			}
			if (t != null) {
				//Reticle aim timer processing
				if (Time <= 0) {
					EndAim.Visible = true;
					Time = 0;
				} else {
					EndAim.Visible = false;
					Time -= 5;
				}
				var reticleWorldPosition = t.GetReticlePosition();
				var vpt = GetViewport();
				var cam = vpt.GetCamera3D();
				if (cam != null) {
					var screenPosition = cam.UnprojectPosition(reticleWorldPosition);
					screenPosition.X = Math.Clamp(screenPosition.X, 8, vpt.GetVisibleRect().Size.X-8);
					screenPosition.Y = Math.Clamp(screenPosition.Y, 8, vpt.GetVisibleRect().Size.Y-8);
					Position = screenPosition;
					Visible = !cam.IsPositionBehind(reticleWorldPosition);
					return;
				}
			}
		}
		Visible = false;
	}
}
