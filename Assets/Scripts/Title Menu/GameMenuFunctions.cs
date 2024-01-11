using UnityEngine;
using System.Collections;

public class GameMenuFunctions : MonoBehaviour {
	public Animator MenuFSM;

	string m_stageToLoad = "";

	// Use this for initialization
	public void StartStage ( string stageName ) {
		m_stageToLoad = stageName;
		MenuFSM.CrossFade ("Game Menu -> Game", 0.0f);
	}

	void Update()
	{
		if (m_stageToLoad != "" && MenuFSM.GetCurrentAnimatorStateInfo (0).IsName ("Game")) {
			Application.LoadLevel (m_stageToLoad);
		}
	}
}
