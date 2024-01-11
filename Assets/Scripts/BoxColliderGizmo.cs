using UnityEngine;
using System.Collections;

public class BoxColliderGizmo : MonoBehaviour {
	public Color color;

	void OnDrawGizmos()
	{
		Gizmos.matrix = Matrix4x4.TRS (transform.position, transform.rotation, transform.localScale);
		Gizmos.color = color;
		Gizmos.DrawLine (Vector3.zero, Vector3.forward);
		Gizmos.DrawCube (Vector3.zero, Vector3.one);
	}
}
