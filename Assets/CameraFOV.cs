using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraFOV : MonoBehaviour {
	Camera m_camera = null;

	float m_baseFOV;

	// Use this for initialization
	void Start () {
		m_camera = GetComponent<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
		m_camera.fieldOfView = SaveManager.instance.GraphicSettings.fieldOfView;
	}
}
