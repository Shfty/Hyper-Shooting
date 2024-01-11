using UnityEngine;
using System.Collections;

public class SingleUseParticle : MonoBehaviour {
	ParticleSystem m_particleSystem;
	// Use this for initialization
	void Start () {
		m_particleSystem = GetComponent<ParticleSystem> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!m_particleSystem.isPlaying) {
			GameObject.Destroy( gameObject );
		}
	}
}
