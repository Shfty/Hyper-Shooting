using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GraphicsManager : MonoBehaviour {

	// Public Members
	public static GraphicsManager instance;
	
	public List<Vector2> supportedResolutions;
	public List<int> supportedRefreshRates;

	// Private Members
	SaveManager.GraphicSettingsClass m_settings;

	// Use this for initialization
	void Start () {
		instance = this;
		m_settings = SaveManager.instance.GraphicSettings;

		Resolution[] resolutions = Screen.resolutions;

		for (int i = 0; i < resolutions.Length; ++i) {
			Vector2 res = new Vector2( resolutions[i].width, resolutions[i].height );
			if(!supportedResolutions.Contains(res))
			{
				supportedResolutions.Add(res);
			}

			if(!supportedRefreshRates.Contains(resolutions[i].refreshRate))
			{
				supportedRefreshRates.Add( resolutions[i].refreshRate );
			}
		}
	}

	public void UpdateScreenSettings()
	{
		Vector2 targetRes = new Vector2( Screen.width, Screen.height );
		int targetRefreshRate = Screen.currentResolution.refreshRate;

		if (m_settings.resolution != -1 && m_settings.resolution < GraphicsManager.instance.supportedResolutions.Count) {
			targetRes = GraphicsManager.instance.supportedResolutions [m_settings.resolution];
		}

		if (m_settings.refreshRate != -1 && m_settings.refreshRate < GraphicsManager.instance.supportedRefreshRates.Count) {
			targetRefreshRate = GraphicsManager.instance.supportedRefreshRates [m_settings.refreshRate];
		}

		Screen.SetResolution( (int)targetRes.x, (int)targetRes.y, m_settings.fullscreen, targetRefreshRate );
		Application.targetFrameRate = targetRefreshRate;
	}

	public void UpdateQualitySettings()
	{
		// Set shadow resolution via quality level first to work around lack of proper interface
		QualitySettings.SetQualityLevel( m_settings.shadowResolution );
		
		QualitySettings.vSyncCount = m_settings.vsync ? 1 : 0;

		QualitySettings.masterTextureLimit = 3 - m_settings.textureQuality;

		switch (m_settings.anisotropicFiltering) {
		case 0:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			break;
		case 1:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
			break;
		case 2:
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
			break;
		default:
			break;
		}
		
		switch (m_settings.hardwareAntiAliasing) {
		case 0:
			QualitySettings.antiAliasing = 0;
			break;
		case 1:
			QualitySettings.antiAliasing = 2;
			break;
		case 2:
			QualitySettings.antiAliasing = 4;
			break;
		case 3:
			QualitySettings.antiAliasing = 8;
			break;
		default:
			break;
		}
		
		switch (m_settings.shadowProjection) {
		case 0:
			QualitySettings.shadowProjection = ShadowProjection.CloseFit;
			break;
		case 1:
			QualitySettings.shadowProjection = ShadowProjection.StableFit;
			break;
		default:
			break;
		}
		
		switch (m_settings.shadowQuality) {
		case 0:
			QualitySettings.shadowCascades = 0;
			break;
		case 1:
			QualitySettings.shadowCascades = 2;
			break;
		case 2:
			QualitySettings.shadowCascades = 4;
			break;
		default:
			break;
		}
	}
}
