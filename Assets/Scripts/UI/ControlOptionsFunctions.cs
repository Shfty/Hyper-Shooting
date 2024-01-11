using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ControlOptionsFunctions : MonoBehaviour {
	Slider m_mouseSensitivitySlider;
	Slider m_analogSensitivitySlider;


	void Start()
	{
		m_mouseSensitivitySlider = GameObject.Find("Mouse Aim Sensitivity").GetComponentInChildren<Slider> ();
		m_mouseSensitivitySlider.value = SaveManager.instance.ControlSettings.mouseSensitivity * m_mouseSensitivitySlider.maxValue;

		m_analogSensitivitySlider = GameObject.Find("Controller Aim Sensitivity").GetComponentInChildren<Slider> ();
		m_analogSensitivitySlider.value = SaveManager.instance.ControlSettings.analogSensitivity;
	}
	
	public void MouseSensitivityChanged()
	{
		SaveManager.instance.ControlSettings.mouseSensitivity = m_mouseSensitivitySlider.value / m_mouseSensitivitySlider.maxValue;
		SaveManager.instance.Save ();
	}
	
	public void AnalogSensitivityChanged()
	{
		SaveManager.instance.ControlSettings.analogSensitivity = m_analogSensitivitySlider.value;
		SaveManager.instance.Save ();
	}
}
