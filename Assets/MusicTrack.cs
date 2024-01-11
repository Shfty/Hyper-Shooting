using UnityEngine;
using System.Collections;

public class MusicTrack : SoundEffect {

	public MusicTrack( string options ) : base( options ) { }

	public override void Play()
	{
		m_synth.parameters.masterVolume = m_defaultVolume * SaveManager.instance.SoundSettings.musicVolume * SaveManager.instance.SoundSettings.masterVolume;
		m_synth.Play ();
	}
}
