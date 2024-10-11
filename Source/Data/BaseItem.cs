using Godot;

[GlobalClass]
public partial class BaseItem : Resource {
    [Export] public string DisplayName;
    [Export(PropertyHint.MultilineText)] public string DisplayDescription;
    [Export] public Texture2D Icon;
    [Export] public Vector2I SlotSize = new Vector2I(1,1);
    [Export(PropertyHint.Range,"1,255")] public int MaxStack = 1;
    //
    public string ID {
        get {
            return this.ResourcePath;
        }
    }
    public static BaseItem Get(string id) {
        return OZResourceLoader.Load<BaseItem>(id);
    }
}