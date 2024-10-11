using Godot;
using System;

public partial class UiParent : Control
{
	[Export] Control[] UIs;
	public override void _Ready()
	{
		SetUIMode(0);
	}
	public void SetUIMode(int idx) {
		for (int i = 0; i < UIs.Length; i++) {
			// Set UI with right index as active. Hide the rest.
			var ui = UIs[i];
			ui.Visible = idx == i;
			if (ui.Visible) {
				// Through reflection, we can call a method called Refresh with no parameters.
				var m = ui.GetType().GetMethod("Refresh");
				if (m != null) m.Invoke(ui, null);
			}
		}
	}
}
