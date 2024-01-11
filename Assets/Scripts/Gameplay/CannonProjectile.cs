using UnityEngine;
using System.Collections;

public class CannonProjectile : ProjectileBase {
	public GameObject CannonExplosion;
	public GameObject ExplosionParticle;
	public GameObject CannonBurnProjector;

	bool m_spawnedExplosion = false;

	new void OnCollisionEnter( Collision col )
	{

		if (!m_spawnedExplosion) {
			Vector3 pos = col.contacts[0].point + col.contacts[0].normal * 0.01f;
			GameObject.Instantiate( CannonExplosion, pos, Quaternion.identity );
			GameObject.Instantiate( ExplosionParticle, pos, Quaternion.FromToRotation(Vector3.forward, col.contacts[0].normal) );
			GameObject.Instantiate( CannonBurnProjector, pos, Quaternion.FromToRotation(Vector3.forward, -col.contacts[0].normal) );
			m_spawnedExplosion = true;
		}
		base.OnCollisionEnter (col);
	}
}
