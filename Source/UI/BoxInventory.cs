using Godot;
using System;

public partial class BoxInventory : Control
{
    [Export] VBoxContainer verticalSlots = null;
    [Export] int itemBoxSize = 3;
    [Export] InvSlotButton slotTemplate = null;
    public InvSlotButton[] boxSlots;
    public Inventory inventorySibling;

    public override void _Ready()
    {
        base._Ready();
        Visible = false;
        slotTemplate.Visible = false;
        RefreshBoxGrid();
    }

    public void Refresh()
    {
        RefreshBoxGrid();
        RefreshBoxSlots();
    }

    public void RefreshBoxGrid()
    {
        if (boxSlots != null) foreach(var s in boxSlots) s.QueueFree();
        boxSlots = new InvSlotButton[itemBoxSize];
        for (int i=0; i<itemBoxSize; i++)
        {
            boxSlots[i] = slotTemplate.Duplicate() as InvSlotButton;
            boxSlots[i].Visible = true;
            boxSlots[i].Setup(null);
            boxSlots[i].Index = -1;
            boxSlots[i].GridPosition.Y = i;
            boxSlots[i].ParentBox = this;
            verticalSlots.AddChild(boxSlots[i]);
        }
    }

    public void RefreshBoxSlots()
    {
        var boxEntries = Main.Instance.State.GetBoxEntries();
        var size = Math.Min(itemBoxSize, boxEntries.Count);
        GD.Print("Box list size: " + size);
        for (int i = 0; i<size; i++)
        {
            var entry = boxEntries[i];
            boxSlots[i].Setup(entry);
        }
    }

    public bool GetActiveState()
    {
        return Visible;
    }
}
