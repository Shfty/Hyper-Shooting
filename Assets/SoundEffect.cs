using UnityEngine;
using System.Collections;

public class SoundEffect {

	string m_optionString;
	protected float m_defaultVolume;
	protected SfxrSynth m_synth = new SfxrSynth();

	public SoundEffect( string options )
	{
		m_synth.parameters.SetSettingsString (options);
		m_defaultVolume = m_synth.parameters.masterVolume;
		m_synth.CacheSound ();
	}

	public virtual void Play()
	{
		m_synth.parameters.masterVolume = m_defaultVolume * SaveManager.instance.SoundSettings.effectsVolume * SaveManager.instance.SoundSettings.masterVolume;
		m_synth.Play ();
	}
}
