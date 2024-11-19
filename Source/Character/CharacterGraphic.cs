using Godot;
using System;

[Tool]
public partial class CharacterGraphic : Node3D
{
	[Export] Skeleton3D Armature;
	[Export] string[] WeaponBoneNames;
	[Export] Node3D WeaponSlot;
	[Export] private int CurrentWeaponBoneIdx;
	[Export] bool ReplicateInEditor;
	private bool initializedBones;
	private int[] weaponBoneIdxs = new int[0];
	private float move = 0;
	private float moveSpeed = 0;
	private Node3D currentWeaponModel;
	private PackedScene lastModelScene = null;
	public CharacterStateMachine StateMachine;
    //
	public Node3D GetWeaponSpawnPoint() {
		if (currentWeaponModel != null) {
			if (currentWeaponModel is WeaponGraphic) {
				var weapon = currentWeaponModel as WeaponGraphic;
				return weapon.SpawnPoint;
			}
		}
		return currentWeaponModel;
	}
	public void SetWeaponAnimation(string animId) {
		if (currentWeaponModel != null) {
			if (currentWeaponModel is WeaponGraphic) {
				var weapon = currentWeaponModel as WeaponGraphic;
				weapon.Play(animId);
			}
		}
	}
	public void SetWeaponModel(PackedScene model, int idx=0) {
		CurrentWeaponBoneIdx = idx;
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
		if (!Engine.IsEditorHint()) {
			foreach (var c in GetChildren()) {
				if (c is CharacterStateMachine) StateMachine = c as CharacterStateMachine;
			}
		}
        InitializeBones();
    }
    public override void _Process(double delta)
    {
		if (Engine.IsEditorHint() && !ReplicateInEditor) return;
		if (WeaponBoneNames == null || WeaponBoneNames.Length == 0) return;
		if (WeaponSlot != null) WeaponSlot.Transform = GetWeaponBonePose(CurrentWeaponBoneIdx);
    }
    private void InitializeBones() {
		if (initializedBones && weaponBoneIdxs.Length == WeaponBoneNames.Length) return;
		if (WeaponBoneNames == null || WeaponBoneNames.Length == 0) return;
		weaponBoneIdxs = new int[WeaponBoneNames.Length];
		if (Armature != null) {
			for (int j = 0; j < WeaponBoneNames.Length; j++) {
				for (int i = 0; i < Armature.GetBoneCount(); i++) {
					var name = Armature.GetBoneName(i);
					if (name.CompareTo(WeaponBoneNames[j]) == 0) {
						weaponBoneIdxs[j] = i;
						break;
					}
				}
			}
			
		}
		initializedBones = true;
	}
    private Transform3D GetWeaponBonePose(int idx=0) {
		InitializeBones();
		if (Armature != null) {
			return Armature.GetBonePose(weaponBoneIdxs[idx]);
		}
		return Transform;
	}
}
