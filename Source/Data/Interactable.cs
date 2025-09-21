using Godot;
using System;

/// <summary>
/// Interface that all items that the player can interact with implement
/// </summary
public partial interface Interactable {
    /// <summary>
    /// Generic Active boolean used to toggle a singular generic state
    /// </summary>
    bool Active
        { get; set; }
    
    /// <summary
    /// Specific boolean meant to be used to check if an Interactable type is currenly available to be interacted with
    /// </summary>
    public bool CanInteract
        { get; set; }
        
    /// <summary>
    /// Boolean to check the state of an assigned interface within an object
    /// </summary>
    bool InterfaceVisible
        { get; set; }

    /// <summary>
    /// Processing of the player entering the Interactable's area
    /// </summary>
    /// <param>Node3D</param>
    public void _on_player_entered(Node3D body);

    /// <summary>
    /// Processing of the player leaving the Interactable's area
    /// </summary>
    /// <param>Node3D</param>
    public void _on_player_left(Node3D body);

    /// <summary>
    /// Return a Vector3 equivalent to the item's position
    /// </summary>
    /// <returns>Vector3</returns>
    public Vector3 GetItemPosition();

    /// <summary>
    /// Reveal the interactable item's prefered UI
    /// </summary>
    public void ShowInterface();

    /// <summary>
    /// Hide the interactable item's prefered UI
    /// </summary>
    public void HideInterface();

    /// <summary>
    /// Item Interaction processing
    /// </summary>
    public void InteractItem(string item = "empty");
}
