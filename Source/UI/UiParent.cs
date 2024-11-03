using Godot;
using System;

public partial class UiParent : Control
{
	public enum EModes { TITLE, GAMEPLAY, MESSAGE, CUTSCENE, SPLASH }
	[Export] Control[] UIs;
	private int _mode;
	public int Mode => _mode;
	public override void _Ready()
	{
		SetUIMode((int)EModes.SPLASH);
	}
	public void SetUIMode(int idx) {
		_mode = idx;
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
	public MessageUI Message {
		get {
			foreach (var c in UIs) {
				if (c is MessageUI) return c as MessageUI;
			}
			return null;
		}
	}
	public GameplayUI Gameplay {
		get {
			foreach (var c in UIs) {
				if (c is GameplayUI) return c as GameplayUI;
			}
			return null;
		}
	}
}
