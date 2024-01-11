using UnityEngine;
using System.Collections;

public class BulletHoleFadeOut : MonoBehaviour {
	Projector m_projector;
	Color m_color = new Color( 0.0f, 0.0f, 0.0f, 1.0f );

	public float m_fadeRate = 0.01f;

	// Use this for initialization
	void Start () {
		m_projector = GetComponent<Projector> ();
		//m_color = m_projector.material.GetColor ("_Color");
	}
	
	// Update is called once per frame
	void Update () {
		m_color.a -= m_fadeRate;
		//m_projector.material.SetColor( "_Color", m_color );

		if ( m_color.a <= 0.0f)
		{
			GameObject.Destroy( gameObject );
		}
	}
}
