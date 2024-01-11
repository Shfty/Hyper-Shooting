using UnityEngine;
using System.Collections;

public class ExplosionParticle : MonoBehaviour {
	public float ScaleRate = 0.9f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.localScale *= ScaleRate;

		if (transform.localScale.x < 0.01f) {
			Destroy( gameObject );
		}
	}
}
