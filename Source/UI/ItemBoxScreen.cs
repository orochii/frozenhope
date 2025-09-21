using Godot;
using System;

public partial class ItemBoxScreen : AInventoryScreen
{
    [Export] InventoryGrid InventoryGrid;
    [Export] public InfoColumn InfoColumn;
    public override void _Ready()
	{
		base._Ready();
		Visible = false;
	}
    public void Refresh()
    {
        InventoryGrid.RefreshGrid();
        InventoryGrid.RefreshSlots();
        InfoColumn.Setup(null);
        InventoryGrid.GrabFocus(0);
    }
    public override void ItemSelected(InvSlotButton invSlotButton)
    {
        //
    }
    public override void RefreshSlots()
    {
        //
    }
}
