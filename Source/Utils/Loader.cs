using Godot;
using System;

public partial class Loader : Control
{
	[Signal] public delegate void OnShowFinishedEventHandler();
	[Signal] public delegate void OnLoadFinishedEventHandler(int v, Resource resource);
	[Export] AnimationPlayer Animation;
	string path;
	bool loading = false;
	bool waiting = false;
	public void LoadScene(string name) {
		path = name;
		loading = false;
		Visible = true;
		waiting = true;
		Animation.Play("show");
		if (name.Length != 0) {
			Error e = ResourceLoader.LoadThreadedRequest(path);
			if (e != Error.Ok) return;
			loading = true;
		}
	}
	public void HideLoader() {
		Animation.Play("hide");
	}
    public override void _Ready()
    {
		base._Ready();
        Visible = false;
		Animation.AnimationFinished += OnAnimationFinished;
    }
	private void OnAnimationFinished(StringName name) {
		if (name == "show") {
			waiting = false;
			EmitSignal(SignalName.OnShowFinished);
			if (!loading) EmitSignal(SignalName.OnLoadFinished, new Variant[]{0});
		}
		else if (name == "hide") {
			Visible = false;
		}
	}
    public override void _Process(double delta)
    {
		if (waiting) return;
        if (loading) {
			var s = ResourceLoader.LoadThreadedGetStatus(path);
			switch (s) {
				case ResourceLoader.ThreadLoadStatus.Failed:
				case ResourceLoader.ThreadLoadStatus.InvalidResource:
					loading = false;
					Animation.Play("hide");
					EmitSignal(SignalName.OnLoadFinished, new Variant[]{0});
					break;
				case ResourceLoader.ThreadLoadStatus.Loaded:
					loading = false;
					Animation.Play("hide");
					var resource = ResourceLoader.LoadThreadedGet(path);
					GD.Print("Loaded! ", resource);
					EmitSignal(SignalName.OnLoadFinished, new Variant[]{1,resource});
					break;
			}
		}
    }
}
