using Godot;
using System;

public partial class Main : Node
{
	public static Main Instance;
	[Export] public Node WorldParent;
	[Export] public UiParent UI;
	[Export] public int StartScene = 0;
	[Export] public bool SkipIntro;
	public Loader Loader;
	public Database Database;
	public GameState State;
	Node3D currentScene;
	public Node3D CurrentScene => currentScene;
    public bool Busy;
	public Vector3 TransferVector = Vector3.Zero;
	public Vector3 TransferRotate = Vector3.Zero;

	//Functions start here
	public override void _Ready()
	{
		Instance = this;
		Database = Database.Get();
		Loader = UI.Loader;
	}
	public void StartGame() {
		State = new GameState();
		foreach(var e in Database.StartingItems) {
			State.AddItem(e);
		}
		State.SetEquippedItem(0);
		// Go to scene in position [StartScene]
		ChangeMap(Database.StartingScene[StartScene]);
	}
	private string GetFullMapName(string baseName) {
		if (baseName.Length==0) return "";
        return string.Format("res://Scenes/maps/{0}.tscn", baseName);
    }
	public async void ChangeMap(string newMapName) {
        // Hide screen.
		var sceneToLoad = GetFullMapName(newMapName);
		GD.Print("Loading ", sceneToLoad);
        Loader.LoadScene(sceneToLoad);
        await Loader.ToSignal(Loader, Loader.SignalName.OnShowFinished);
        // Clear current map.
        if (currentScene != null) {
            // Deinstantiate current map.
            currentScene.QueueFree();
            currentScene = null;
        }
		// This is a dumb hack to deal with signals.
        var awaiter = await Loader.ToSignal(Loader, Loader.SignalName.OnLoadFinished);
        var newMapScene = (awaiter[0].AsInt32()==1) ? awaiter[1].As<PackedScene>() : null;
		//We pause the game to get rid of pesky gravity based Player character bugs... kinda
		GetTree().Paused = true;
		// Instantiate new map if we received any.
        if (newMapScene != null) {
            if(State!=null) State.MapName = newMapName;
            currentScene = newMapScene.Instantiate<Node3D>();
            WorldParent.AddChild(currentScene);
			int uiMode = 1;
			if (currentScene is Cutscene) uiMode = (int)UiParent.EModes.CUTSCENE;
			if (currentScene.HasMeta("uiMode")) {
				uiMode = currentScene.GetMeta("uiMode").AsInt32();
			}
			if (uiMode >= 0) {
				// Set UI to gameplay mode.
				UI.SetUIMode(uiMode);
			}
        } else {
			// Set UI to title screen.
			UI.SetUIMode(0);
		}
        // Wait for a bit.
        var t = GetTree().CreateTimer(0.1);
        await ToSignal(t, Timer.SignalName.Timeout);
        // Show screen.
		GetTree().Paused = false;
        Loader.HideLoader();
    }
	

    public void LoadIntroMap() {
		if (SkipIntro) Main.Instance.StartGame();
		else ChangeMap(Database.IntroScene);
	}
}
