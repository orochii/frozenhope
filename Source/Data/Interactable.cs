using Godot;
using System;

/// <summary>
/// Interface that all items that the player can interact with implement
/// </summary
public partial interface Interactable {
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
    public void InteractItem();
}
