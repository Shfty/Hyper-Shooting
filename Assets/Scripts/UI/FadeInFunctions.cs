using UnityEngine;
using System.Collections;

public class FadeInFunctions : MonoBehaviour {
	public void UpdateGraphicOptions() {
		GraphicsManager.instance.UpdateScreenSettings ();
		GraphicsManager.instance.UpdateQualitySettings ();
	}
}
