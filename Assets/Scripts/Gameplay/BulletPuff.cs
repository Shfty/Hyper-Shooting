using UnityEngine;
using System.Collections;

public class BulletPuff : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.position += new Vector3( 0.0f, 0.01f, 0.0f );
		transform.localScale *= 0.9f;

		if (transform.localScale.x < 0.01f) {
			Destroy( gameObject );
		}
	}
}
