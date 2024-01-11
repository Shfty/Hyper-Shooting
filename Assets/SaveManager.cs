using UnityEngine;
using System.Collections;

public class SaveManager : MonoBehaviour {
	public class GameSettingsClass
	{
		public bool toggleLockOn = false;
	}
	
	public class ControlSettingsClass
	{
		public float mouseSensitivity = 0.25f;
		public float analogSensitivity = 6.0f;
	}
	
	public class GraphicSettingsClass
	{
		public int resolution = -1;
		public int refreshRate = -1;
		public bool vsync = true;
		public bool fullscreen = true;
		public int fieldOfView = 90;
		public int textureQuality = 3;
		public int anisotropicFiltering = 2;
		public int hardwareAntiAliasing = 3;
		public int shadowResolution = 3;
		public int shadowProjection = 1;
		public int shadowQuality = 2;
		public int postProcessAntiAliasing = 3;
	}
	
	public class SoundSettingsClass
	{
		public float masterVolume = 1.0f;
		public float effectsVolume = 1.0f;
		public float musicVolume = 1.0f;
	}

	// Public Members
	public static SaveManager instance;
	public GameSettingsClass GameSettings = new GameSettingsClass();
	public ControlSettingsClass ControlSettings = new ControlSettingsClass();
	public GraphicSettingsClass GraphicSettings = new GraphicSettingsClass();
	public SoundSettingsClass SoundSettings = new SoundSettingsClass();

	// Private Members

	// Use this for initialization
	void Start () {
		instance = this;

		Load ();
	}

	// Utility Methods
	string firstRunKey = "First Run";

	string lockOnKey = "Toggle Lock-On";

	string mouseSensKey = "Mouse Aim Sensitivity";
	string analogSensKey = "Analog Aim Sensitivity";
	
	string resolutionKey = "Resolution";
	string refreshRateKey = "Refresh Rate";
	string vsyncKey = "VSync";
	string fullscreenKey = "Fullscreen";
	string fieldOfViewKey = "Field of View";
	string textureQualityKey = "Texture Quality";
	string anisotropicFilteringKey = "Anisotropic Filtering";
	string hardwareAAKey = "Hardware Anti-Aliasing";
	string shadowResolutionKey = "Shadow Resolution";
	string shadowProjectionKey = "Shadow Projection";
	string shadowQualityKey = "Shadow Quality";
	string postProcessAAKey = "Post-Process Anti-Aliasing";

	string masterVolumeKey = "Master Volume";
	string effectsVolumeKey = "Effects Volume";
	string musicVolumeKey = "Music Volume";

	public void Load() {
		// Skip loading and save defaults if the PlayerPrefs entries don't exist
		if (!PlayerPrefs.HasKey (firstRunKey)) {
			PlayerPrefs.SetInt (firstRunKey, 0);
			Save ();
			return;
		}

		// Game Settings
		if (PlayerPrefs.HasKey (lockOnKey)) {
			GameSettings.toggleLockOn = PlayerPrefs.GetInt(lockOnKey) == 1;
		}
		
		// Control Settings
		if (PlayerPrefs.HasKey (mouseSensKey)) {
			ControlSettings.mouseSensitivity = PlayerPrefs.GetFloat(mouseSensKey);
		}

		if (PlayerPrefs.HasKey (analogSensKey)) {
			ControlSettings.analogSensitivity = PlayerPrefs.GetFloat(analogSensKey);
		}
		
		// Graphic Settings
		if (PlayerPrefs.HasKey (resolutionKey)) {
			GraphicSettings.resolution = PlayerPrefs.GetInt(resolutionKey);
		}

		if (PlayerPrefs.HasKey (refreshRateKey)) {
			GraphicSettings.refreshRate = PlayerPrefs.GetInt(refreshRateKey);
		}
		
		if (PlayerPrefs.HasKey (vsyncKey)) {
			GraphicSettings.vsync = PlayerPrefs.GetInt(vsyncKey) == 1;
		}
		
		if (PlayerPrefs.HasKey (fullscreenKey)) {
			GraphicSettings.fullscreen = PlayerPrefs.GetInt(fullscreenKey) == 1;
		}
		
		if (PlayerPrefs.HasKey (fieldOfViewKey)) {
			GraphicSettings.fieldOfView = PlayerPrefs.GetInt(fieldOfViewKey);
		}
		
		if (PlayerPrefs.HasKey (textureQualityKey)) {
			GraphicSettings.textureQuality = PlayerPrefs.GetInt(textureQualityKey);
		}
		
		if (PlayerPrefs.HasKey (anisotropicFilteringKey)) {
			GraphicSettings.anisotropicFiltering = PlayerPrefs.GetInt(anisotropicFilteringKey);
		}
		
		if (PlayerPrefs.HasKey (hardwareAAKey)) {
			GraphicSettings.hardwareAntiAliasing = PlayerPrefs.GetInt(hardwareAAKey);
		}
		
		if (PlayerPrefs.HasKey (shadowResolutionKey)) {
			GraphicSettings.shadowResolution = PlayerPrefs.GetInt(shadowResolutionKey);
		}
		
		if (PlayerPrefs.HasKey (shadowProjectionKey)) {
			GraphicSettings.shadowProjection = PlayerPrefs.GetInt(shadowProjectionKey);
		}
		
		if (PlayerPrefs.HasKey (shadowQualityKey)) {
			GraphicSettings.shadowQuality = PlayerPrefs.GetInt(shadowQualityKey);
		}
		
		if (PlayerPrefs.HasKey (postProcessAAKey)) {
			GraphicSettings.postProcessAntiAliasing = PlayerPrefs.GetInt(postProcessAAKey);
		}
		
		// Sound Settings
		if (PlayerPrefs.HasKey (masterVolumeKey)) {
			SoundSettings.masterVolume = PlayerPrefs.GetFloat(masterVolumeKey);
		}

		if (PlayerPrefs.HasKey (effectsVolumeKey)) {
			SoundSettings.effectsVolume = PlayerPrefs.GetFloat(effectsVolumeKey);
		}

		if (PlayerPrefs.HasKey (musicVolumeKey)) {
			SoundSettings.musicVolume = PlayerPrefs.GetFloat(musicVolumeKey);
		}
	}

	public void Save() {
		// Game Settings
		PlayerPrefs.SetInt (lockOnKey, GameSettings.toggleLockOn ? 1 : 0);
		
		// Control Settings
		PlayerPrefs.SetFloat (mouseSensKey, ControlSettings.mouseSensitivity);
		PlayerPrefs.SetFloat (analogSensKey, ControlSettings.analogSensitivity);
		
		// Graphic Settings
		PlayerPrefs.SetInt (resolutionKey, GraphicSettings.resolution);
		PlayerPrefs.SetInt (refreshRateKey, GraphicSettings.refreshRate);
		PlayerPrefs.SetInt (vsyncKey, GraphicSettings.vsync ? 1 : 0);
		PlayerPrefs.SetInt (vsyncKey, GraphicSettings.fullscreen ? 1 : 0);
		PlayerPrefs.SetInt (fieldOfViewKey, GraphicSettings.fieldOfView);
		PlayerPrefs.SetInt (textureQualityKey, GraphicSettings.textureQuality);
		PlayerPrefs.SetInt (anisotropicFilteringKey, GraphicSettings.anisotropicFiltering);
		PlayerPrefs.SetInt (hardwareAAKey, GraphicSettings.hardwareAntiAliasing);
		PlayerPrefs.SetInt (shadowResolutionKey, GraphicSettings.shadowResolution);
		PlayerPrefs.SetInt (shadowProjectionKey, GraphicSettings.shadowProjection);
		PlayerPrefs.SetInt (shadowQualityKey, GraphicSettings.shadowQuality);
		PlayerPrefs.SetInt (postProcessAAKey, GraphicSettings.postProcessAntiAliasing);
		
		// Sound Settings
		PlayerPrefs.SetFloat (masterVolumeKey, SoundSettings.masterVolume);
		PlayerPrefs.SetFloat (effectsVolumeKey, SoundSettings.effectsVolume);
		PlayerPrefs.SetFloat (musicVolumeKey, SoundSettings.musicVolume);
	}
}
