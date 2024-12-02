using Godot;
using System;
using System.Threading.Tasks;

public partial class MessageUI : Control
{
	[Signal] public delegate void OnMessageCloseEventHandler();
	[Export] RichTextLabel MessageLabel;
	[Export] Control Background;
	[Export] int LettersBySecond = 30;
	public int LastMode;
	private float visibleCharacters = 0;
	private float autocloseTimer = 0f;
	public void Refresh() {
		// Nothing for now I guess?
	}
	public async Task<bool> SetText(string text, bool showBackground, float autoclose=0f) {
		//
		if (Main.Instance.UI.Mode != (int)UiParent.EModes.MESSAGE) LastMode = Main.Instance.UI.Mode;
		Main.Instance.UI.SetUIMode((int)UiParent.EModes.MESSAGE);
		//
		MessageLabel.Text = "[center]"+text;
		MessageLabel.VisibleCharacters = 0;
		visibleCharacters = 0;
		autocloseTimer = autoclose;
		await ToSignal(this, SignalName.OnMessageClose);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		return true;
	}
	public async Task<bool> SetBars(bool v, float duration=0.5f) {
		//
		if (Main.Instance.UI.Mode != (int)UiParent.EModes.MESSAGE) LastMode = Main.Instance.UI.Mode;
		Main.Instance.UI.SetUIMode((int)UiParent.EModes.MESSAGE);
		//
		var startCol = v ? Colors.Transparent: Colors.White;
		var endCol = v ? Colors.White: Colors.Transparent;
		Background.Modulate = startCol;
		var tween = CreateTween().TweenProperty(Background, "modulate", endCol, duration);
		await ToSignal(tween, Tween.SignalName.Finished);
		//
		return true;
	}
	public void EndMessage() {
		Main.Instance.UI.SetUIMode(LastMode);
	}
    public override void _Process(double delta)
    {
        if (!IsVisibleInTree()) return;
		if (AllVisible) {
			// Show all text.
			MessageLabel.VisibleRatio = 1f;
			// Close window.
			var close = false;
			if (autocloseTimer > 0) {
				autocloseTimer -= (float)delta;
				if (autocloseTimer <= 0) close = true;
			} else if (Input.IsActionJustPressed("interact")) close = true;
			if (close) {
				// Close
				MessageLabel.Text = "";
				EmitSignal(SignalName.OnMessageClose);
				//Visible = false;
			}
		} else {
			// Advance letters.
			visibleCharacters += (float)delta * LettersBySecond;
			MessageLabel.VisibleCharacters = (int)visibleCharacters;
			// Fast forward.
			if (autocloseTimer <= 0 && Input.IsActionJustPressed("interact")) {
				visibleCharacters = MessageLabel.Text.Length;
			}
		}
    }
	public bool AllVisible {
		get {
			return visibleCharacters >= MessageLabel.Text.Length;
		}
	}
}
