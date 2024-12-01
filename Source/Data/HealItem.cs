using Godot;
using System;

public partial class HealItem : BaseItem {
    [Export] public Texture2D HealIcon;
    [Export] public int HealAmount;

    public override string GetDesc()
    {
        //string heal = string.Format("[img]{0}[/img]", HealIcon.ResourcePath);
        return string.Format(DisplayDescription);
    }
}
