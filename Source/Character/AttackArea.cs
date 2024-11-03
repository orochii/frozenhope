using System.Collections.Generic;
using Godot;

public partial class AttackArea : Area3D {
    public List<Targettable> BodiesInsideArea = new List<Targettable>();
    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }
    private void OnBodyEntered(Node3D body) {
        if (body is Targettable) {
            var t = body as Targettable;
            if (!BodiesInsideArea.Contains(t)) BodiesInsideArea.Add(t);
        }
    }
    private void OnBodyExited(Node3D body) {
        if (body is Targettable) {
            var t = body as Targettable;
            if (BodiesInsideArea.Contains(t)) BodiesInsideArea.Remove(t);
        }
    }
}