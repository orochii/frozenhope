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
    public bool Combining = false;
    public bool Moving = false;
    public Vector2I GridPosition;
    public Inventory ParentInventory;
    private BaseItem _item;
    private int _index;
    private Inventory _parentInventory;
    public InvSlotButton _currentSlot;
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
        
    }

    /// <summary>
    /// Make the sub menu visible and grant it focus. Pass it the item, index, parentinventory as well as the slot
    /// that called it.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="index"></param>
    /// <param name="parentInventory"></param>
    /// <param name="slot"></param>
    public void MakeVisible(BaseItem item, Inventory parentInventory, InvSlotButton slot)
    {
        _item = item;
        _parentInventory = parentInventory;
        _currentSlot = slot;
        if (_item is WeaponItem) UseButton.Text = "Equip";
        else UseButton.Text = "Use";
        FocusMode = FocusModeEnum.All;
        Visible = true;
        Active = true;
        UseButton.GrabFocus();
    }

    /// <summary>
    /// Close the SubMenu and return focus to the last slot that was selected before calling the SubMenu.
    /// </summary>
    public void CloseSubMenu()
    {
        Visible = false;
        Active = false;
        _currentSlot.GrabFocus();
    }

    public void DeactivateSubMenu()
    {
        if (Combining) Combining = false;
		if (Moving) Moving = false;
    }

    public void ResetCursor()
    {
        _currentSlot.GrabFocus();
    }

    /// <summary>
    /// Checks if the SubMenu has any processes currently active for inventory interactions and returns whether
    /// the SubMenu is currently in use or not.
    /// </summary>
    /// <returns>The status of any SubMenu activity</returns>
    public bool GetSubMenuStatus()
    {
        if (Active) return Active;
        if (Combining) return Combining;
        if (Moving) return Moving;
        return false;
    }

    /// <summary>
    /// Signal invocated method:
    /// use the currently selected item.
    /// </summary>
    public void _on_use_button()
    {
        GD.Print("Use Button pressed");
        switch (_item)
        {
            case WeaponItem:
                _currentSlot.EquipItem();
                CloseSubMenu();
                break;
            case UseableItem:
                CloseSubMenu();
                if (_currentSlot.UseItem()) Main.Instance.UI.Gameplay.CloseMenu();
                break;
            case HealItem:
                _currentSlot.UseItem();
                if (_currentSlot.UseHealItem()) CloseSubMenu();
                break;
        }
    }

    /// <summary>
    /// Signal invoked method:
    /// 
    /// </summary>
    public void _on_combine_button()
    {
        GD.Print("Combine Button pressed");
        _currentSlot.CombineItem();
        CloseSubMenu();
        Combining = true;
    }

    /// <summary>
    /// Signal invoked method:
    /// Check the currently selected item.
    /// </summary>
    public void _on_check_button()
    {
        GD.Print("Use Check pressed");
        _currentSlot.CheckItem();
        CloseSubMenu();
    }

    /// <summary>
    /// Signal invoked method:
    /// 
    /// </summary>
    public void _on_move_button()
    {
        GD.Print("Use Move pressed");
        _currentSlot.MoveItem();
        CloseSubMenu();
        Moving = true;
    }
}
