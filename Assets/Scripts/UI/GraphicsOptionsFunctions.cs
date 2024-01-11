using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GraphicsOptionsFunctions : MonoBehaviour {
	SelectList m_resolutionSelect;
	SelectList m_refreshRateSelect;
	Toggle m_vsyncToggle;
	Toggle m_fullscreenToggle;
	Slider m_fieldOfViewSlider;
	SelectList m_textureQualitySelect;
	SelectList m_anisotropySelect;
	SelectList m_hardwareAASelect;
	SelectList m_shadowResolutionSelect;
	SelectList m_shadowProjectionSelect;
	SelectList m_shadowQualitySelect;
	SelectList m_postProcessAASelect;

	void Start()
	{
		m_resolutionSelect = GameObject.Find("Resolution Select").GetComponentInChildren<SelectList> ();

		List<Vector2> resolutions = GraphicsManager.instance.supportedResolutions;
		string[] resolutionNames = new string[resolutions.Count];
		for (int i = 0; i < resolutionNames.Length; ++i) {
			resolutionNames[i] = resolutions[i].x + "x" + resolutions[i].y;
		}

		m_resolutionSelect.keys = resolutionNames;
		m_resolutionSelect.value = SaveManager.instance.GraphicSettings.resolution;
		
		m_refreshRateSelect = GameObject.Find("Refresh Rate Select").GetComponentInChildren<SelectList> ();
		
		List<int> refreshRates = GraphicsManager.instance.supportedRefreshRates;
		string[] refreshRateNames = new string[refreshRates.Count];
		for (int i = 0; i < refreshRateNames.Length; ++i) {
			refreshRateNames[i] = refreshRates[i].ToString();
		}
		
		m_refreshRateSelect.keys = refreshRateNames;
		m_refreshRateSelect.value = SaveManager.instance.GraphicSettings.refreshRate;

		m_vsyncToggle = GameObject.Find("VSync Toggle").GetComponentInChildren<Toggle> ();
		m_vsyncToggle.isOn = SaveManager.instance.GraphicSettings.vsync;
		
		m_fullscreenToggle = GameObject.Find("Fullscreen Toggle").GetComponentInChildren<Toggle> ();
		m_fullscreenToggle.isOn = SaveManager.instance.GraphicSettings.fullscreen;
		
		m_fieldOfViewSlider = GameObject.Find("Field Of View").GetComponentInChildren<Slider> ();
		m_fieldOfViewSlider.value = SaveManager.instance.GraphicSettings.fieldOfView;

		m_textureQualitySelect = GameObject.Find("Texture Quality").GetComponentInChildren<SelectList> ();
		m_textureQualitySelect.value = SaveManager.instance.GraphicSettings.textureQuality;
		
		m_anisotropySelect = GameObject.Find("Anisotropic Filtering").GetComponentInChildren<SelectList> ();
		m_anisotropySelect.value = SaveManager.instance.GraphicSettings.anisotropicFiltering;
		
		m_hardwareAASelect = GameObject.Find("Hardware Anti-Aliasing").GetComponentInChildren<SelectList> ();
		m_hardwareAASelect.value = SaveManager.instance.GraphicSettings.hardwareAntiAliasing;
		
		m_shadowResolutionSelect = GameObject.Find("Shadow Resolution").GetComponentInChildren<SelectList> ();
		m_shadowResolutionSelect.value = SaveManager.instance.GraphicSettings.shadowResolution;
		
		m_shadowProjectionSelect = GameObject.Find("Shadow Projection").GetComponentInChildren<SelectList> ();
		m_shadowProjectionSelect.value = SaveManager.instance.GraphicSettings.shadowProjection;
		
		m_shadowQualitySelect = GameObject.Find("Shadow Quality").GetComponentInChildren<SelectList> ();
		m_shadowQualitySelect.value = SaveManager.instance.GraphicSettings.shadowQuality;
		
		m_postProcessAASelect = GameObject.Find("Post-Process Anti-Aliasing").GetComponentInChildren<SelectList> ();
		m_postProcessAASelect.value = SaveManager.instance.GraphicSettings.postProcessAntiAliasing;
	}
	
	public void ResolutionSelected()
	{
		SaveManager.instance.GraphicSettings.resolution = m_resolutionSelect.value;
		SaveManager.instance.Save ();
		GraphicsManager.instance.UpdateScreenSettings ();
	}
	
	public void RefreshRateChanged()
	{
		SaveManager.instance.GraphicSettings.refreshRate = m_refreshRateSelect.value;
		SaveManager.instance.Save ();
		GraphicsManager.instance.UpdateScreenSettings ();
	}
	
	public void VsyncChanged()
	{
		SaveManager.instance.GraphicSettings.vsync = m_vsyncToggle.isOn;
		SaveManager.instance.Save ();
		GraphicsManager.instance.UpdateQualitySettings ();
	}
	
	public void FullscreenChanged()
	{
		SaveManager.instance.GraphicSettings.fullscreen = m_fullscreenToggle.isOn;
		SaveManager.instance.Save ();
		GraphicsManager.instance.UpdateScreenSettings ();
	}
	
	public void FieldOfViewChanged()
	{
		SaveManager.instance.GraphicSettings.fieldOfView = (int)m_fieldOfViewSlider.value;
		SaveManager.instance.Save ();
	}
	
	public void TextureQualityChanged()
	{
		SaveManager.instance.GraphicSettings.textureQuality = m_textureQualitySelect.value;
		SaveManager.instance.Save ();
		GraphicsManager.instance.UpdateQualitySettings ();
	}
	
	public void AnisotropicFilteringChanged()
	{
		SaveManager.instance.GraphicSettings.anisotropicFiltering = m_anisotropySelect.value;
		SaveManager.instance.Save ();
		GraphicsManager.instance.UpdateQualitySettings ();
	}
	
	public void HardwareAntiAliasingChanged()
	{
		SaveManager.instance.GraphicSettings.hardwareAntiAliasing = m_hardwareAASelect.value;
		SaveManager.instance.Save ();
		GraphicsManager.instance.UpdateQualitySettings ();
	}
	
	public void ShadowResolutionChanged()
	{
		SaveManager.instance.GraphicSettings.shadowResolution = m_shadowResolutionSelect.value;
		SaveManager.instance.Save ();
		GraphicsManager.instance.UpdateQualitySettings ();
	}
	
	public void ShadowProjectionChanged()
	{
		SaveManager.instance.GraphicSettings.shadowProjection = m_shadowProjectionSelect.value;
		SaveManager.instance.Save ();
		GraphicsManager.instance.UpdateQualitySettings ();
	}
	
	public void ShadowQualityChanged()
	{
		SaveManager.instance.GraphicSettings.shadowQuality = m_shadowQualitySelect.value;
		SaveManager.instance.Save ();
		GraphicsManager.instance.UpdateQualitySettings ();
	}
	
	public void PostProcessAntiAliasingChanged()
	{
		SaveManager.instance.GraphicSettings.postProcessAntiAliasing = m_postProcessAASelect.value;
		SaveManager.instance.Save ();
	}
}
