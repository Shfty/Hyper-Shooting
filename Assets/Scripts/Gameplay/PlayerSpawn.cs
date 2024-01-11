using UnityEngine;
using System.Collections;

public class PlayerSpawn : MonoBehaviour {
	public Transform PlayerController;
	public Transform Skybox;
	public Color Color;

	SkyboxCamera m_skyboxCamera;

	// Use this for initialization
	void Start () {
		Transform pc = (Transform)Instantiate (PlayerController, transform.position, transform.rotation);
		HSMovementFSM movementScript = FindObjectOfType<HSMovementFSM>();
		movementScript.RegisterSpawnPoint(this.gameObject);

		m_skyboxCamera = Skybox.FindChild ("Camera").GetComponent<SkyboxCamera>();
		m_skyboxCamera.PlayerCamera = pc.Find ("Turn Wrapper").Find ("Camera Wrapper").Find ("First Person Camera");
	}

	void OnDrawGizmos()
	{
		float xz = PlayerController.FindChild( "Normal Collider" ).GetComponent<CapsuleCollider> ().radius;
		float y = PlayerController.FindChild( "Normal Collider" ).GetComponent<CapsuleCollider> ().height;
		Gizmos.matrix = Matrix4x4.TRS (transform.position + new Vector3( 0.0f, y * 0.5f, 0.0f ), transform.rotation, transform.localScale);
		Gizmos.color = Color;
		Gizmos.DrawLine (Vector3.zero, Vector3.forward);
		Gizmos.DrawWireCube (Vector3.zero, new Vector3( xz, y, xz ) );
	}

}
