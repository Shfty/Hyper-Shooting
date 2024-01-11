using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class PostProcessManager : MonoBehaviour {
	Antialiasing m_aa;

	BlurOptimized m_blur;
	float m_currentBlurIntensity = 0.0f;
	float m_targetBlurIntensity = 0.0f;

	public static PostProcessManager instance;
	
	public float BlurLerpFactor = 0.1f;
	public float TargetBlurIntensity {
		get {
			return m_targetBlurIntensity;
		}
		set {
			m_targetBlurIntensity = value;
		}
	}

	// Use this for initialization
	void Start () {
		instance = this;

		m_aa = GetComponent<Antialiasing> ();

		m_blur = GetComponent<BlurOptimized> ();
	}
	
	// Update is called once per frame
	void Update () {
		// Update Anti-Aliasing
		if (SaveManager.instance.GraphicSettings.postProcessAntiAliasing == 0) {
			m_aa.enabled = false;
		} else {
			m_aa.enabled = true;
		}

		switch (SaveManager.instance.GraphicSettings.postProcessAntiAliasing) {
			case 1:
				m_aa.mode = AAMode.FXAA2;
				break;
			case 2:
				m_aa.mode = AAMode.FXAA3Console;
				break;
			case 3:
				m_aa.mode = AAMode.FXAA1PresetA;
				break;
			case 4:
				m_aa.mode = AAMode.FXAA1PresetB;
				break;
			case 5:
				m_aa.mode = AAMode.NFAA;
				break;
			case 6:
				m_aa.mode = AAMode.SSAA;
				break;
			case 7:
				m_aa.mode = AAMode.DLAA;
				break;
			default:
				break;
		}

		// Update Blur
		m_currentBlurIntensity = Mathf.Lerp (m_currentBlurIntensity, m_targetBlurIntensity, BlurLerpFactor);

		if ( m_currentBlurIntensity < 0.2f ) {
			m_blur.enabled = false;
		} else {
			m_blur.enabled = true;
			m_blur.blurIterations = Mathf.CeilToInt (m_currentBlurIntensity);
			m_blur.blurSize = 1.0f - (m_blur.blurIterations - m_currentBlurIntensity);
		}
	}
}
