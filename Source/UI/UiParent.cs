using Godot;
using System;

public partial class UiParent : Control
{
	public enum EModes { TITLE, GAMEPLAY, MESSAGE, CUTSCENE, SPLASH, GAMEOVER }
	public enum EMenus { INVENTORY, BOX, FILES, MAP, SKILLS, MENU };
	[Export] Control[] UIs;
	[Export] public Loader Loader;
	public EMenus menuMode;
	private int _mode;
	public int Mode => _mode;
	public override void _Ready()
	{
		base._Ready();
		SetUIMode((int)EModes.SPLASH);
		SetMenuState(EMenus.INVENTORY);
	}
	public void SetUIMode(EModes mode) {
		SetUIMode((int)mode);
	}
	public void SetUIMode(int idx) {
		_mode = idx;
		for (int i = 0; i < UIs.Length; i++) {
			// Set UI with right index as active. Hide the rest.
			var ui = UIs[i];
			ui.Visible = idx == i;
			if (ui.Visible) {
				// Through reflection, we can call a method called Refresh with no parameters.
				var method = ui.GetType().GetMethod("Refresh");
				if (method != null) method.Invoke(ui, null);
			}
		}
	}

	public void SetMenuState(EMenus mode)
	{
		if (menuMode != mode) menuMode = mode;
	}
	public EMenus GetMenuState()
	{
		return menuMode;
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
