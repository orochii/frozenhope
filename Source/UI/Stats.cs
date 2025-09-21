using Godot;
using System;

public partial class Stats : Control
{
	[ExportGroup("Components")]
	[Export] HealthIndicator Health;
	[Export] Label HealthLabel;
	[Export] TextureRect EquippedIcon;
	[Export] RichTextLabel AmmoLabel;
	[ExportGroup("Properties")]
	[Export] GradientTexture1D HealthColorGradient;
	private Image _cachedHealthGradient;
	public override void _Ready()
	{
		base._Ready();
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
		if (healthPerc == 0) Health.SpeedMult = 0; // at zero it does go to 0, I think the change is more evident like that.
		else Health.SpeedMult = 0.2f + (0.8f * healthPerc); // simple remapping of value from 0.2 to 1.0 (don't want speed at 0)
		Health.LineColor = SampleGradient(_cachedHealthGradient, healthPerc);
		// Set equipment icon.
		SetEquipIcon();
	}
	private void SetEquipIcon() {
		var equip = Main.Instance.State.GetEquippedItemEntry();
		if (equip != null)
		{
			var item = BaseItem.Get(equip.itemID);
			EquippedIcon.Texture = item.Icon;
			if (equip.ammoId.Length > 0)
			{
				var iconLine = "[img]res://Graphics/textures/icons/ammo_bullet.png[/img]";
				var ammoItem = BaseItem.Get(equip.ammoId) as AmmoItem;
				if (ammoItem != null && ammoItem.AmmoIcon != null)
				{
					iconLine = string.Format("[img]{0}[/img]", ammoItem.AmmoIcon.ResourcePath);
				}
				AmmoLabel.Text = iconLine + equip.ammoQty.ToString();
			}
			else
			{
				AmmoLabel.Text = "";
			}
		} else {
			EquippedIcon.Texture = null;
			AmmoLabel.Text = "";
		}
	}
	// Simple function to sample color from a gradient.
	private Color SampleGradient(Image g, float l)
	{
		int w = g.GetWidth();
		int x = (int)(w * l);
		if (x < 0) x = 0;
		if (x >= w) x = w - 1;
		return g.GetPixel(x, 0);
	}
}
