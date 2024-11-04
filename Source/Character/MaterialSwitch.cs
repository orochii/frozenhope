using Godot;
using System;

public partial class MaterialSwitch : MeshInstance3D
{
	const float FLAP_DURATION = 0.1f;
	[Export] public int MaterialOverrideIdx;
	[Export] public int SurfaceOverrideIdx;
	[Export] Material[] AlternateMaterials;
	private int prevMaterialOverrideIdx;
	private bool isFlapping;
	private float flappingTimer;
	public void SetFlapping() {
		isFlapping = true;
		MaterialOverrideIdx = 1;
		flappingTimer = FLAP_DURATION;
	}
	public void UnsetFlapping() {
		isFlapping = false;
		MaterialOverrideIdx = 0;
		flappingTimer = 0;
	}
    public override void _Process(double delta)
    {
        // Update flapping.
		if (isFlapping) {
			flappingTimer -= (float)delta;
			if (flappingTimer < 0) {
				MaterialOverrideIdx = MaterialOverrideIdx != 0 ? 0 : 1;
				flappingTimer = FLAP_DURATION;
			}
		}
		// Update current material.
		if (prevMaterialOverrideIdx != MaterialOverrideIdx) {
			if (MaterialOverrideIdx >= 0 && MaterialOverrideIdx < AlternateMaterials.Length) {
				var mat = AlternateMaterials[MaterialOverrideIdx];
				SetSurfaceOverrideMaterial(SurfaceOverrideIdx, mat);
			}
			prevMaterialOverrideIdx = MaterialOverrideIdx;
		}
    }
}
