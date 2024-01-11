using UnityEngine;
using System.Collections;

public class ExplosionBase : MonoBehaviour {
	public float Radius = 3.75f;
	public int Damage = 40;
	public float ScaleRate = 0.75f;

	SfxrSynth m_soundExplosion = new SfxrSynth();

	// Use this for initialization
	public void Start () {
		GetComponent<SphereCollider> ().radius = Radius;
		transform.Find ( "Mesh" ).localScale *= Radius * 2.0f;

		m_soundExplosion.parameters.SetSettingsString ("3,.2,,.25,.42,.5,.3,.094,,-.03,,,,,,,,,,,,,,,,1,,,,,,");
		m_soundExplosion.CacheSound ();
		m_soundExplosion.Play ();
	}

	public void FixedUpdate () {
		transform.Find ( "Mesh" ).localScale -= new Vector3( ScaleRate, ScaleRate, ScaleRate );

		if (transform.Find ( "Mesh" ).localScale.x < 0.0f) {
			GameObject.Destroy( gameObject );
		}
	}

	public void OnTriggerEnter( Collider col )
	{
		if (col.tag == "Enemy") {
			RaycastHit enemyHitInfo = new RaycastHit();
			bool enemyHit = Physics.Raycast( transform.position, col.transform.position - transform.position, out enemyHitInfo );

			if( enemyHit && enemyHitInfo.transform.tag == "Enemy" )
			{
				col.GetComponent<EnemyBase>().TakeDamage( Damage );
			}
		}
	}
}
