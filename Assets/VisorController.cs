using UnityEngine;
using System.Collections;

public class VisorController : MonoBehaviour {
	public float OffsetFactorX = 5.0f;
	public float OffsetFactorY = 5.0f;
	public float OffsetLimitX = 5.0f;
	public float OffsetLimitY = 5.0f;
	public float OffsetLerpFactor = 0.4f;
	public float HeadbobXFactor = 0.0f;
	public float HeadbobYFactor = 10.0f;
	public float HeadbobLerpFactor = 0.4f;
	
	public HSMovementFSM MovementFSM;
	public RectTransform VisorH;
	public RectTransform VisorV;

	public Vector2 m_offsetPosition;
	public Vector3 m_targetPosition;

	// Use this for initialization
	void Start () {
		m_targetPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 lookVector;
		lookVector.x = -InputManager.instance.CurrentInput.look.x * OffsetFactorX;
		lookVector.y = InputManager.instance.CurrentInput.look.y * OffsetFactorY;

		m_offsetPosition += lookVector;
		m_offsetPosition.x = Mathf.Clamp (m_offsetPosition.x, -OffsetLimitX, OffsetLimitX);
		m_offsetPosition.y = Mathf.Clamp (m_offsetPosition.y, -OffsetLimitY, OffsetLimitY);

		m_offsetPosition = Vector2.Lerp (m_offsetPosition, Vector2.zero, OffsetLerpFactor);

		m_targetPosition = m_offsetPosition + new Vector2( MovementFSM.CameraHeadbob.x * HeadbobXFactor, MovementFSM.CameraHeadbob.y * HeadbobYFactor );
		VisorH.localPosition = Vector2.Lerp (VisorH.localPosition, m_targetPosition, HeadbobLerpFactor);
	}
}
