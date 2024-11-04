using Godot;

[GlobalClass]
public partial class WeaponItem : BaseItem {
    [Export] public string[] CompatibleAmmo;
    [Export] public int AmmoMax;
    [Export] public int AimTime;
    [Export] public PackedScene EquippedModel;
    [Export] public int WeaponBoneIdx;
    [Export] public string AnimationSet = "";
    // Action when not holding ammo
    public bool IsCompatibleWithAmmo(string id) {
        foreach (var a in CompatibleAmmo) {
            if (a.CompareTo(id) == 0) return true;
        }
        return false;
    }
}