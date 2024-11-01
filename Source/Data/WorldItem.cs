using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class WorldItem : StaticBody3D, Interactable
{
    [Export] public BaseItem Container;
    [Export] public int Amount;
    [Export] public Label3D Interface;
    private string PickedUpFlag {
        get {
            string basePath = Main.Instance.WorldParent.GetPath();
            string fullPath = GetPath();
            return fullPath.Substr(basePath.Length, fullPath.Length-basePath.Length);
        }
    }
    public override void _Ready() {
        if (Main.Instance.State.GetSwitch(PickedUpFlag)) {
            Hide();
        }
        Interface.Visible = false;
        GD.Print("Read: Hide Interface");
    }

    public void ShowInterface() {
        if (!IsVisibleInTree()) return;
        GD.Print("Show Interface");
        Interface.Visible = true;
    }

    public void HideInterface() {
        GD.Print("Hide Interface");
        Interface.Visible = false;
    }

    public void InteractItem() {
        if (!IsVisibleInTree()) return;
        //Add to inventory
        Main.Instance.State.AddItem(Container,Amount);
        //We remove the item (I suspect we'll need to add a permant removal flag)
        Main.Instance.State.SetSwitch(PickedUpFlag,true);
        Hide();
    }
}
