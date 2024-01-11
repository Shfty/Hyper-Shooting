using UnityEngine;
using System.Collections;

public class SkyboxCamera : MonoBehaviour {
	Camera m_camera = null;
	Camera m_playerCamera = null;

	public Transform PlayerCamera = null;

	void Start() {
		m_camera = GetComponent<Camera> ();
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (PlayerCamera != null) {
			if( m_playerCamera == null )
			{
				m_playerCamera = PlayerCamera.GetComponent<Camera>();
			}

			transform.rotation = PlayerCamera.rotation;
			m_camera.fieldOfView = m_playerCamera.fieldOfView;
		}
	}
}
