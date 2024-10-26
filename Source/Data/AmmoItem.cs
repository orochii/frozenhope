using Godot;

public enum EDamageType { NONE, BULLET, EXPLOSIVE }
[GlobalClass]
public partial class AmmoItem : BaseItem {
    [Export] public Texture2D AmmoIcon;
    [Export] public PackedScene Projectile;
    [Export] public int HitscanDamage;
    [Export] public EDamageType HitscanDamageType;
    [Export] public int HitscanShots;
    [Export] public float HitscanSpread;
    [Export] public PackedScene HitSparkObject; // 
    [Export] public PackedScene HitSparkBlood;
    // Action when ammo is in weapon.
    public override string GetDesc()
    {
        string ammo = string.Format("[img]{0}[/img]", AmmoIcon.ResourcePath);
        return string.Format(DisplayDescription, ammo);
    }
}