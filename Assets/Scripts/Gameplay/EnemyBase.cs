using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour {
	// Public Members
	public int Health = 100;

	// Utility Methods
	public void TakeDamage ( int num ) {
		Health -= num;

		if (Health <= 0) {
			GameObject.Destroy( gameObject );
		}
	}
}
