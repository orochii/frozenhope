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
		AudioManager.Init();
		DirAccess.MakeDirRecursiveAbsolute(SnapPath());
		Database = Database.Get();
		Loader = UI.Loader;
	}
    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey) {
			var key = evt as InputEventKey;
			if (key.Pressed && key.Keycode == Key.F9) {
				TakeScreenshot();
			}
			if (evt.IsActionPressed("sys_fullscreen")) {
				ToggleFullscreen();
			}
		}
    }
	private string SnapPath() {
		return "user://Snap/";
	}
	public void ToggleFullscreen() {
		var mode = DisplayServer.WindowGetMode();
		if (mode == DisplayServer.WindowMode.Fullscreen) SetFullscreen(false);
		else SetFullscreen(true);
	}
	public void SetFullscreen(bool v) {
		if (v)
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		else
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
	}
	public void TakeScreenshot() {
		ulong timestamp = (ulong)(Time.GetUnixTimeFromSystem() * 1000);
		var name = SnapPath() + timestamp.ToString() + ".png";
		var img = GetViewport().GetTexture().GetImage();
		img.SavePng(name);
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
	
	/// <summary>
	/// Transfers the user to the "NewMapName" scene and then Move and Rotate the player based on boolean parameters.
	/// </summary>
	/// <param name="newMapName"></param>
	/// <param name="MovePlayer"></param>
	/// <param name="RotatePlayer"></param>
	public async void ChangeMap(string newMapName, bool MovePlayer = false, bool RotatePlayer = false)
	{
		// Hide screen.
		var sceneToLoad = GetFullMapName(newMapName);
		GD.Print("Loading ", sceneToLoad);
		Loader.LoadScene(sceneToLoad);
		await Loader.ToSignal(Loader, Loader.SignalName.OnShowFinished);
		// Clear current map.
		if (currentScene != null)
		{
			// Deinstantiate current map.
			currentScene.QueueFree();
			currentScene = null;
		}
		// This is a dumb hack to deal with signals.
		var awaiter = await Loader.ToSignal(Loader, Loader.SignalName.OnLoadFinished);
		var newMapScene = (awaiter[0].AsInt32() == 1) ? awaiter[1].As<PackedScene>() : null;
		//We pause the game to get rid of pesky gravity based Player character bugs... kinda
		GetTree().Paused = true;
		// Instantiate new map if we received any.
		if (newMapScene != null)
		{
			if (State != null) State.MapName = newMapName;
			currentScene = newMapScene.Instantiate<Node3D>();
			WorldParent.AddChild(currentScene);
			/* Search for the player node within the scene tree and move and rotate them based
			on NewPlayerPosition and NewPlayerRotation. If these values aren't given, do nothing.
			*/
			Player PlayerNode = GetPlayerNode(currentScene);
			if (PlayerNode != null)
			{
				if (MovePlayer) PlayerNode.GlobalPosition = TransferVector;
				if (RotatePlayer) PlayerNode.GlobalRotation = TransferRotate;
				PlayerNode.FreezeStatus();
			}
			//We set the UI Mode
			int uiMode = 1;
			if (currentScene is Cutscene) uiMode = (int)UiParent.EModes.CUTSCENE;
			if (currentScene.HasMeta("uiMode"))
			{
				uiMode = currentScene.GetMeta("uiMode").AsInt32();
			}
			if (uiMode >= 0)
			{
				// Set UI to gameplay mode.
				UI.SetUIMode(uiMode);
			}
		}
		else
		{
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

	//We search for the player Node inside of a given scene and return it
	public Player GetPlayerNode(Node3D Scene){
		if (Scene == null)
			return Instance.FindChild("Player") as Player;
			
		var _playerNode = Scene.FindChild("Player") as Player;
		if (_playerNode != null)
			GD.Print("Player Node Found");
		else
			GD.Print("Player Node not found");
		return _playerNode;
	}
}
