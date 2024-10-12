using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;

public static class UIUtils {
    public static void SetupGridList(Control[] array, int columns) {
        for (int i = 0; i < array.Length; i++) {
            var curr = array[i];
            var t = i - columns >= 0             ? array[i - columns] : curr;
            var b = i + columns < array.Length  ? array[i + columns] : curr;
            var l = i - 1       >= 0             ? array[i - 1] : curr;
            var r = i + 1       < array.Length  ? array[i + 1] : curr;
            curr.FocusNeighborTop =     t.GetPath();
            curr.FocusNeighborBottom =  b.GetPath();
            curr.FocusNeighborLeft =    l.GetPath();
            curr.FocusNeighborRight =   r.GetPath();
        }
    }
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