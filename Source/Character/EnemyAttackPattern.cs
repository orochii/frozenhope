using Godot;

[GlobalClass]
public partial class EnemyAttackPattern : Resource {
    [Export] string DebugName;
    [Export] public int AttackAreaIdx;
    [Export] public string AnimationVariationId;
    [Export] public int Damage;
    [Export] public float[] DamageFrames;
    [Export] public EActionState TargetActionState;
}