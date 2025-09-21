using Godot;

[GlobalClass]
public partial class BaseItem : Resource
{
    // res://Data/items/glock.tres
    const string LOCATION = "res://Data/items/";
    const string EXTENSION = ".tres";
    [ExportCategory("Display")]
    [Export] public string DisplayName;
    [Export(PropertyHint.MultilineText)] public string DisplayDescription;
    [Export] public Texture2D Icon;
    [Export] public PackedScene Model;
    [ExportCategory("Inventory Properties")]
    [Export] public Vector2I SlotSize = new Vector2I(1, 1);
    [Export(PropertyHint.Range, "1,255")] public int MaxStack = 1;
    //[ExportCategory("Combine")]
    // Define what to combine with, and result.
    //
    public string ID
    {
        get
        {
            var path = ResourcePath;
            return path.Substring(LOCATION.Length, path.Length - LOCATION.Length - EXTENSION.Length);
        }
    }
    //Fetches the name of the item
    public virtual string GetItemName() { return DisplayName; }
    //Fetches the description of the item
    public virtual string GetDesc() { return DisplayDescription; }
    //Fetches the item from the database by its path
    public static BaseItem Get(string id)
    {
        return OZResourceLoader.Load<BaseItem>(LOCATION + id + EXTENSION);
    }
}