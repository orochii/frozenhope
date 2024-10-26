using Godot;
using System;

public partial class Stats : Control
{
	[ExportGroup("Components")]
	[Export] HealthIndicator Health;
	[Export] Label HealthLabel;
	[Export] TextureRect EquippedIcon;
	[ExportGroup("Properties")]
	[Export] GradientTexture1D HealthColorGradient;
	private Image _cachedHealthGradient;
	public override void _Ready()
	{
		// This is a costly operation so we're doing it only once.
		_cachedHealthGradient = HealthColorGradient.GetImage();
	}
	public override void _Process(double delta)
	{
		// 
		if (Main.Instance == null) return;
		if (Main.Instance.State == null) return;
		// Set visuals for health.
		HealthLabel.Text = Main.Instance.State.GetHealth().ToString();
		var healthPerc = Main.Instance.State.HealthPerc;
		if (healthPerc==0) Health.SpeedMult = 0; // at zero it does go to 0, I think the change is more evident like that.
		else Health.SpeedMult = 0.2f + (0.8f * healthPerc); // simple remapping of value from 0.2 to 1.0 (don't want speed at 0)
		Health.LineColor = SampleGradient(_cachedHealthGradient, healthPerc);
		// Set equipment icon.
		var equip = Main.Instance.State.GetEquippedItem();
		if (equip == null) EquippedIcon.Texture = null;
		else EquippedIcon.Texture = equip.Icon;
	}
	// Simple function to sample color from a gradient.
	private Color SampleGradient(Image g, float l) {
		int w = g.GetWidth();
		int x = (int)(w * l);
		if (x < 0) x = 0;
		if (x >= w) x = w-1;
		return g.GetPixel(x,0);
	}
}
