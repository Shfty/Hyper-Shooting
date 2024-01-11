using UnityEngine;
using System.Collections;

public class MenuState : StateMachineBehaviour {
	public string TargetMenu;
	public string ActivationTrigger;
	
	GameObject m_menu = null;
	UnityEngine.UI.Selectable m_firstSelectable = null;
	CanvasGroup m_canvasGroup = null;
	Animation m_anim = null;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (m_menu == null) {
			if (TargetMenu != "") {
				GameObject menu = GameObject.Find (TargetMenu);
				m_menu = menu;
				m_firstSelectable = m_menu.GetComponentInChildren<UnityEngine.UI.Selectable> ();
				m_canvasGroup = menu.GetComponentInChildren<CanvasGroup> ();
				m_anim = menu.GetComponentInChildren<Animation> ();
			}
		}

		if( m_menu != null )
		{
			string animName = "";
			if( animator.GetBool( ActivationTrigger ) )
			{
				animName = "Menu Inward Down";
			}
			else
			{
				animName = "Menu Outward Up";
			}
			
			if( animName != "" )
			{
				Animation anim = m_menu.GetComponentInChildren<Animation> ();

				Debug.Log ( m_menu );

				m_menu.GetComponent<MenuAnimation>().PlayAnimation( anim, animName, null );
			}
		}

		animator.SetBool( ActivationTrigger, false );
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (m_menu != null) {
			if (!m_anim.isPlaying) {
				if (!m_canvasGroup.blocksRaycasts) {
					m_canvasGroup.interactable = true;
					m_canvasGroup.blocksRaycasts = true;
					m_firstSelectable.Select ();
				}

				// Manually create cancel events
				if (InputManager.instance.PressInput.cancel) {
					animator.SetBool ("Cancel", true);
				}
			}
		}
	}
	
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (m_menu != null)
		{
			m_canvasGroup.interactable = false;
			m_canvasGroup.blocksRaycasts = false;
			
			string animName = "";
			if( animator.GetBool ( "Cancel" ) )
			{
				animName = "Menu Inward Up";
			}
			else
			{
				animName = "Menu Outward Down";
			}

			if( animName != "" )
			{
				Animation anim = m_menu.GetComponentInChildren<Animation> ();
				
				m_menu.GetComponent<MenuAnimation>().PlayAnimation( anim, animName, null );
			}
		}

		animator.SetBool( "Cancel", false );
	}
}
