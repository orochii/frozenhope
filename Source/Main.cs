using Godot;
using System;

public partial class Main : Node
{
	public static Main Instance;
	[Export] Node WorldParent;
	[Export] UiParent GameplayUI;
	[Export] Loader Loader;
	Database Database;
	GameState State;
	Node3D currentMap;
	public override void _Ready()
	{
		Instance = this;
		Database = Database.Get();
	}
	public void StartGame() {
		State = new GameState();
		//State.AddItem();
		foreach(var item in Database.StartingItems) {
			State.AddItem(item, 1);
		}
		GD.Print(State.ListAllItems());
		// Go to scene.
		ChangeMap(Database.StartingScene);
	}
	private string GetFullMapName(string baseName) {
		if (baseName.Length==0) return "";
        return string.Format("res://Scenes/maps/{0}.tscn", baseName);
    }
	public async void ChangeMap(string newMapName) {
        // Hide screen.
		
        Loader.LoadScene(GetFullMapName(newMapName));
        await Loader.ToSignal(Loader, Loader.SignalName.OnShowFinished);
        // Clear current map.
        if (currentMap != null) {
            // Deinstantiate current map.
            currentMap.QueueFree();
            currentMap = null;
        }
		// This is a dumb hack to deal with signals.
        var awaiter = await Loader.ToSignal(Loader, Loader.SignalName.OnLoadFinished);
        var newMapScene = (awaiter[0].AsInt32()==1) ? awaiter[1].As<PackedScene>() : null;
		// Instantiate new map if we received any.
        if (newMapScene != null) {
            State.MapName = newMapName;
            currentMap = newMapScene.Instantiate<Node3D>();
            WorldParent.AddChild(currentMap);
			// Set UI to gameplay mode.
			GameplayUI.SetUIMode(1);
        } else {
			// Set UI to title screen.
			GameplayUI.SetUIMode(0);
		}
        // Wait for a bit.
        var t = GetTree().CreateTimer(0.1);
        await ToSignal(t, Timer.SignalName.Timeout);
        // Show screen.
        Loader.HideLoader();
    }
}
