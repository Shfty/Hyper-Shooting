using UnityEngine;
using System.Collections;

public class PauseMenuUnpause : StateMachineBehaviour {
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (InputManager.instance.PressInput.pause) {
			animator.SetTrigger( "Cancel" );
		}
	}
}
