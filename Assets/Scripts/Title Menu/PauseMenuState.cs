using UnityEngine;
using System.Collections;

public class PauseMenuState : StateMachineBehaviour {
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		UnityEngine.EventSystems.EventSystem.current.GetComponent<UnityEngine.EventSystems.HSInputModule>().DeactivateModule();
#if !UNITY_EDITOR
		LockCursor ();
#endif
		PostProcessManager.instance.TargetBlurIntensity = 0.0f;
		Time.timeScale = 1.0f;
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (InputManager.instance.PressInput.pause) {
			animator.SetTrigger( "Activate Pause Menu" );
		}

#if UNITY_EDITOR
		if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None) {
			LockCursor ();
		}
#endif
	}
	
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
#if !UNITY_EDITOR
		UnlockCursor ();
#endif
		PostProcessManager.instance.TargetBlurIntensity = 3.0f;
		Time.timeScale = 0.0f;
	}

	void LockCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	
	void UnlockCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
}
