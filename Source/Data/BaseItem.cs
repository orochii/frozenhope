using Godot;

[GlobalClass]
public partial class BaseItem : Resource {
    // res://Data/items/glock.tres
    const string LOCATION = "res://Data/items/";
    const string EXTENSION = ".tres";
    [ExportCategory("Display")]
    [Export] public string DisplayName;
    [Export(PropertyHint.MultilineText)] public string DisplayDescription;
    [Export] public Texture2D Icon;
    [Export] public PackedScene Model;
    [ExportCategory("Inventory Properties")]
    [Export] public Vector2I SlotSize = new Vector2I(1,1);
    [Export(PropertyHint.Range,"1,255")] public int MaxStack = 1;
    //[ExportCategory("Combine")]
    // Define what to combine with, and result.
    //
    public string ID {
        get {
            var p = ResourcePath; 
            return p.Substring(LOCATION.Length, p.Length - LOCATION.Length - EXTENSION.Length);
        }
    }
    public virtual string GetItemName() { return DisplayName; }
    public virtual string GetDesc() { return DisplayDescription; }
    public static BaseItem Get(string id) {
        return OZResourceLoader.Load<BaseItem>(LOCATION + id + EXTENSION);
    }
}