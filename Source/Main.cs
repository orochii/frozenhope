using Godot;
using System;

public partial class Main : Node
{
	public static Main Instance;
	[Export] public Node WorldParent;
	[Export] public UiParent UI;
	[Export] public Loader Loader;
	public Database Database;
	public GameState State;
	Node3D currentMap;
	public override void _Ready()
	{
		Instance = this;
		Database = Database.Get();
	}
	public void StartGame() {
		State = new GameState();
		//State.AddItem();
		foreach(var e in Database.StartingItems) {
			string ammoId = e.AmmoInside==null ? "" : e.AmmoInside.ID;
			State.AddItem(e.Item, e.Amount, ammoId, e.AmmoQty);
		}
		GD.Print(State.ListAllItems());
		State.SetEquippedItem(0);
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
			UI.SetUIMode(1);
        } else {
			// Set UI to title screen.
			UI.SetUIMode(0);
		}
        // Wait for a bit.
        var t = GetTree().CreateTimer(0.1);
        await ToSignal(t, Timer.SignalName.Timeout);
        // Show screen.
        Loader.HideLoader();
    }
}
