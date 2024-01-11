using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Eppy;
using XInputDotNetPure;

public class HSShootingFSM : MonoBehaviour {
	// Editor Settings
	public Transform m_camera;
	
	public RawImage m_crosshair;
	public RawImage m_lockOnRing;

	public GameObject BulletPuffPrefab;
	public GameObject BulletHolePrefab;
	public GameObject CannonProjectilePrefab;
	public Transform[] Weapons;
	public Transform CurrentWeapon {
		get {
			return Weapons[ m_currentWeaponIdx ];
		}
	}

	[System.Serializable]
	public class Settings
	{
		public float freeAimCrosshairSize = 0.05f;
		public float lockOnCrosshairSize = 0.075f;
		public float lockOnRadius = 0.8f; // lock-on radius in view space
	}
	public Settings m_settings = new Settings();

	// Member Variables
	StateMachine fsm = new StateMachine();
	HSMovementFSM m_movementFSM;

	int m_currentWeaponIdx = 0;
	int m_weaponSwitchId = 0;
	int m_numWeapons = 3;

	Vector3 m_defaultWeaponPosition;
	Vector3 m_targetWeaponPosition;

	Quaternion m_targetWeaponRotation;

	Vector3 m_crosshairPosition = Vector3.zero;
	Vector3 m_targetCrosshairPosition = Vector3.zero;
	
	Vector3 m_aimVector;

	float m_crosshairSize;
	float m_targetCrosshairSize;
	
	bool m_lockOn;
	bool m_lockToEnemy;
	Transform m_lockOnTransform = null;
	Vector3 m_lockOnPoint;

	SoundEffect m_soundPistolFire = new SoundEffect( "5,.2,,.1534,,.2913,.3,.3,.01,-.3,-.35,,,,,,,,,,.7719,-.6395,,,,1,,,.25,,," );
	SoundEffect m_soundShotgunFire = new SoundEffect( "3,.1,,.12,.4,.35,.3,.2,,-.3,,,,,,,,,,,,,.3,.1,.1,1,,,,,," );
	SoundEffect m_soundCannonFire = new SoundEffect( "3,.15,,.5,.5,.3,.3,.075,,-.1,.39,,,,,,,,,,,,,,,.9,-.075,,,,," );

	// Use this for initialization
	void Start () {
		m_movementFSM = GetComponent<HSMovementFSM> ();

		CurrentWeapon.gameObject.SetActive (true);
		m_defaultWeaponPosition = CurrentWeapon.localPosition;

		m_aimVector = m_camera.rotation * Vector3.forward;

		m_crosshairPosition = m_targetCrosshairPosition;

		m_crosshairSize = m_settings.freeAimCrosshairSize;
		m_targetCrosshairSize = m_crosshairSize;

		// Define states
		fsm.RegisterState ("Ready", StateReady, StateReadyEnter, null );
		fsm.RegisterState ("Fire", StateFire, StateFireEnter, null );
		fsm.RegisterState ("Reload", StateReload, StateReloadEnter, null );
		fsm.RegisterState ("SwitchOut", StateSwitchOut, StateSwitchOutEnter, null );
		fsm.RegisterState ("SwitchIn", StateSwitchIn, StateSwitchInEnter, null );
		
		// Define state transitions
		fsm.RegisterTransition ("Ready", "Fire");
		fsm.RegisterTransition ("Ready", "Reload");
		fsm.RegisterTransition ("Ready", "SwitchOut");
		
		fsm.RegisterTransition ("Fire", "Fire");
		fsm.RegisterTransition ("Fire", "Ready");
		fsm.RegisterTransition ("Fire", "SwitchOut");

		fsm.RegisterTransition ("SwitchOut", "SwitchIn");

		fsm.RegisterTransition ("SwitchIn", "Ready");
		
		// Force the FSM into the default state
		fsm.SwitchState ("Ready", true);
	}

	// Update is called once per frame
	void Update() {
		// Respond to KeyDown events that may be lost if checked in FixedUpdate
		if (InputManager.instance.CurrentInput.weaponSlot1) {
			m_weaponSwitchId = 0;
		}
		
		if (InputManager.instance.CurrentInput.weaponSlot2) {
			m_weaponSwitchId = 1;
		}
		
		if (InputManager.instance.CurrentInput.weaponSlot3) {
			m_weaponSwitchId = 2;
		}
		
		m_weaponSwitchId -= InputManager.instance.CurrentInput.mousewheel;
		if (m_weaponSwitchId < 0) {
			m_weaponSwitchId = Weapons.Length - 1;
		} else {
			m_weaponSwitchId %= Weapons.Length;
		}
		
		// Interpolate weapon position
		CurrentWeapon.localPosition = Vector3.Lerp (CurrentWeapon.localPosition, m_targetWeaponPosition, 0.4f);
		
		// Set weapon position and rotation
		m_targetWeaponRotation = Quaternion.LookRotation (m_aimVector, m_camera.rotation * Vector3.up);
		
		// Calculate weapon rotation and crosshair position
		if (m_lockOn && m_lockOnTransform == null) {
			CurrentWeapon.rotation = m_targetWeaponRotation;
			m_crosshairPosition = m_targetCrosshairPosition;
		} else {
			CurrentWeapon.rotation = Quaternion.Slerp (CurrentWeapon.rotation, m_targetWeaponRotation, 0.4f);
			m_crosshairPosition = Vector3.Lerp (m_crosshairPosition, m_targetCrosshairPosition, 0.4f);
		}
		
		// Linearly interpolate crosshair size
		m_crosshairSize = Mathf.Lerp (m_crosshairSize, m_targetCrosshairSize, 0.4f);

		// Change crosshair size when locked on
		if (!m_lockOn) {
			m_targetCrosshairSize = Screen.height * m_settings.freeAimCrosshairSize;
		} else {
			m_targetCrosshairSize = Screen.height * m_settings.lockOnCrosshairSize;
		}
		
		m_crosshair.rectTransform.anchoredPosition = m_crosshairPosition;
		m_crosshair.rectTransform.sizeDelta = new Vector2( m_crosshairSize, m_crosshairSize );

		m_lockOnRing.rectTransform.sizeDelta = new Vector2( Screen.height * m_settings.lockOnRadius, Screen.height * m_settings.lockOnRadius );
	}

	// FixedUpdate is called once per physics frame
	void FixedUpdate () {
		UpdateGlobalState();
		fsm.UpdateState();
	}

	// State Behaviours
	void UpdateGlobalState()
	{
		// Raycast into the scene along the aiming vector to see if we're aiming at something
		RaycastHit aimRayHitInfo = new RaycastHit ();
		bool aimRayHit = Physics.Raycast (m_camera.position, m_aimVector, out aimRayHitInfo);
		
		// Search visible scene for lockable targets
		GameObject closestEnemy = null;
		if (!m_lockOn) {
			GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");
			ArrayList visibleEnemies = new ArrayList ();

			Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes (m_camera.GetComponent<Camera> ());

			foreach (GameObject enemy in enemies) {
				if (GeometryUtility.TestPlanesAABB (frustumPlanes, enemy.GetComponent<Collider> ().bounds)) {
					Vector3 camToEnemy = enemy.transform.position - m_camera.position;
					
					RaycastHit enemyRayHitInfo = new RaycastHit ();
					bool enemyRayHit = Physics.Raycast (m_camera.position, camToEnemy, out enemyRayHitInfo);
					if (enemyRayHit && enemyRayHitInfo.transform.tag == "Enemy") {
						visibleEnemies.Add (enemy);
					}
				}
			}

			float closestDist = 0.0f;
			foreach (GameObject enemy in visibleEnemies) {
				Vector3 screenSpaceAimPoint = new Vector3( Screen.width * 0.5f, Screen.height * 0.5f, 0.0f );
				Vector3 enemyPosition = m_camera.GetComponent<Camera>().WorldToScreenPoint( enemy.transform.position );
				float dist = Vector3.Magnitude (screenSpaceAimPoint - enemyPosition);

				if (closestEnemy == null || dist < closestDist) {
					// Only lock if the enemy is inside the lock radius
					if( dist < 0.5f * Screen.height * m_settings.lockOnRadius )
					{
						closestEnemy = enemy;
						closestDist = dist;
					}
				}
			}
		}

		// Enable/Disable lock-on, set lock point based on whether we're aiming at an object or the skybox
		bool holdLock = !SaveManager.instance.GameSettings.toggleLockOn;
		bool currentLock = InputManager.instance.CurrentInput.lockOn;
		bool pressLock = InputManager.instance.PressInput.lockOn;

		if (!m_lockOn) {
			if (closestEnemy != null) {
				if (( holdLock && currentLock ) || ( !holdLock && pressLock ) ) {
					m_lockOn = true;
					m_lockToEnemy = true;
					m_lockOnTransform = closestEnemy.transform;
				}
			} else if (pressLock) {
				m_lockOn = true;
				m_lockToEnemy = false;
				
				if (aimRayHit) {
					m_lockOnPoint = aimRayHitInfo.point;
				} else {
					m_lockOnPoint = m_camera.position + (m_aimVector * m_camera.GetComponent<Camera> ().farClipPlane);
				}
			}
		}
		else {
			// Determine whether the crosshair is onscreen
			bool onScreen = Mathf.Abs ( m_targetCrosshairPosition.x ) < ( Screen.width * 0.5f ) + m_crosshairSize
				&& Mathf.Abs ( m_targetCrosshairPosition.y ) < ( Screen.height * 0.5f ) + m_crosshairSize;

			bool delockInput = false;

			if( holdLock )
			{
				if( !InputManager.instance.CurrentInput.lockOn )
				{
					delockInput = true;
				}
			}
			else
			{
				if( InputManager.instance.PressInput.lockOn )
				{
					delockInput = true;
				}
			}

			if ( delockInput || !onScreen || ( m_lockToEnemy && m_lockOnTransform == null )) {
				m_lockOn = false;
				m_lockToEnemy = false;
				m_lockOnTransform = null;
				m_lockOnPoint = Vector3.zero;
			}
		}
	}

	void StateReadyEnter( string fromState, string toState )
	{
	}

	void StateReady()
	{
		// Behaviour
		AimWeapon ();
		ApplyWeaponBob ();

		// State Transitions
		if (InputManager.instance.CurrentInput.fire) {
			fsm.SwitchState( "Fire" );
		}

		if (m_weaponSwitchId != m_currentWeaponIdx) {
			fsm.SwitchState( "SwitchOut" );
		}
	}

	void StateFireEnter( string fromState, string toState )
	{
		CurrentWeapon.Find ("Mesh").GetComponent<Animation> ().Stop ();

		switch (m_currentWeaponIdx) {
		case 0:
			float spread = 0.0f;

			if( !m_lockOn )
			{
				spread = 5.0f;
			}

			FireBullets( 1, 15, spread, spread );
			m_soundPistolFire.Play ();
			goto default;
		case 1:
			FireBullets( 12, 8, 5.0f, 5.0f );
			m_soundShotgunFire.Play ();
			goto default;
		case 2:
			FireProjectile( CannonProjectilePrefab, 90, 30.0f, 0.0f, 0.0f );
			m_soundCannonFire.Play ();
			goto default;
		default:
			CurrentWeapon.Find ("Mesh").GetComponent<Animation> ().Play ();
			break;
		}
	}
	
	void StateFire()
	{
		// Behaviour
		AimWeapon ();
		ResetWeaponBob ();
		
		// State Transitions
		// Get current animation state
		Animation anim = CurrentWeapon.Find("Mesh").GetComponent<Animation>();
		string animName = "";
		float refireCancelTiming = 0.0f;
		float switchCancelTiming = 0.0f;
		
		switch (m_currentWeaponIdx) {
		case 0:
			animName = "Pistol Fire" ;
			refireCancelTiming = 0.2f;
			switchCancelTiming = 0.6f;
			break;
		case 1:
			animName = "Shotgun Fire" ;
			refireCancelTiming = 0.62f;
			switchCancelTiming = 0.6f;
			break;
		case 2:
			animName = "Cannon Fire" ;
			refireCancelTiming = 0.7f;
			switchCancelTiming = 0.5f;
			break;
		default:
			break;
		}

		AnimationState comp = anim [animName];
		float animProgress = comp.time / comp.length;

		// Pistol instant refire
		if (!CurrentWeapon.Find ("Mesh").GetComponent<Animation> ().isPlaying) {
			fsm.SwitchState( "Ready" );
		}

		if (InputManager.instance.PressInput.fire && animProgress >= refireCancelTiming) {
			fsm.SwitchState( "Fire" );
		}
		
		if (m_weaponSwitchId != m_currentWeaponIdx && animProgress >= switchCancelTiming) {


			if( animProgress >= switchCancelTiming )
			{
				fsm.SwitchState( "SwitchOut" );
			}
		}
	}
	
	void StateReloadEnter( string fromState, string toState )
	{
	}
	
	void StateReload()
	{
		// Behaviour
		ResetWeaponBob ();
	}

	void StateSwitchOutEnter( string fromState, string toState )
	{
		m_aimVector = m_camera.rotation * Vector3.forward;
	}

	void StateSwitchOut()
	{
		// Behaviour
		m_aimVector = Vector3.Slerp (m_aimVector, m_camera.rotation * Vector3.down, 0.1f);
		
		// State Transitions
		if (Vector3.Dot (m_aimVector, m_camera.rotation * Vector3.down) > 0.9f) {
			fsm.SwitchState( "SwitchIn" );
		}
	}

	void StateSwitchInEnter( string fromState, string toState )
	{
		SwitchWeapon (m_weaponSwitchId);
	}
	
	void StateSwitchIn()
	{
		// Behaviour
		m_aimVector = Vector3.Slerp (m_aimVector, m_camera.rotation * Vector3.forward, 0.1f);
		
		// State Transitions
		if (Vector3.Dot (m_aimVector, m_camera.rotation * Vector3.forward) > 0.9f) {
			fsm.SwitchState( "Ready" );
		}
	}

	// Utility Methods
	void AimWeapon()
	{
		// Calculate aiming vector (view space direction to target) and screen-space crosshair position
		if (m_lockOn) {
			Vector3 lockPos;

			if( m_lockToEnemy )
			{
				lockPos = m_lockOnTransform.position;
			}
			else
			{
				lockPos = m_lockOnPoint;
			}

			m_aimVector = Vector3.Normalize (lockPos - m_camera.position);
			m_targetCrosshairPosition = m_camera.GetComponent<Camera> ().WorldToScreenPoint (lockPos);
			m_targetCrosshairPosition -= new Vector3( Screen.width * 0.5f, Screen.height * 0.5f, 0.0f );
		} else {
			m_aimVector = m_camera.rotation * Vector3.forward;
			m_targetCrosshairPosition = new Vector3 (0.0f, 0.0f, 0.0f);
		}
	}

	void ApplyWeaponBob()
	{
		m_targetWeaponPosition = m_defaultWeaponPosition + m_movementFSM.CameraHeadbob * 0.25f;
	}

	void ResetWeaponBob()
	{
		m_targetWeaponPosition = m_defaultWeaponPosition;
	}
	
	void FireBullets( int num, int damage, float xSpread, float ySpread )
	{
		for( int i = 0; i < num; ++i )
		{
			RaycastHit lookHit = new RaycastHit();
			
			Quaternion xRot = Quaternion.AngleAxis( xSpread * Random.Range( -0.5f, 0.5f ), CurrentWeapon.rotation * Vector3.up );
			Quaternion yRot = Quaternion.AngleAxis( ySpread * Random.Range( -0.5f, 0.5f ), CurrentWeapon.rotation * Vector3.right );
			
			bool lHit = Physics.Raycast( m_camera.position, xRot * yRot * m_aimVector, out lookHit );
			
			RaycastHit bulletHit = new RaycastHit();
			bool bHit = false;
			
			if( lHit )
			{
				bHit = Physics.Raycast( CurrentWeapon.Find ( "Mesh" ).position, lookHit.point - CurrentWeapon.Find ( "Mesh" ).position, out bulletHit );
			}
			
			if( bHit )
			{
				Vector3 hitPos = bulletHit.point + bulletHit.normal * 0.01f;
				Quaternion puffRotation = Quaternion.Euler( new Vector3( Random.Range ( 0.0f, 90.0f ), Random.Range ( 0.0f, 90.0f ), Random.Range ( 0.0f, 90.0f ) ) );
				GameObject.Instantiate( BulletPuffPrefab, hitPos, puffRotation );
				
				if( bulletHit.transform.tag == "Enemy" )
				{
					bulletHit.transform.GetComponent<EnemyBase>().TakeDamage( damage );
				}
				else
				{
					Quaternion holeRotation = Quaternion.LookRotation( -bulletHit.normal );
					GameObject.Instantiate( BulletHolePrefab, hitPos, holeRotation );
				}
			}
		}
	}
	
	void FireProjectile( GameObject projectile, int damage, float velocity, float xSpread, float ySpread )
	{
		RaycastHit lookHit = new RaycastHit();
		
		Quaternion xRot = Quaternion.AngleAxis( xSpread * Random.Range( -0.5f, 0.5f ), CurrentWeapon.rotation * Vector3.up );
		Quaternion yRot = Quaternion.AngleAxis( ySpread * Random.Range( -0.5f, 0.5f ), CurrentWeapon.rotation * Vector3.right );
		
		bool lHit = Physics.Raycast( m_camera.position, xRot * yRot * m_aimVector, out lookHit );

		Quaternion rot;
		if (lHit) {
			rot = Quaternion.LookRotation (lookHit.point - CurrentWeapon.Find ("Mesh").position, CurrentWeapon.rotation * Vector3.up);

		} else {
			rot = Quaternion.LookRotation (m_aimVector, CurrentWeapon.rotation * Vector3.up);
		}

		GameObject go = (GameObject)GameObject.Instantiate( projectile, CurrentWeapon.position, rot );
		go.GetComponent<ProjectileBase>().Damage = damage;
		go.GetComponent<ProjectileBase>().Velocity = velocity;
	}
	
	void SwitchWeapon( int idx )
	{
		Quaternion tempRotation = CurrentWeapon.rotation;

		CurrentWeapon.localPosition = m_defaultWeaponPosition;
		CurrentWeapon.rotation = Quaternion.identity;
		CurrentWeapon.Find ("Mesh").GetComponent<Animation> ().clip.SampleAnimation (CurrentWeapon.Find ("Mesh").gameObject, 0.0f);
		CurrentWeapon.Find ("Mesh").GetComponent<Animation> ().Stop();
		CurrentWeapon.gameObject.SetActive (false);

		m_currentWeaponIdx = idx;

		CurrentWeapon.gameObject.SetActive (true);
		m_defaultWeaponPosition = CurrentWeapon.localPosition;
		CurrentWeapon.rotation = tempRotation;

		ApplyWeaponBob ();
	}
}
