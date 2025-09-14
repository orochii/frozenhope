using Godot;

[GlobalClass]
public partial class UseableItem : BaseItem
{
    [Export] public string FakeName;
    [Export] public string RealName;
    [Export(PropertyHint.MultilineText)] public string RealDisplayDescription;

    public void ItemChecked()
    {
        var state = Main.Instance.State;
        if (!state.GetSwitch(DisplayName)) state.SetSwitch(DisplayName, true);
    }

    //#Overriden functions
    public override string GetItemName()
    {
        //If Switch is true, return this name, otherwise return normally
        if (Main.Instance.State.GetSwitch(DisplayName))
        {
            return RealName;
        }
        return FakeName;
    }

    public override string GetDesc()
    {
        //If Switch flag is true, return this description, otherwise return normally
        if (Main.Instance.State.GetSwitch(DisplayName))
        {
            return RealDisplayDescription;
        }
        return base.GetDesc();
    }
}