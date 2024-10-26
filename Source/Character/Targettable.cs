using Godot;

public partial interface Targettable {
    public Vector3 GetPivotPosition();
    public Vector3 GetReticlePosition();
    //
    public bool CanBleed();
    public void Damage(EDamageType damageType, int damage);
}