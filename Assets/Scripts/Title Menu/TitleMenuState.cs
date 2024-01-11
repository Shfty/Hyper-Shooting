using UnityEngine;
using System.Collections;

public class TitleMenuState : StateMachineBehaviour {
	SoundEffect m_soundActivateTitle = new SoundEffect( "3,.15,,.5,.5,.3,.3,.075,,-.1,.39,,,,,,,,,,,,,,,.9,-.075,,,,," );

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		UnityEngine.EventSystems.EventSystem.current.GetComponent<UnityEngine.EventSystems.HSInputModule>().DeactivateModule();
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (InputManager.instance.PressInput.select || InputManager.instance.PressInput.fire) {
			m_soundActivateTitle.Play();
			animator.SetTrigger( "Activate Title Menu" );
		}
	}
}
