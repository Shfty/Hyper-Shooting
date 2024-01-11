using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SoundOptionsFunctions : MonoBehaviour {
	Slider m_masterVolumeSlider;
	Slider m_effectsVolumeSlider;
	Slider m_musicVolumeSlider;


	void Start()
	{
		m_masterVolumeSlider = GameObject.Find("Master Volume Slider").GetComponentInChildren<Slider> ();
		m_masterVolumeSlider.value = SaveManager.instance.SoundSettings.masterVolume * m_masterVolumeSlider.maxValue;

		m_effectsVolumeSlider = GameObject.Find("Effects Volume Slider").GetComponentInChildren<Slider> ();
		m_effectsVolumeSlider.value = SaveManager.instance.SoundSettings.effectsVolume * m_effectsVolumeSlider.maxValue;

		m_musicVolumeSlider = GameObject.Find("Music Volume Slider").GetComponentInChildren<Slider> ();
		m_musicVolumeSlider.value = SaveManager.instance.SoundSettings.musicVolume * m_musicVolumeSlider.maxValue;
	}
	
	public void MasterVolumeChanged()
	{
		SaveManager.instance.SoundSettings.masterVolume = m_masterVolumeSlider.value / m_masterVolumeSlider.maxValue;
		SaveManager.instance.Save ();
	}
	
	public void EffectsVolumeChanged()
	{
		SaveManager.instance.SoundSettings.effectsVolume = m_effectsVolumeSlider.value / m_effectsVolumeSlider.maxValue;
		SaveManager.instance.Save ();
	}
	
	public void MusicVolumeChanged()
	{
		SaveManager.instance.SoundSettings.musicVolume = m_musicVolumeSlider.value / m_musicVolumeSlider.maxValue;
		SaveManager.instance.Save ();
	}
}
