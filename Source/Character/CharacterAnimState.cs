using Godot;
using System;
using System.Collections.Generic;

/*
MoveState: STAND,WALK,RUN
ActionState: IDLE,AIMING,ATTACK
Variation: *held weapon animation id* (pistol, rifle, etc)
*/
public enum EMoveState { STAND, WALK, RUN }
public enum EModeState { IDLE, AIMING }
public enum EActionState { NONE, ATTACK, DEATH, REVIVE, DAMAGE, KNOCKED_DOWN }

[GlobalClass]
public partial class CharacterAnimState : Resource
{
	//
	[Export] public EMoveState MoveState;
	[Export] public EModeState ModeState;
	[Export] public EActionState ActionState;
	[Export] public bool IgnoreActionExit;
	[Export] CharacterAnimStateEntry[] AnimEntries;
	private Dictionary<string,CharacterAnimStateEntry> animEntriesDictionary;
	public CharacterAnimStateEntry Get(string id) {
		// Create and cache entries if it's not done.
		if (animEntriesDictionary == null) {
			animEntriesDictionary = new Dictionary<string, CharacterAnimStateEntry>();
			foreach (var e in AnimEntries) animEntriesDictionary.Add(e.Id, e);
		}
		// 
		if (animEntriesDictionary.TryGetValue(id, out var ee)) {
			return ee;
		}
		// Return first as default.
		return AnimEntries[0];
	}
}
