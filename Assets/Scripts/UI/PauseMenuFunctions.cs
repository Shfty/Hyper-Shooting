using UnityEngine;
using System.Collections;

public class PauseMenuFunctions : MonoBehaviour {
	public void Quit()
	{
		Application.LoadLevel ("Title Menu");
	}
}
