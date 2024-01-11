using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameOptionsFunctions : MonoBehaviour {
	Toggle m_toggleLockOnToggle;


	void Start()
	{
		m_toggleLockOnToggle = GameObject.Find("Toggle Lock-On").GetComponentInChildren<Toggle> ();
		m_toggleLockOnToggle.isOn = SaveManager.instance.GameSettings.toggleLockOn;
	}

	public void ToggleLockOnChanged()
	{
		SaveManager.instance.GameSettings.toggleLockOn = m_toggleLockOnToggle.isOn;
		SaveManager.instance.Save ();
	}
}
