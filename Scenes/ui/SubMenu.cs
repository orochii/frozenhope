using Godot;
using System;

public partial class SubMenu : Control
{
    public Vector2I GridPosition;
    public Inventory ParentInventory;
    private BaseItem _item;
    private int _index;
    private Inventory _parentInvetory;
    [Signal]
    public delegate void sub_menu_closedEventHandler();

    public override void _Process(double delta)
    {
        if (Visible)
        {
            switch (_item)
            {
                case WeaponItem:
                    Main.Instance.State.SetEquippedItem(_index);
                    Player.Instance.RefreshEquippedModel();
                    _parentInvetory.RefreshSlots();
                    AudioManager.PlaySystemSound("decision");
                    _parentInvetory.SubMenu.Visible = false;
                    EmitSignal(SignalName.sub_menu_closed);
                    break;
                case UseableItem:
                    Main.Instance.State.SetSwitch(_item.DisplayName, true);
                    _parentInvetory.RefreshSlots();
                    _parentInvetory.InfoColumn.SetupDescription(_item);
                    AudioManager.PlaySystemSound("decision");
                    _parentInvetory.SubMenu.Visible = false;
                    EmitSignal(SignalName.sub_menu_closed);
                    Main.Instance.UI.Gameplay.CloseMenu();
                    Player.Instance.UsedItem = _item.DisplayName;
                    break;
            }
        }
    }


    public void MakeVisible(BaseItem item, int index, Inventory parentInventory)
    {
        _item = item;
        _index = index;
        _parentInvetory = parentInventory;
        FocusMode = FocusModeEnum.All;
        Visible = true;
    }

}
