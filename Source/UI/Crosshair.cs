using Godot;
using System;

public partial class Crosshair : Control
{
	public override void _Ready()
	{
		Visible = false;
	}
	public override void _Process(double delta)
	{
		if (IsInstanceValid(Player.Instance)) {
			var t = Player.Instance.CurrentTarget;
			if (t != null) {
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
