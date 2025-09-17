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
                    //Temporary way to change an items fake description to real description
                    Main.Instance.State.SetSwitch(_item.DisplayName, true);
                    //Normal code processes
                    _parentInvetory.RefreshSlots();
                    _parentInvetory.InfoColumn.SetupDescription(_item);
                    AudioManager.PlaySystemSound("decision");
                    _parentInvetory.SubMenu.Visible = false;
                    //Let the parent know that the submenu has been closed
                    EmitSignal(SignalName.sub_menu_closed);
                    //Check if use of item closes the menu to interact with the environment
                    var interactable = Player.Instance.CloestInteractable;
                    if ((interactable is WorldScenery || interactable is Door) && interactable.CanInteract)
                    {
                        interactable.InteractItem(_item.DisplayName);
                        Main.Instance.UI.Gameplay.CloseMenu();
                    }
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
