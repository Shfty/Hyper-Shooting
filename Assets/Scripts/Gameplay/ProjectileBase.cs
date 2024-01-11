using UnityEngine;
using System.Collections;

public class ProjectileBase : MonoBehaviour {
	public float Velocity = 30.0f;
	public int Damage = 75;

	Rigidbody m_rigidbody;

	// Use this for initialization
	void Start () {
		m_rigidbody = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	public void FixedUpdate () {
		m_rigidbody.velocity = transform.rotation * new Vector3( 0.0f, 0.0f, Velocity );
	}

	public void OnCollisionEnter( Collision col )
	{
		Transform trans = col.transform;
		if (trans.tag == "Enemy") {
			trans.GetComponent<EnemyBase>().TakeDamage( Damage );
		}

		GameObject.Destroy (gameObject);
	}
}
