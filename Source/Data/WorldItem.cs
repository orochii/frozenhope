using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class WorldItem : StaticBody3D, Interactable
{
    [Export] ItemAddEntry Item;
    [Export] public Label3D Interface;
    [Export] public float InteractAngle = 45f;

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
    }

    public void ShowInterface() {
        if (!IsVisibleInTree()) return;
        Interface.Visible = true;
    }

    public void HideInterface() {
        Interface.Visible = false;
    }

    public async void InteractItem() {
        if (!IsVisibleInTree()) return;
        // Stop game
        Main.Instance.Busy = true;
        // Add item to inventory
        Main.Instance.State.AddItem(Item);
        //We remove the item (I suspect we'll need to add a permant removal flag)
        Main.Instance.State.SetSwitch(PickedUpFlag,true);
        Hide();
        // Assign and display text
        string str = string.Format("You found {0} {1}(s).", Item.Amount, Item.Item.DisplayName);
        await Main.Instance.UI.Message.SetText(str, false);
        // Call this on end of message, this just returns the UI mode back to whatever it was (usually gameplay).
        // Needed certain things from messages to stay, like the bars up/down for cool "in-level" cutscenes :vaccabayt:
        Main.Instance.UI.Message.EndMessage();
        // Unpause game
        Main.Instance.Busy = false;
    }
}
