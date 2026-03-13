using Godot;
using System;

public partial class BoxInventory : Control
{
    [Export] VBoxContainer verticalSlots = null;
    [Export] int itemBoxSize = 3;
    [Export] InvSlotButton slotTemplate = null;
    public InvSlotButton[] boxSlots;
    public Inventory inventorySibling;
    public Control BoxCombineObj;
    public TextureRect BoxCombineIcon;
    public InvSlotButton currentBoxCombineSlot;
    private InvSlotButton _transitCombine;
    public bool boxNavigate;

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
        SetupLoopNeighbors();
    }

    //Currently unused, slated for deletion
    public void SetupLoopNeighbors()
    {
        var size = itemBoxSize;
        boxSlots[0].FocusNeighborTop = boxSlots[size-1].GetPath();
        boxSlots[size-1].FocusNeighborBottom = boxSlots[0].GetPath();
    }

    public void RefreshBoxSlots()
    {
        
        var boxEntries = Main.Instance.State.GetBoxEntries();
        // Clear all slots
		foreach (var slot in boxSlots) {
			slot.Index = -1;
			slot.Setup(null);
		}

        var size = Math.Min(itemBoxSize, boxEntries.Count);
        for (int i = 0; i<size; i++)
        {
            var entry = boxEntries[i];
            GD.Print("Setup Box slot " + i+1);
            GD.Print("Entry is: " +  entry.itemID);
            GD.Print("Entry stack size is: " + entry.stackSize);
            boxSlots[i].Setup(entry);
            var str = boxSlots[i].Quantity.Text;
            GD.Print("Quantity in box: " + str);
        }
    }

    public void SetCombine(InvSlotButton button) {
		if (currentBoxCombineSlot != null) currentBoxCombineSlot.IsCombining(false);
		currentBoxCombineSlot = button;
		if (button == null || button.Item==null) {
			BoxCombineObj.Visible = false;
			currentBoxCombineSlot = null;
		}
		else {
			button.IsCombining(true);
			BoxCombineObj.Visible = true;
			// Set icon
			BoxCombineIcon.Texture = button.Item.Icon;
			// Resize container
			float sizeX = Math.Max(32, button.Item.SlotSize.X * 32);
			float sizeY = Math.Max(32, button.Item.SlotSize.Y * 32);
			// Set position
			BoxCombineObj.GlobalPosition = button.GlobalPosition;
		}
	}
	public InvSlotButton GetCombine() {
		return currentBoxCombineSlot;
	}

    public void FocusFirstSlot()
    {
        boxSlots[0].GrabFocus();
    }

    public void SetBoxNavigation(bool navigating)
    {
        boxNavigate = navigating;
    }

    public bool GetActiveState()
    {
        return Visible;
    }
}
