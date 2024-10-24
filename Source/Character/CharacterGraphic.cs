using Godot;
using System;

public partial class CharacterGraphic : Node3D
{
	[Export] AnimationTree Animator;
	[Export] public CharacterStateMachine StateMachine;
	[Export] Skeleton3D Armature;
	[Export] string WeaponBoneName;
	[Export] Node3D WeaponSlot;
	private bool initializedBones;
	private int weaponBoneIdx;
	private float move = 0;
	private float moveSpeed = 0;
	private Node3D currentWeaponModel;
	private PackedScene lastModelScene = null;
    //
	public void SetWeaponModel(PackedScene model) {
		if (model == lastModelScene) return;
		if (currentWeaponModel != null) {
			currentWeaponModel.QueueFree();
			currentWeaponModel = null;
		}
		if (model != null) {
			currentWeaponModel = model.Instantiate<Node3D>();
			WeaponSlot.AddChild(currentWeaponModel);
		}
		lastModelScene = model;
	}
	public void SetVariationId(string id) {
		StateMachine.VariationId = id;
	}
    public override void _Ready()
    {
        InitializeBones();
    }
    public override void _Process(double delta)
    {
        WeaponSlot.Transform = GetWeaponBonePose();
    }
    private void InitializeBones() {
		if (initializedBones) return;
		if (Armature != null) {
			for (int i = 0; i < Armature.GetBoneCount(); i++) {
				var name = Armature.GetBoneName(i);
				if (name.CompareTo(WeaponBoneName) == 0) {
					weaponBoneIdx = i;
					break;
				}
			}
		}
		initializedBones = true;
	}
    private Transform3D GetWeaponBonePose() {
		InitializeBones();
		if (Armature != null) {
			return Armature.GetBonePose(weaponBoneIdx);
		}
		return Transform;
	}
}
