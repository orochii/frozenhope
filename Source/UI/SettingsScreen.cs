using Godot;
using System;

public partial class SettingsScreen : Control
{
    [Export] Control FirstSelection;
    Control lastFocused;
    public override void _Process(double delta)
    {
        if (!IsVisibleInTree()) return;
        var focused = GetViewport().GuiGetFocusOwner();
        if (lastFocused != focused)
        {
            AudioManager.PlaySystemSound("cursor");
            lastFocused = focused;
        }
    }
    public void Refresh()
    {
        FirstSelection.GrabFocus();
        lastFocused = FirstSelection;
    }
    public void OnResetDefaults()
    {
        Main.Settings.Reset();
    }
    public void OnClose()
    {
        Main.Instance.UI.CloseSettings();
    }
}
