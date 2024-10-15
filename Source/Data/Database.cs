using Godot;

[GlobalClass]
public partial class Database : Resource {
    [Export] public ItemAddEntry[] StartingItems;
    [Export] public string StartingScene;
    public static Database Get() {
        return OZResourceLoader.Load<Database>("res://Data/database.tres");
    }
}