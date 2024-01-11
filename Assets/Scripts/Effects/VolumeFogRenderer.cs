using UnityEngine;
using System.Collections;

public class VolumeFogRenderer : MonoBehaviour {
	// Editor Settings
	[System.Serializable]
	public class Settings
	{
		public Shader fogShader = null;
		public Material fogMaterial = null;
		public RenderTexture debugRenderTexture = null;
	}
	public Settings m_settings = new Settings();

	Camera m_camera;
	RenderTexture m_renderTexture;

	// Use this for initialization
	void Start () {
		m_camera = GetComponent<Camera> ();

		if (m_settings.debugRenderTexture != null) {
			m_renderTexture = m_settings.debugRenderTexture;
		} else {
			m_renderTexture = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.RFloat);
			m_renderTexture.Create ();
		}
	}
	
	// Update is called one per frame
	void Update () {
		CameraClearFlags currentClearFlags = m_camera.clearFlags;
		m_camera.clearFlags = CameraClearFlags.Color;

		RenderingPath currentRenderingPath = m_camera.renderingPath;
		m_camera.renderingPath = RenderingPath.Forward;

		m_camera.targetTexture = m_renderTexture;

		m_camera.RenderWithShader (m_settings.fogShader, "VolumeFog");

		m_camera.targetTexture = null;

		m_camera.renderingPath = currentRenderingPath;

		m_camera.clearFlags = currentClearFlags;

		m_settings.fogMaterial.SetTexture ("_BaseTexture", m_renderTexture);
	}
}
