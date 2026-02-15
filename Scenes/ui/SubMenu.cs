using Godot;
using System;
using System.Transactions;

public partial class SubMenu : Control
{
    [Export] public Button UseButton;
    [Export] public Button CombineButton;
    [Export] public Button CheckButton;
    [Export] public Button MoveButton;
    public bool Active = false;
    public Vector2I GridPosition;
    public Inventory ParentInventory;
    private BaseItem _item;
    private int _index;
    private Inventory _parentInvetory;
    public InvSlotButton _lastFocus;
    [Signal]
    public delegate void sub_menu_closedEventHandler();

    public override void _Ready()
    {
        base._Ready();
        UseButton.Pressed += () => _on_use_button();
        CombineButton.Pressed += () => _on_combine_button();
        CheckButton.Pressed += () => _on_check_button();
        MoveButton.Pressed += () => _on_move_button();
    }

    public override void _Process(double delta)
    {

        /*if (Visible)
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
        }*/
    }


    public void MakeVisible(BaseItem item, int index, Inventory parentInventory, InvSlotButton slot)
    {
        _item = item;
        _index = index;
        _parentInvetory = parentInventory;
        _lastFocus = slot;
        if (_item is WeaponItem) UseButton.Text = "Equip";
        else UseButton.Text = "Use";
        FocusMode = FocusModeEnum.All;
        Visible = true;
        Active = true;
        UseButton.GrabFocus();
    }

    public void CloseSubMenu()
    {
        Visible = false;
        Active = false;
        _lastFocus.GrabFocus();
    }

    public void _on_use_button()
    {
        GD.Print("Use Button pressed");
    }

    public void _on_combine_button()
    {
        GD.Print("Combine Button pressed");
    }

    public void _on_check_button()
    {
        GD.Print("Use Check pressed");
    }

    public void _on_move_button()
    {
        GD.Print("Use Move pressed");
    }

}
