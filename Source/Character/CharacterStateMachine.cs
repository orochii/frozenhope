using Godot;
using System;

public partial class CharacterStateMachine : Node
{
	[Export] AnimationPlayer Animator;
	[Export] CharacterAnimState[] AnimStates;
	[Export] public EMoveState MoveState;
	[Export] public EModeState ModeState;
	[Export] public EActionState ActionState;
	[Export] public string VariationId = "";
	CharacterAnimState _lastState = null;
    public override void _Ready()
    {
		if (Engine.IsEditorHint()) return;
        if (Animator != null) Animator.AnimationFinished += OnAnimationFinished;
    }
    public override void _Process(double delta)
    {
		if (Engine.IsEditorHint()) return;
        float blend = _lastState == null ? 0 : 0.1f;
        var currentState = GetCurrentState();
        GoToState(currentState, blend);
    }
    private void GoToState(CharacterAnimState currentState, float blend)
    {
        if (_lastState != currentState)
        {
            var stateVariation = currentState.Get(VariationId);
            var currentAnimationId = stateVariation.AnimationId;
            if (stateVariation.PlayBackwards) {
				if (Animator != null) Animator.PlayBackwards(currentAnimationId, blend);
			}
            else {
				if (Animator != null) Animator.Play(currentAnimationId, blend);
			}
        }
        _lastState = currentState;
    }
    private void OnAnimationFinished(StringName animationName) {
		if (ActionState != EActionState.NONE) {
			var state = GetCurrentState();
			var variation = state.Get(VariationId);
			if (variation.AnimationId != animationName) return;
			if (state.ActionState == ActionState) {
				if (!state.IgnoreActionExit) {
					ActionState = EActionState.NONE;
					var currentState = GetCurrentState();
					GoToState(currentState, 0);
				}
			}
		}
	}
	private CharacterAnimState GetCurrentState() {
		// Idea here is: pick a state that doesn't fully 
		CharacterAnimState altState = null;
		foreach (var state in AnimStates) {
			var valid = IsStateValid(state);
			if (valid==1) return state;
			else if (valid==2) altState = state;
		}
		if (altState != null) return altState;
		return AnimStates[0];
	}
	private int IsStateValid(CharacterAnimState state) {
		// It is imperative that the action state is correct.
		if (state.ActionState != ActionState) return 0;
		// And this for some that can use alternates.
		int retVal = 1;
		if (state.MoveState != MoveState) retVal = 2;
		if (state.ModeState != ModeState) retVal = 2;
		return retVal;
	}
}
