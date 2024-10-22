using Godot;

[GlobalClass]
public partial class AmmoItem : BaseItem {
    [Export] public Texture2D AmmoIcon;
    // Action when ammo is in weapon.
    public override string GetDesc()
    {
        string ammo = string.Format("[img]{0}[/img]", AmmoIcon.ResourcePath);
        return string.Format(DisplayDescription, ammo);
    }
}