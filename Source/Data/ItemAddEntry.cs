using Godot;

[GlobalClass]
public partial class ItemAddEntry : Resource {
    [Export] public BaseItem Item;
    [Export] public int Amount;
    [Export] public BaseItem AmmoInside;
    [Export] public int AmmoQty=0;
}