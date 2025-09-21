using Godot;

public abstract partial class AInventoryScreen : Control
{
    public virtual void ItemSelected(InvSlotButton invSlotButton) { }
    public virtual void RefreshSlots() { }
    public virtual void SetCombine(InvSlotButton button) { }
}