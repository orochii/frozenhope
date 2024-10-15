using Godot;

[GlobalClass]
public partial class WeaponItem : BaseItem {
    [Export] public string[] CompatibleAmmo;
    [Export] public int AmmoMax;
    // Action when not holding ammo
    public bool IsCompatibleWithAmmo(string id) {
        foreach (var a in CompatibleAmmo) {
            if (a.CompareTo(id) == 0) return true;
        }
        return false;
    }
}