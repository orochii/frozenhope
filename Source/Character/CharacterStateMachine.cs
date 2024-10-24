using Godot;
using System;

public partial class CharacterStateMachine : Node
{
	[Export] AnimationPlayer Animator;
	[Export] CharacterAnimState[] AnimStates;
	[Export] public CharacterAnimState.EMoveState MoveState;
	[Export] public CharacterAnimState.EModeState ModeState;
	[Export] public CharacterAnimState.EActionState ActionState;
	[Export] public string VariationId = "";
    public override void _Ready()
    {
        Animator.AnimationFinished += OnAnimationFinished;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
		var a = GetCurrentAnimation();
		if (a != Animator.CurrentAnimation) {
			Animator.CurrentAnimation = a;
			Animator.Play(a);
		}
    }
    private void OnAnimationFinished(StringName animationName) {
		if (ActionState != CharacterAnimState.EActionState.NONE) {
			var state = GetCurrentState();
			var variation = state.Get(VariationId);
			if (variation.AnimationId != animationName) return;
			if (state.ActionState == ActionState) {
				ActionState = CharacterAnimState.EActionState.NONE;
				Animator.CurrentAnimation = GetCurrentAnimation();
			}
		}
	}
    public string GetCurrentAnimation() {
		var state = GetCurrentState();
		var variation = state.Get(VariationId);
		return variation.AnimationId;
	}
	private CharacterAnimState GetCurrentState() {
		// Idea here is: pick a state that doesn't fully 
		CharacterAnimState altState = null;
		foreach (var state in AnimStates) {
			var valid = IsStateValid(state);
			if (valid==1) return state;
			else if (valid==2) altState = state;
		}
		if (altState==null) return altState;
		return AnimStates[0];
	}
	private int IsStateValid(CharacterAnimState state) {
		// It is imperative that the movestate is correct.
		if (state.MoveState != MoveState) return 0;
		if (state.ActionState != ActionState) return 0;
		// And this for some that can use alternates.
		int retVal = 1;
		if (state.ModeState != ModeState) retVal = 2;
		return retVal;
	}
}
