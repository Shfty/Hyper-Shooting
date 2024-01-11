using UnityEngine;
using System;
using System.Collections;

public class MenuAnimation : MonoBehaviour {
	public void PlayAnimation( Animation animation, string name, Action onComplete )
	{
		StartCoroutine( animation.Play(name, false, onComplete ) );
	}
}
