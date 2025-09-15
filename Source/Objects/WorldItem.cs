using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class WorldItem : Area3D, Interactable
{
    [Export] ItemAddEntry Item;
    [Export] public Label3D Interface;
    [Export] public float InteractAngle = 45f; //Currently unused
    [Export] public Camera3D ItemCamera;
    private Player _playerCharacter;
    public bool Active
        { get; set; }
    public bool InterfaceVisible
        { get; set; }
    private string PickedUpFlag {
        get {
            string basePath = Main.Instance.WorldParent.GetPath();
            string fullPath = GetPath();
            return fullPath.Substr(basePath.Length, fullPath.Length-basePath.Length);
        }
    }
    
    //Overriden Ready function
    public override void _Ready()
    {
        base._Ready();
        if (Main.Instance.State.GetSwitch(PickedUpFlag))
        {
            QueueFree();
        }
        HideInterface();
    }
    
    public void _on_player_entered(Node3D body)
    {
        _playerCharacter = (Player)body;
        _playerCharacter.NearbyInteractables.Add(this);
    }

    public void _on_player_left(Node3D body)
    {
        _playerCharacter.NearbyInteractables.Remove(this);
        Active = false;
        HideInterface();
    }

    public override void _Process(double delta)
    {
        if (Active) ShowInterface();
        else HideInterface();
    }

    //Interface Functions
    public Vector3 GetItemPosition(){
        return GlobalPosition;
    }

    public void ShowInterface() {
        if (!IsVisibleInTree()) return;
        Interface.Visible = true;
        InterfaceVisible = true;
    }

    public void HideInterface() {
        Interface.Visible = false;
        InterfaceVisible = false;
    }

    public async void InteractItem() {
        if (!IsVisibleInTree()) return;
        // Stop game
        Main.Instance.Busy = true;
        // Add item to inventory (We will need to add an exception case where inventory is full or not all ammo can be picked up)
        Main.Instance.State.AddItem(Item);
        // Assign and display text
        await Main.Instance.UI.Message.SetBars(true, 0.1f);
        string str = "";
        switch (Item.Item)
        {
            case AmmoItem:
                str = string.Format("You found {0} {1}(s).", Item.Amount, Item.Item.DisplayName);
                break;
            case UseableItem:
                var item = Item.Item as UseableItem;
                str = string.Format("You found {0}.", item.FakeName);
                break;
        }
        await Main.Instance.UI.Message.SetText(str, false);
        // Call this on end of message, this just returns the UI mode back to whatever it was (usually gameplay).
        // Needed certain things from messages to stay, like the bars up/down for cool "in-level" cutscenes :vaccabayt:
        await Main.Instance.UI.Message.SetBars(false, 0.1f);
        Main.Instance.UI.Message.EndMessage();
        //We remove the item (I suspect we'll need to add a permanent removal flag)
        Main.Instance.State.SetSwitch(PickedUpFlag,true);
        Hide();
        // Unpause game
        Main.Instance.Busy = false;
        QueueFree();
    }

    //Overridden ToString
    public override string ToString() {
        return Name;
    }
}
