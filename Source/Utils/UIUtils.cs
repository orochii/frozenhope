using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;

public static class UIUtils {
    public static void SetupVBoxList(Control[] array) {
        for (int i = 0; i < array.Length; i++) {
            var curr = array[i];
            curr.FocusNeighborLeft = curr.GetPath();
            curr.FocusNeighborRight = curr.GetPath();
            if (i > 0) curr.FocusNeighborTop = array[i-1].GetPath();
            else curr.FocusNeighborTop = curr.GetPath();
            if (i < array.Length-1) curr.FocusNeighborBottom = array[i+1].GetPath();
            else curr.FocusNeighborBottom = curr.GetPath();
        }
    }
    public static void SetupHBoxList(Control[] array) {
        for (int i = 0; i < array.Length; i++) {
            var curr = array[i];
            curr.FocusNeighborTop = curr.GetPath();
            curr.FocusNeighborBottom = curr.GetPath();
            if (i > 0) curr.FocusNeighborLeft = array[i-1].GetPath();
            else curr.FocusNeighborLeft = curr.GetPath();
            if (i < array.Length-1) curr.FocusNeighborRight = array[i+1].GetPath();
            else curr.FocusNeighborRight = curr.GetPath();
        }
    }
}