using UnityEngine;
using System.Collections;

using Eppy;
using XInputDotNetPure;

public class HSMovementFSM : MonoBehaviour {
	// Editor Settings
	public CapsuleCollider m_capsuleCollider = null;
	public Transform m_slideCollider = null;
	public Transform m_turnWrapper = null;
	public Transform m_cameraWrapper = null;
	public Camera m_firstPersonCamera = null;

	[System.Serializable]
	public class Settings
	{
		public float gravityStrength = 1.0f; // strength of gravity vector used to stick the player to surfaces
		public float baseRotationLerpFactor = 0.1f; // rate at which the controller interpolates between movement speeds

		public float headbobLerpFactor = 0.1f;
		public float headbobFrequency = 1.0f; // rate at which the camera offset interpolates
		public float headbobMagnitude = 0.25f; // rate at which the camera offset interpolates
		public float headbobXFactor = 1.0f;
		public float headbobYFactor = 1.0f;

		public float groundDeltaV = 2.0f; // rate at which the controller accellerates on the ground when there is input
		public float slowDownFactor = 0.8f; // rate at which the controller comes to a stop when there is no input on the ground
		public float runSpeed = 10.0f; // maximum walking speed
		public float speedLerpFactor = 0.1f; // rate at which the controller interpolates between movement speeds
		
		public float airDeltaV = 0.5f; // rate at which the controller accellerates in the air when there is input
		
		public float jumpDeltaV = 16.0f; // vertical velocity change applied on jump
		
		public float dashDeltaV = 30.0f; // velocity change applied on dash
		public float dashDuration = 0.2f; // length of full-speed dash
		public float dashStopDuration = 0.2f; // length of deceleration period
		public float dashStopFactor = 0.8f; // rate at which the controller comes to a stop after a dash
		public float dashRechargeDuration = 1.5f; // rate at which the controller comes to a stop after a dash
		public float dashTilt = 7.5f; // degrees away from the up vector that the controller will tilt while dashing
		public float dashFOVOffset = 5.0f; // FOV offset in degrees, applied to the first person camera on dash
		
		public float slideDashDeltaV = 30.0f; // velocity change applied on slide dash
		public float slideDashDuration = 0.4f; // length of full-speed slide dash
		public float slideDashStopDuration = 0.4f; // length of deceleration period
		public float slideDashStopFactor = 0.9f; // rate at which the controller comes to a stop after a slide dash
		public float slideDashTilt = 15.0f; // degrees away from the up vector that the controller will tilt while slide dashing
		public float slideDashVerticalOffset = 1.5f;
		public float slideDashFOVOffset = 5.0f; // FOV offset in degrees, applied to the first person camera on dash

		public float wallrunRayAngle = 30.0f; // angle offset from player orientation at which to check for walls
		public float wallrunRayHeightTop = 0.25f; // vertical offset for wall box-casting
		public float wallrunRayHeightBottom = 0.4f; // vertical offset for wall box-casting
		public float wallrunStickDistance = 2.0f; // maximum distance at which a player will start wallrunning
		public float wallrunGravityStrength = 1.0f; // strength of gravity vector whilst wallrunnning
		public float wallrunSpeed = 14.0f; // strength of gravity vector whilst wallrunnning
		public float wallrunUpGravityStrength = 1.0f; // strength of gravity vector whilst wallrunnning
		public float wallrunUpDeltaV = 16.0f; // vertical change in velocity during upwards wall run
		public float wallrunMaxJump = 8.0f; // maximum vertical ascent velocity whilst wallrunning
		public float wallrunStickSpeed = 4.0f; // minimum velocity required to maintain a wallrun
		public float wallrunKickAngle = 45.0f; // minimum velocity required to maintain a wallrun
		public float wallrunTilt = 15.0f; // angle away from the wall that the camera tilts during a wallrun
		
		public float climbOverDeltaV = 8.0f; // velocity change applied during climb-over
		
		public float boostDashSpeed = 16.0f; // maximum speed during boost dash
		public float boostDashSideDuration = 1.5f; // length of boost dash
		public float boostDashUpDuration = 0.5f; // length of boost dash
		public float boostDashFOVOffset = 5.0f;
		
		public float lungeUpDeltaV = 20.0f; // velocity change applied on dash

		public float groundedCheckDistance = 0.5f; // maximum distance that the player will stick to the ground at
		public float groundPushCheckDistance = 1.0f; // maximum distance that the player will stick to the ground at
		public float groundPushForce = 0.2f; // vertical velocity change used to keep the player grounded
		
		public float ceilingCheckDistance = 1.0f; // minimum distance between the player's head and ceiling that will allow a jump
		
		public float yawClamp = 90.0f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
		public float rightStickSensitivity = 6.0f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
	}
	public Settings m_settings = new Settings();
	
	// Public Variables
	public Vector3 CameraHeadbob {
		get {
			return m_cameraHeadbob;
		}
	}

	// Member Variables
	StateMachine fsm = new StateMachine();

	GameObject m_spawnPoint;

	Rigidbody m_rigidbody = null;

	float m_yaw = 0.0f;
	float m_pitch = 0.0f;
	float m_yawOffset = 0.0f;
	float m_pitchOffset = 0.0f;
	float m_targetPitchOffset = 0.0f;
	float m_targetYawOffset = 0.0f;

	Vector3 m_defaultCameraOffset;
	Vector3 m_cameraHeadbob;
	Vector3 m_cameraOffset;
	Vector3 m_targetCameraOffset;
	float m_cameraMomentum = 0.0f;
	
	Vector3 m_defaultGravity = Vector3.down;
	Vector3 m_gravity;
	Quaternion m_baseRotation = Quaternion.identity;
	Quaternion m_targetBase = Quaternion.identity;
	
	bool m_shouldJump = false;
	bool m_shouldDash = false;

	float m_runEpsilon = 0.1f;
	bool m_justJumped = false;
	bool m_justWallran = false;
	bool m_doubleJumpAvailable = false;
	
	float m_maxVel = 0.0f;
	float m_targetMaxVel = 0.0f;

	float m_dashTimer = 0.0f;
	int m_groundDashCount = 0;
	float m_groundDashTimer = 0.0f;
	bool m_airDashAvailable = false;
	bool m_airDashWallAvailable = false;

	Vector3 m_wallrunNormal;
	Vector3 m_wallrunTiltAxis;
	Vector3 m_wallrunDirection;
	
	float m_boostDashTimer = 0.0f;

	bool m_lungeUpAvailable = false;
	
	bool m_grounded = false;
	bool m_groundPush = false;
	bool m_ceilingCheck = false;
	
	Vector3 m_lateralVelocity {
		get {
			return m_rigidbody.velocity - m_verticalVelocity;
		}
	}
	Vector3 m_verticalVelocity {
		get {
			return Vector3.Project (m_rigidbody.velocity, m_gravity);
		}
	}

	// Sound Effects
	SoundEffect m_soundFootstep = new SoundEffect( "5,.25,,,,.2,.1,.3,,-.5,,,,,,,,,,,,,,,,1,,,,,," );
	SoundEffect m_soundJump = new SoundEffect( "5,.1,,.2,,.25,.3,.43,,.15,,,,,,,,,,,.2,,,,,.7,,,,,," );
	SoundEffect m_soundLand = new SoundEffect( "3,.1,,.02,,.2,.3,.3974,,-.6,,,,,,,,,,,,,,,,1,,,.2,,," );
	SoundEffect m_soundDash = new SoundEffect( "5,.125,,.25,,.25,.3,.6,,.25,,,,,,,,,,,.595,,,,,1,,,.1,,," );
	SoundEffect m_soundWallCling = new SoundEffect( "3,.1,,.1,,.12,.2,.45,,-.5,,,,,,,,,,,,,,,,1,,,,,," );

	// Spawn Methods
	public void RegisterSpawnPoint( GameObject obj )
	{
		m_spawnPoint = obj;
	}
	
	void Respawn()
	{
		transform.position = m_spawnPoint.transform.position;
		m_capsuleCollider.transform.rotation = m_spawnPoint.transform.rotation;
		m_rigidbody.velocity = Vector3.zero;
		ResetCameraRotation ();
		ResetBaseRotation ( false );
		ResetGravity ();
		fsm.SwitchState ("Idle", true);
	}
	
	// Use this for initialization
	void Start () {
		// Pre-fetch controller components
		m_rigidbody = GetComponent<Rigidbody> ();

		m_defaultCameraOffset = m_turnWrapper.localPosition;
		m_targetCameraOffset = m_defaultCameraOffset;
		m_cameraOffset = m_targetCameraOffset;

		// Initialise camera rotation state
		ResetCameraRotation();

		// Initialise gravity state
		ResetGravity ();

		// Initialise interpolating values
		m_maxVel = m_settings.runSpeed;
		m_targetMaxVel = m_maxVel;

		// Define states
		fsm.RegisterState ("Idle", StateIdle, StateIdleEnter, null );
		fsm.RegisterState ("Run", StateRun, null, null);
		fsm.RegisterState ("Jump", StateJump, StateJumpEnter, null);
		fsm.RegisterState ("Fall", StateFall, null, null);
		fsm.RegisterState ("DoubleJump", StateDoubleJump, StateDoubleJumpEnter, null);
		fsm.RegisterState ("Dash", StateDash, StateDashEnter, StateDashExit);
		fsm.RegisterState ("DashStop", StateDashStop, StateDashStopEnter, StateDashStopExit);
		fsm.RegisterState ("WallrunSide", StateWallrunSide, StateWallrunSideEnter, StateWallrunSideExit);
		fsm.RegisterState ("WallrunUp", StateWallrunUp, StateWallrunUpEnter, StateWallrunUpExit);
		fsm.RegisterState ("ClimbOver", StateClimbOver, null, null);
		fsm.RegisterState ("BoostDash", StateBoostDash, StateBoostDashEnter, StateBoostDashExit);
		fsm.RegisterState ("LungeUp", StateLungeUp, StateLungeUpEnter, StateLungeUpExit);
		fsm.RegisterState ("FastFall", StateFastFall, StateFastFallEnter, null);
		fsm.RegisterState ("SlideDash", StateSlideDash, StateSlideDashEnter, StateSlideDashExit);
		fsm.RegisterState ("SlideDashStop", StateSlideDashStop, StateSlideDashStopEnter, StateSlideDashStopExit);

		// Define state transitions
		fsm.RegisterTransition ("Idle", "Run");
		fsm.RegisterTransition ("Idle", "Jump");
		fsm.RegisterTransition ("Idle", "Fall");
		fsm.RegisterTransition ("Idle", "BoostDash");

		fsm.RegisterTransition ("Run", "Idle");
		fsm.RegisterTransition ("Run", "Jump");
		fsm.RegisterTransition ("Run", "Fall");
		fsm.RegisterTransition ("Run", "Dash");
		fsm.RegisterTransition ("Run", "SlideDash");
		
		fsm.RegisterTransition ("Jump", "Jump");
		fsm.RegisterTransition ("Jump", "Fall");
		fsm.RegisterTransition ("Jump", "DoubleJump");
		fsm.RegisterTransition ("Jump", "Dash");
		fsm.RegisterTransition ("Jump", "WallrunSide");
		fsm.RegisterTransition ("Jump", "WallrunUp");
		fsm.RegisterTransition ("Jump", "FastFall");
		
		fsm.RegisterTransition ("Fall", "Idle");
		fsm.RegisterTransition ("Fall", "Jump");
		fsm.RegisterTransition ("Fall", "DoubleJump");
		fsm.RegisterTransition ("Fall", "Dash");
		fsm.RegisterTransition ("Fall", "WallrunSide");
		fsm.RegisterTransition ("Fall", "WallrunUp");
		fsm.RegisterTransition ("Fall", "FastFall");
		
		fsm.RegisterTransition ("DoubleJump", "Fall");
		fsm.RegisterTransition ("DoubleJump", "Dash");
		fsm.RegisterTransition ("DoubleJump", "WallrunSide");
		fsm.RegisterTransition ("DoubleJump", "WallrunUp");
		fsm.RegisterTransition ("DoubleJump", "FastFall");

		fsm.RegisterTransition ("Dash", "DashStop");
		fsm.RegisterTransition ("Dash", "Jump");
		fsm.RegisterTransition ("Dash", "DoubleJump");
		fsm.RegisterTransition ("Dash", "WallrunSide");
		fsm.RegisterTransition ("Dash", "WallrunUp");
		fsm.RegisterTransition ("Dash", "BoostDash");
		
		fsm.RegisterTransition ("DashStop", "Idle");
		fsm.RegisterTransition ("DashStop", "Fall");
		fsm.RegisterTransition ("DashStop", "Jump");
		fsm.RegisterTransition ("DashStop", "DoubleJump");
		fsm.RegisterTransition ("DashStop", "Dash");
		fsm.RegisterTransition ("DashStop", "WallrunSide");
		fsm.RegisterTransition ("DashStop", "WallrunUp");
		fsm.RegisterTransition ("DashStop", "FastFall");
		fsm.RegisterTransition ("DashStop", "SlideDash");
		
		fsm.RegisterTransition ("WallrunSide", "Fall");
		fsm.RegisterTransition ("WallrunSide", "Jump");
		fsm.RegisterTransition ("WallrunSide", "BoostDash");
		
		fsm.RegisterTransition ("WallrunUp", "Fall");
		fsm.RegisterTransition ("WallrunUp", "Jump");
		fsm.RegisterTransition ("WallrunUp", "ClimbOver");
		fsm.RegisterTransition ("WallrunUp", "BoostDash");
		fsm.RegisterTransition ("WallrunUp", "LungeUp");
		
		fsm.RegisterTransition ("ClimbOver", "Idle");
		
		fsm.RegisterTransition ("BoostDash", "Jump");
		fsm.RegisterTransition ("BoostDash", "Fall");
		
		fsm.RegisterTransition ("LungeUp", "Jump");
		fsm.RegisterTransition ("LungeUp", "Fall");
		fsm.RegisterTransition ("LungeUp", "WallrunUp");
		fsm.RegisterTransition ("LungeUp", "ClimbOver");
		
		fsm.RegisterTransition ("FastFall", "Idle");
		
		fsm.RegisterTransition ("SlideDash", "SlideDashStop");
		fsm.RegisterTransition ("SlideDash", "Jump");
		fsm.RegisterTransition ("SlideDash", "Fall");
		
		fsm.RegisterTransition ("SlideDashStop", "Idle");
		fsm.RegisterTransition ("SlideDashStop", "Jump");
		fsm.RegisterTransition ("SlideDashStop", "Fall");

		// Manually call default state enter method
		fsm.SwitchState ("Idle", true);
	}

	void OnTriggerEnter( Collider trigger )
	{
		if (trigger.tag == "Respawn") {
			Respawn ();
		}
	}

	// FixedUpdate is called once per physics frame
	void FixedUpdate () {
		UpdateGlobalState ();
		fsm.UpdateState();
	}

	// State Behaviours
	void UpdateGlobalState()
	{
		if (InputManager.instance.PressInput.quickturn) {
			m_targetYawOffset = 180.0f;
		}

		// Check if the controller is grounded or should be pushed to ground
		var radius = m_capsuleCollider.radius * 0.9f;
		RaycastHit dummy = new RaycastHit();

		m_grounded = Physics.SphereCast( m_capsuleCollider.transform.position + m_capsuleCollider.center, radius, m_gravity, out dummy, ( m_capsuleCollider.height * 0.5f ) - radius + m_settings.groundedCheckDistance );
		m_groundPush = Physics.SphereCast( m_capsuleCollider.transform.position + m_capsuleCollider.center, radius, m_gravity, out dummy, ( m_capsuleCollider.height * 0.5f ) - radius + m_settings.groundPushCheckDistance );

		m_shouldJump = InputManager.instance.PressInput.jump && !m_ceilingCheck;
		m_shouldDash = InputManager.instance.PressInput.dash && InputManager.instance.CurrentInput.movement.sqrMagnitude > 0.0f;

		if (fsm.CurrentState == "SlideDash") {
			m_ceilingCheck = Physics.Raycast(m_slideCollider.position, -m_gravity, m_settings.ceilingCheckDistance);
		} else {
			m_ceilingCheck = Physics.SphereCast (m_capsuleCollider.transform.position + m_capsuleCollider.center, radius, -m_gravity, out dummy, (m_capsuleCollider.height * 0.5f) - radius + m_settings.ceilingCheckDistance);
		}

		// Countdown ground dash recovery
		if (m_groundDashTimer > 0.0f) {
			if( !InputManager.instance.CurrentInput.dash )
			{
				m_groundDashTimer -= Time.fixedDeltaTime;
			}
		} else {
			m_groundDashCount = 0;
		}
		
		// Smoothly interpolate maximum velocity toward its target
		m_maxVel = Mathf.Lerp (m_maxVel, m_targetMaxVel, m_settings.speedLerpFactor);
		
		// Smoothly interpolate base controller rotation toward its target
		m_baseRotation = Quaternion.Slerp (m_baseRotation, m_targetBase, m_settings.baseRotationLerpFactor);
		
		// Get mouse input and calculate camera pitch/yaw, clamp to prevent over-rotation
		m_yaw += InputManager.instance.CurrentInput.look.x;
		m_pitch += InputManager.instance.CurrentInput.look.y;
		m_pitch = Mathf.Clamp (m_pitch, -m_settings.yawClamp, m_settings.yawClamp);

		// Smoothly interpolate pitch/yaw offsets
		m_pitchOffset = Mathf.Lerp (m_pitchOffset, m_targetPitchOffset, 0.2f);
		m_yawOffset = Mathf.Lerp (m_yawOffset, m_targetYawOffset, 0.2f);
		
		if( Mathf.Abs( m_targetPitchOffset - m_pitchOffset ) < 1.0f )
		{
			TransferPitchOffset();
		}

		if( Mathf.Abs( m_targetYawOffset - m_yawOffset ) < 1.0f )
		{
			TransferYawOffset();
		}
		
		
		// Reset the player controller container's rotation to stop the physics system from being weird
		transform.rotation = Quaternion.identity;
		
		// Rotate the collider and camera wrapper to match the base rotation, then on the Y axis
		m_capsuleCollider.transform.rotation = m_baseRotation * Quaternion.Euler( 0.0f, m_yaw + m_yawOffset, 0.0f );
		m_turnWrapper.rotation = m_capsuleCollider.transform.rotation;


		// Calculate the camera's look vector
		float farPlane = m_firstPersonCamera.farClipPlane;
		Vector3 lookPoint =  m_rigidbody.position + Quaternion.Euler( m_pitch + m_pitchOffset, m_yaw + m_yawOffset, 0.0f ) * Vector3.forward * farPlane;
		m_cameraWrapper.LookAt(
			m_baseRotation * lookPoint,
			m_baseRotation * Vector3.up
		);

		// Smoothly interpolate camera offset and apply
		m_cameraOffset = Vector3.Lerp (m_cameraOffset, m_cameraHeadbob + m_targetCameraOffset, m_settings.headbobLerpFactor);
		m_turnWrapper.localPosition = m_defaultCameraOffset + m_cameraOffset * m_rigidbody.velocity.magnitude * m_settings.headbobMagnitude;

		// Reset target camera offset
		m_cameraHeadbob = Vector3.zero;
		m_targetCameraOffset = Vector3.zero;
	}

	void StateIdleEnter( string fromState, string tostring )
	{
		// Reset the Double Jump, Air Dash and Upward Wallrun
		m_doubleJumpAvailable = true;
		m_airDashAvailable = true;
		m_airDashWallAvailable = true;
		m_lungeUpAvailable = true;

		// If transitioning from a fall, play the landing sound
		if (fromState == "Fall") {
			m_soundLand.Play();
		}
	}

	void StateIdle()
	{
		// Behaviour
		ApplyGravity( m_settings.gravityStrength );
		GroundPush ();
		
		var slowVel = m_lateralVelocity;
		slowVel *= m_settings.slowDownFactor;
		m_rigidbody.velocity = slowVel + m_verticalVelocity;

		// State Transitions
		if (InputManager.instance.CurrentInput.movement != Vector3.zero || m_lateralVelocity.magnitude > m_runEpsilon) {
			fsm.SwitchState( "Run" );
		}
		
		if (m_shouldJump) {
			fsm.SwitchState( "Jump" );
		}
		
		if (!m_grounded) {
			fsm.SwitchState( "Fall" );
		}
	}
	
	void StateRun()
	{
		// Behaviour
		// Copy current velocity for modification
		var moveVel = m_lateralVelocity;

		// Calculate velocity change from directional input, rotate based on collider facing
		var deltaV = m_capsuleCollider.transform.rotation * ( m_settings.groundDeltaV * InputManager.instance.CurrentInput.movement );
		
		// If the player isn't inputting movement, or is moving against the character's velocity
		// Degrade velocity by a set value
		if (InputManager.instance.CurrentInput.movement.sqrMagnitude == 0.0f || Vector3.Dot (deltaV.normalized, moveVel.normalized) < 0.0f) {
			moveVel *= m_settings.slowDownFactor;
		}
		
		// If the player is inputting movement, add velocity change and clamp velocity to max speed
		if (InputManager.instance.CurrentInput.movement.sqrMagnitude > 0.0f ) {
			moveVel += deltaV;
			
			m_targetMaxVel = m_settings.runSpeed;

			var relativeMaxVel = m_maxVel * InputManager.instance.CurrentInput.movement.magnitude;
			if (moveVel.magnitude > relativeMaxVel) {
				moveVel = moveVel.normalized * relativeMaxVel;
			}
		}
		
		// Apply calculated movement velocity
		m_rigidbody.velocity = moveVel + m_verticalVelocity;
		
		ApplyGravity( m_settings.gravityStrength );
		GroundPush ();

		ApplyHeadbob ( 1.0f );

		// State Transitions
		if (m_shouldJump) {
			fsm.SwitchState( "Jump" );
		}
		
		if (!m_grounded) {
			fsm.SwitchState( "Fall" );
		}
		
		if (m_lateralVelocity.magnitude < m_runEpsilon) {
			fsm.SwitchState( "Idle" );
		}
		
		if (m_shouldDash) {
			if ( m_groundDashCount < 3 ) {
				fsm.SwitchState( "Dash" );
			}
		}
		
		if (InputManager.instance.PressInput.crouch && InputManager.instance.CurrentInput.movement.sqrMagnitude > 0.0f) {
			fsm.SwitchState( "SlideDash" );
		}
	}
	
	void StateJumpEnter( string fromState, string toState )
	{
		// Set Just Jumped flag to prevent instant transition to double jump
		m_justJumped = true;

		// Play jump sound effect
		m_soundJump.Play ();

		switch (fromState) {
		case "WallrunSide":
		case "WallrunUp":
			// Base jump direction on camera facing
			Vector3 jumpMove = m_capsuleCollider.transform.rotation * Vector3.forward;
			var jumpAngle = Vector3.Angle ( m_wallrunNormal, jumpMove );
			
			// If the jump direction is parallel to the wall, kick them off at an angle
			if( Mathf.Abs ( jumpAngle - 90.0f ) < m_settings.wallrunKickAngle )
			{
				jumpMove = Quaternion.AngleAxis( ( 90.0f - m_settings.wallrunKickAngle ) * Mathf.Sign( Vector3.Cross ( m_wallrunNormal, jumpMove ).y ), -m_gravity ) * m_wallrunNormal;
			}
			
			// If the jump direction is toward the wall, reflect it
			if( jumpAngle > 90.0f + m_settings.wallrunKickAngle )
			{
				jumpMove = Vector3.Reflect( jumpMove, m_wallrunNormal );
			}

			var direction = jumpMove.normalized * m_maxVel;
			direction.y = m_settings.jumpDeltaV;
			m_rigidbody.velocity = direction;
			
			// Reset the Air Dash
			if( m_airDashWallAvailable )
			{
				m_airDashAvailable = true;
				m_airDashWallAvailable = false;
			}

			break;
		case "Dash":
		case "DashStop":
			// Reset the ground dash cooldown to allow for two jumps after dashing off a platform
			m_groundDashCount = 0;
			m_groundDashTimer = 0.0f;
			goto default;
		case "BoostDash":
			// Reset the Air Dash
			if( m_airDashWallAvailable )
			{
				m_airDashAvailable = true;
				m_airDashWallAvailable = false;
			}

			var jumpOffVel = (m_capsuleCollider.transform.rotation * Vector3.forward).normalized * m_maxVel;
			jumpOffVel = Vector3.Slerp ( jumpOffVel, m_wallrunNormal, 0.5f );
			jumpOffVel.y = m_settings.jumpDeltaV;
			m_rigidbody.velocity = jumpOffVel;
			break;
		default:
			Jump ();
			break;
		}
	}
	
	void StateJump()
	{
		// Behaviour
		AirControl ();
		ApplyGravity( m_settings.gravityStrength );

		// State Transitions
		if ( Vector3.Dot( m_rigidbody.velocity, m_gravity ) > 0.5f) {
			fsm.SwitchState( "Fall" );
		}
		
		if (m_shouldJump && !m_justJumped && m_doubleJumpAvailable) {
			fsm.SwitchState( "DoubleJump" );
		}
		m_justJumped = false;
		
		if (m_shouldDash) {
			if ( m_airDashAvailable ) {
				fsm.SwitchState( "Dash" );
			}
		}
		
		if (InputManager.instance.PressInput.crouch) {
			fsm.SwitchState( "FastFall" );
		}
		
		CheckWallrun ();
		CheckWallkick ();
	}
	
	void StateFall()
	{
		// Behaviour
		AirControl ();
		ApplyGravity( m_settings.gravityStrength );
		GroundPush ();

		// State Transitions
		if (m_grounded) {
			fsm.SwitchState( "Idle" );
		}
		
		if (m_shouldJump && m_doubleJumpAvailable) {
			fsm.SwitchState( "DoubleJump" );
		}
		
		if (m_shouldDash) {
			if ( m_airDashAvailable ) {
				fsm.SwitchState( "Dash" );
			}
		}
		
		if (InputManager.instance.PressInput.crouch) {
			fsm.SwitchState( "FastFall" );
		}
		
		CheckWallrun ();
		CheckWallkick ();
	}
	
	void StateDoubleJumpEnter( string fromState, string toState )
	{
		// Mark Double Jump as used
		m_doubleJumpAvailable = false;

		// Play jump sound effect
		m_soundJump.Play ();

		Jump ();
	}
	
	void StateDoubleJump()
	{
		// Behaviour
		AirControl ();
		ApplyGravity( m_settings.gravityStrength );

		// State Transitions
		if ( Vector3.Dot( m_rigidbody.velocity, m_gravity ) > 0.5f) {
			fsm.SwitchState( "Fall" );
		}
		
		if (m_shouldDash) {
			if ( m_airDashAvailable ) {
				fsm.SwitchState( "Dash" );
			}
		}
		
		if (InputManager.instance.PressInput.crouch) {
			fsm.SwitchState( "FastFall" );
		}
		
		CheckWallrun ();
	}
	
	void StateDashEnter( string fromState, string toState )
	{
		// Set maximum velocity to dash speed
		m_maxVel = m_settings.dashDeltaV;
		m_targetMaxVel = m_maxVel;

		// Calculate dash velocity and apply it to the controller
		var dashVel = m_capsuleCollider.transform.rotation * InputManager.instance.CurrentInput.movement.normalized * m_settings.dashDeltaV;
		m_rigidbody.velocity = dashVel;

		// Set the dash timer to the appropriate duration
		m_dashTimer = m_settings.dashDuration;

		// Play the dash sound effect
		m_soundDash.Play ();

		if( m_grounded )
		{
			// Increment the ground dash count and reset the cooldown timer
			m_groundDashCount++;
			m_groundDashTimer = m_settings.dashRechargeDuration;
		}
		else
		{
			// Mark Air Dash as used
			m_airDashAvailable = false;
		}
	}
	
	void StateDash()
	{
		// Behaviour
		// Decrement dash timer
		m_dashTimer -= Time.fixedDeltaTime;
		
		// Tilt camera in direction of dash
		var tiltAxis = Vector3.Cross (-m_gravity, m_lateralVelocity);
		m_targetBase = Quaternion.AngleAxis (m_settings.dashTilt, tiltAxis) * Quaternion.FromToRotation (Vector3.up, -m_gravity);

		// Zero vertical velocity
		m_rigidbody.velocity = m_lateralVelocity;

		// State Transitions
		if (m_dashTimer <= 0.0f) {
			fsm.SwitchState( "DashStop" );
		}
		
		if (m_shouldJump) {
			if( m_grounded || m_groundDashCount > 0 )
			{
				fsm.SwitchState( "Jump" );
			}
			else if( m_doubleJumpAvailable )
			{
				fsm.SwitchState( "DoubleJump" );
			}
		}
		
		CheckWallrun ();
	}
	
	void StateDashExit( string fromState, string toState )
	{
		if (toState != "DashStop") {
			// Reset target max velocity
			m_targetMaxVel = m_settings.runSpeed;

			// Reset base rotation to the opposite of gravity
			ResetBaseRotation( true );
		}
	}
	
	void StateDashStopEnter( string fromState, string toState )
	{
		// Start Dash Stop timer
		m_dashTimer = m_settings.dashStopDuration;
		
		// Reset camera base to the opposite of gravity
		ResetBaseRotation ( true );
	}
	
	void StateDashStop()
	{
		// Behaviour
		// Decrement Dash Timer
		m_dashTimer -= Time.fixedDeltaTime;
		
		// Apply stopping velocity, zero vertical velocity
		var stopVel = m_lateralVelocity * m_settings.dashStopFactor;
		m_rigidbody.velocity = stopVel;

		// State Transitions
		if (m_dashTimer <= 0.0f) {
			if( m_grounded )
			{
				fsm.SwitchState( "Idle" );
			}
			else
			{
				fsm.SwitchState( "Fall" );
			}
		}
		
		if (m_shouldJump) {
			if( m_grounded || m_groundDashCount > 0 )
			{
				fsm.SwitchState( "Jump" );
			}
			else if( m_doubleJumpAvailable )
			{
				fsm.SwitchState( "DoubleJump" );
			}
		}
		
		if (m_shouldDash) {
			var groundDash = m_grounded && m_groundDashCount < 3;
			var airDash = !m_grounded && m_airDashAvailable;
			
			if ( groundDash || airDash ) {
				fsm.SwitchState( "Dash" );
			}
		}

		if (InputManager.instance.PressInput.crouch && m_grounded && InputManager.instance.CurrentInput.movement.sqrMagnitude > 0.0f) {
			fsm.SwitchState( "SlideDash" );
		}
		
		if (InputManager.instance.PressInput.crouch && !m_grounded) {
			fsm.SwitchState( "FastFall" );
		}
		
		CheckWallrun ();
	}
	
	void StateDashStopExit( string fromState, string toState )
	{
		// Reset dash timer
		m_dashTimer = 0.0f;

		// Reset maxVel to standard run speed
		// Don't hard reset on jump cancel
		if (toState == "Jump" || toState == "DoubleJump") {
			m_targetMaxVel = m_settings.runSpeed;
		} else {
			m_maxVel = m_settings.runSpeed;
			m_targetMaxVel = m_maxVel;
		}
	}

	void StateWallrunSideEnter( string fromState, string toState )
	{
		m_justWallran = true;

		// Clamp the player's initial vertical velocity between 0 and the set maximum
		Vector3 clampVel = m_rigidbody.velocity;
		if (clampVel.y > m_settings.wallrunMaxJump) {
			clampVel.y = m_settings.wallrunMaxJump;
		} else if (clampVel.y < 0.0f) {
			clampVel.y = 0.0f;
		}
		m_rigidbody.velocity = clampVel;
	
		// If the player is moving faster than the max wallrun speed
		// Set the max velocity to allow it, but set the lerp target to interpolate back down
		if (m_rigidbody.velocity.magnitude > m_settings.wallrunSpeed) {
			m_maxVel = m_rigidbody.velocity.magnitude;
		} else {
			m_maxVel = m_settings.wallrunSpeed;
		}
		m_targetMaxVel = m_settings.wallrunSpeed;
	
		// Tilt the camera away from the wall
		m_targetBase = Quaternion.AngleAxis (m_settings.wallrunTilt, m_wallrunTiltAxis) * Quaternion.FromToRotation (Vector3.up, -m_gravity);
		
		// Play Wall Cling sound
		m_soundWallCling.Play();
	}
	
	void StateWallrunSide()
	{
		// Behaviour
		bool fallOff = m_lateralVelocity.magnitude < m_settings.wallrunStickSpeed  || ( InputManager.instance.PressInput.crouch );
		bool runOff = !Physics.Raycast( m_capsuleCollider.transform.position, -m_wallrunNormal, m_settings.wallrunStickDistance ) || m_grounded;

		// If the controller's velocity is lower than the minimum Wallrun speed, increase it appropriately
		if( m_lateralVelocity.magnitude < m_settings.wallrunSpeed )
		{
			var moveVel = m_wallrunDirection.normalized * m_settings.wallrunSpeed;
			m_rigidbody.velocity = moveVel + m_verticalVelocity;
		}
		
		// Limit maximum velocity
		var latVel = m_lateralVelocity;
		if (latVel.magnitude > m_maxVel) {
			latVel = latVel.normalized * m_maxVel;
		}
		m_rigidbody.velocity = latVel + m_verticalVelocity;
		
		ApplyGravity (m_settings.wallrunGravityStrength);
		
		ApplyHeadbob ( 1.2f );

		// State Transitions
		if (InputManager.instance.PressInput.dash && !m_justWallran) {
			fsm.SwitchState( "BoostDash" );
		}
		m_justWallran = false;
		
		if( fallOff || runOff )
		{
			fsm.SwitchState ( "Fall" );
		}
		
		if (m_shouldJump) {
			fsm.SwitchState( "Jump" );
		}
	}
	
	void StateWallrunSideExit( string fromState, string toState )
	{
		// Reset target base rotation to the opposite of gravity
		ResetBaseRotation ( true );
	}
	
	void StateWallrunUpEnter( string fromState, string toState )
	{
		m_justWallran = true;

		// Boost the player upwards
		if (Vector3.Dot (m_verticalVelocity, m_gravity) <= 0.0f || fromState == "Dash" || fromState == "DashStop") {
			var runUpVel = -m_gravity * m_settings.wallrunUpDeltaV;
			m_rigidbody.velocity = runUpVel;
		} else {
			m_rigidbody.velocity *= m_settings.wallrunUpGravityStrength;
		}

		// Tilt the camera away from the wall
		m_targetBase = Quaternion.AngleAxis (m_settings.wallrunTilt, m_wallrunTiltAxis) * Quaternion.FromToRotation (Vector3.up, -m_gravity);

		// Play Wall Cling sound
		m_soundWallCling.Play();
	}
	
	void StateWallrunUp()
	{
		// Behaviour
		WallPush ();
		
		ApplyGravity (m_settings.wallrunUpGravityStrength);

		if (m_rigidbody.velocity.y > 0.0f) {
			ApplyHeadbob ( 1.7f );
		}

		// State Transitions
		// Check to see if the player has fallen or run off the wall, or hit the ground
		bool fallOff = m_grounded && m_verticalVelocity.magnitude < m_settings.gravityStrength || ( InputManager.instance.PressInput.crouch );
		bool runOffCenter = !Physics.Raycast( m_capsuleCollider.transform.position, -m_wallrunNormal, m_settings.wallrunStickDistance );
		bool runOffBottom = !Physics.Raycast( m_capsuleCollider.transform.position + ( m_gravity * m_capsuleCollider.height * m_settings.wallrunRayHeightBottom ), -m_wallrunNormal, m_settings.wallrunStickDistance );
		
		if( fallOff )
		{
			fsm.SwitchState( "Fall" );
		}

		if( runOffBottom && runOffCenter )
		{
			if( m_verticalVelocity.y > 0.0f )
			{
				fsm.SwitchState( "ClimbOver" );
			}
		}

		if( runOffBottom )
		{
			if( m_verticalVelocity.y <= 0.0f )
			{
				fsm.SwitchState( "Fall" );
			}
		}
		
		if (m_shouldJump) {
			fsm.SwitchState( "Jump" );
		}
		
		if (InputManager.instance.CurrentInput.dash ) {
			if( m_verticalVelocity.y > 0.0f )
			{
				if( !InputManager.instance.PreviousInput.dash && !m_justWallran && m_lungeUpAvailable )
				{
					fsm.SwitchState( "LungeUp" );
				}
			}
			else
			{
				if( !InputManager.instance.PreviousInput.dash && !m_justWallran && m_lungeUpAvailable )
				{
					fsm.SwitchState( "LungeUp" );
				}
				else
				{
					m_rigidbody.velocity = m_lateralVelocity;
				}
			}
		}
		m_justWallran = false;
	}
	
	void StateWallrunUpExit( string fromState, string toState )
	{
		// Reset target base rotation to the opposite of gravity
		ResetBaseRotation ( true );
	}
	
	void StateClimbOver()
	{
		// Behaviour
		// Push the controller into the wall
		var runInVel = -m_wallrunNormal * m_settings.climbOverDeltaV;
		m_rigidbody.velocity = runInVel;

		// State Transitions
		if (Physics.Raycast (m_rigidbody.position, m_gravity, m_capsuleCollider.height * 0.6f)) {
			fsm.SwitchState( "Idle" );
		}
	}
	
	void OnDrawGizmos()
	{
		if (Application.isPlaying) {
			Gizmos.color = Color.red;
			Gizmos.DrawLine (m_capsuleCollider.transform.position, m_capsuleCollider.transform.position + m_wallrunNormal);
			Gizmos.color = Color.green;
			Gizmos.DrawLine (m_capsuleCollider.transform.position, m_capsuleCollider.transform.position + m_wallrunTiltAxis);
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine (m_capsuleCollider.transform.position, m_capsuleCollider.transform.position + m_wallrunDirection);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine (m_capsuleCollider.transform.position, m_capsuleCollider.transform.position + m_gravity);

			Vector3 topLeft = m_capsuleCollider.transform.position + (-m_wallrunTiltAxis * m_capsuleCollider.radius) + ( -m_gravity * m_capsuleCollider.height * m_settings.wallrunRayHeightTop ) - m_wallrunNormal * m_capsuleCollider.radius;
			Vector3 topRight = m_capsuleCollider.transform.position + (m_wallrunTiltAxis * m_capsuleCollider.radius) + ( -m_gravity * m_capsuleCollider.height * m_settings.wallrunRayHeightTop ) - m_wallrunNormal * m_capsuleCollider.radius;
			Vector3 bottomLeft = m_capsuleCollider.transform.position + (-m_wallrunTiltAxis * m_capsuleCollider.radius) + ( -m_gravity * m_capsuleCollider.height * -m_settings.wallrunRayHeightBottom ) - m_wallrunNormal * m_capsuleCollider.radius;
			Vector3 bottomRight = m_capsuleCollider.transform.position + (m_wallrunTiltAxis * m_capsuleCollider.radius) + ( -m_gravity * m_capsuleCollider.height * -m_settings.wallrunRayHeightBottom ) - m_wallrunNormal * m_capsuleCollider.radius;

			//if( fsm.CurrentState == "WallrunSide" || fsm.CurrentState == "WallrunUp" || fsm.CurrentState == "BoostDash" )
			//{
				Gizmos.color = Color.magenta;
				Gizmos.DrawLine (topLeft, topRight);
				Gizmos.DrawLine (topRight, bottomRight);
				Gizmos.DrawLine (bottomRight, bottomLeft);
				Gizmos.DrawLine (bottomLeft, topLeft);
			//}
		}
	}

	void StateBoostDashEnter( string fromState, string toState )
	{
		// Set maximum velocity to dash speed, interpolate back down to boost dash speed
		m_maxVel = m_settings.dashDeltaV;
		m_targetMaxVel = m_settings.boostDashSpeed;

		// Calculate movement vector based on which wallrun we transitioned from and apply
		Vector3 moveVel = Vector3.zero;

		Vector3 va = -m_wallrunNormal;
		Vector3 vb = m_capsuleCollider.transform.rotation * Vector3.forward;
		float sign = Mathf.Sign( Vector3.Dot (-m_gravity, Vector3.Cross (va, vb)) );

		// Start moving along the wallrun vector at maximum velocity
		moveVel = m_wallrunTiltAxis * -sign * m_maxVel;

		// Start Boost Dash timer
		m_boostDashTimer = m_settings.boostDashSideDuration;
		m_rigidbody.velocity = moveVel;

		// Rotate the controller 90 degrees away from the wall
		m_targetBase = Quaternion.FromToRotation (Vector3.up, m_wallrunNormal);

		// Set the pitch offset to animate back to zero pitch
		m_targetPitchOffset = -m_pitch;

		// Play dash sound effect
		m_soundDash.Play();
	}

	void StateBoostDash()
	{
		// Constrain velocity to maximum
		if (m_rigidbody.velocity.magnitude > m_maxVel) {
			m_rigidbody.velocity = m_rigidbody.velocity.normalized * m_maxVel;
		}

		// Count down Boost Dash timer
		m_boostDashTimer -= Time.fixedDeltaTime;

		ApplyHeadbob ( 1.0f );

		// State Transitions
		bool fallOff = m_rigidbody.velocity.magnitude < m_settings.wallrunStickSpeed  || ( InputManager.instance.PressInput.crouch );
		bool runOff = !Physics.Raycast( m_capsuleCollider.transform.position, -m_wallrunNormal, m_settings.wallrunStickDistance );

		if (fallOff || runOff) {
			fsm.SwitchState( "Fall" );
		}

		if ( ( m_shouldJump ) || m_boostDashTimer <= 0.0f ) {
			fsm.SwitchState( "Jump" );
		}
	}
	
	void StateBoostDashExit( string fromState, string toState )
	{
		ResetBaseRotation (true);
		
		// Set the pitch offset to animate back to zero pitch
		m_targetPitchOffset = -m_pitch;
	}
	
	void StateLungeUpEnter( string fromState, string toState )
	{
		m_soundDash.Play();

		m_rigidbody.velocity = -m_gravity * m_settings.lungeUpDeltaV;

		m_lungeUpAvailable = false;
	}
	
	void StateLungeUp()
	{
		ApplyGravity( m_settings.gravityStrength );

		// State Transitions
		// Check to see if the player has fallen or run off the wall, or hit the ground
		bool fallOff = Vector3.Dot (m_rigidbody.velocity, m_gravity) > 0.5f;
		bool runOffCenter = !Physics.Raycast( m_capsuleCollider.transform.position, -m_wallrunNormal, m_settings.wallrunStickDistance );
		bool runOffBottom = !Physics.Raycast( m_capsuleCollider.transform.position + ( m_gravity * m_capsuleCollider.height * m_settings.wallrunRayHeightBottom ), -m_wallrunNormal, m_settings.wallrunStickDistance );
		
		if( fallOff )
		{
			fsm.SwitchState( "Fall" );
		}
		
		if( runOffBottom && runOffCenter )
		{
			if( m_verticalVelocity.y > 0.0f )
			{
				fsm.SwitchState( "ClimbOver" );
			}
		}
		
		if( runOffBottom )
		{
			if( m_verticalVelocity.y <= 0.0f )
			{
				fsm.SwitchState( "Fall" );
			}
		}
		
		if (m_shouldJump && !m_justJumped ) {
			fsm.SwitchState( "Jump" );
		}
		m_justJumped = false;
	}
	
	void StateLungeUpExit( string fromState, string toState )
	{
		ResetBaseRotation ( true );

		// Set the pitch offset to animate back to zero pitch
		m_targetPitchOffset = -m_pitch;
	}

	void StateFastFallEnter( string fromState, string toState )
	{
		m_maxVel = m_settings.dashDeltaV;

		var jumpVel = InputManager.instance.CurrentInput.movement.normalized * m_maxVel;
		jumpVel.y = -m_settings.dashDeltaV;
		jumpVel = m_capsuleCollider.transform.rotation * jumpVel;
		m_rigidbody.velocity = jumpVel;
		
		// Play the dash sound effect
		m_soundDash.Play ();
	}

	void StateFastFall()
	{
		// Behaviour
		ApplyGravity( m_settings.gravityStrength );
		GroundPush ();
		
		// State Transitions
		if (m_grounded) {
			fsm.SwitchState( "Idle" );
		}
		
		if (m_shouldJump && m_doubleJumpAvailable) {
			fsm.SwitchState( "DoubleJump" );
		}
		
		if (m_shouldDash) {
			if ( m_airDashAvailable ) {
				fsm.SwitchState( "Dash" );
			}
		}
	}

	void StateSlideDashEnter( string fromState, string toState )
	{
		// Set maximum velocity to dash speed
		m_maxVel = m_settings.slideDashDeltaV;
		m_targetMaxVel = m_maxVel;
		
		// Calculate dash velocity and apply it to the controller
		var dashVel = m_capsuleCollider.transform.rotation * InputManager.instance.CurrentInput.movement.normalized * m_settings.slideDashDeltaV;
		m_rigidbody.velocity = dashVel;
		
		// Set the dash timer to the appropriate duration
		m_dashTimer = m_settings.slideDashDuration;
		
		// Play the dash sound effect
		m_soundDash.Play ();

		// Resize collider
		m_capsuleCollider.enabled = false;
		m_slideCollider.gameObject.SetActive( true );
	}
	
	void StateSlideDash()
	{
		// Behaviour
		// Decrement dash timer
		m_dashTimer -= Time.fixedDeltaTime;

		// Tilt camera in direction of dash
		var tiltAxis = Vector3.Cross (-m_gravity, m_lateralVelocity);
		m_targetBase = Quaternion.AngleAxis (-m_settings.slideDashTilt, tiltAxis) * Quaternion.FromToRotation (Vector3.up, -m_gravity);

		m_targetCameraOffset = m_defaultCameraOffset + new Vector3( 0.0f, -m_settings.slideDashVerticalOffset, 0.0f );

		// Zero vertical velocity
		m_rigidbody.velocity = m_lateralVelocity.normalized * m_maxVel;

		// State Transitions
		if (m_dashTimer <= 0.0f && !m_ceilingCheck) {
			fsm.SwitchState( "SlideDashStop" );
		}
		
		if (m_shouldJump) {
			fsm.SwitchState( "Jump" );
		}

		if (!m_grounded) {
			fsm.SwitchState( "Fall" );
		}
		
		CheckWallrun ();
	}
	
	void StateSlideDashExit( string fromState, string toState )
	{
		if (toState != "SlideDashStop") {
			// Reset target max velocity
			m_targetMaxVel = m_settings.runSpeed;
			
			// Reset base rotation to the opposite of gravity
			ResetBaseRotation( true );
		}

		// Reset collider size
		m_slideCollider.gameObject.SetActive( false );
		m_capsuleCollider.enabled = true;
	}
	
	void StateSlideDashStopEnter( string fromState, string toState )
	{
		// Start Dash Stop timer
		m_dashTimer = m_settings.slideDashStopDuration;
		
		// Reset camera base to the opposite of gravity
		ResetBaseRotation ( true );
	}
	
	void StateSlideDashStop()
	{
		// Behaviour
		// Decrement Dash Timer
		m_dashTimer -= Time.fixedDeltaTime;
		
		// Apply stopping velocity, zero vertical velocity
		var stopVel = m_lateralVelocity * m_settings.slideDashStopFactor;
		m_rigidbody.velocity = stopVel;
		
		// State Transitions
		if (m_dashTimer <= 0.0f) {
			fsm.SwitchState( "Idle" );
		}
		
		if (!m_grounded) {
			fsm.SwitchState( "Fall" );
		}
		
		if (m_shouldJump) {
			fsm.SwitchState( "Jump" );
		}
	}
	
	void StateSlideDashStopExit( string fromState, string toState )
	{
		// Reset dash timer
		m_dashTimer = 0.0f;
		
		// Reset maxVel to standard run speed
		// Don't hard reset on jump cancel
		if (toState == "Jump") {
			m_targetMaxVel = m_settings.runSpeed;
		} else {
			m_maxVel = m_settings.runSpeed;
			m_targetMaxVel = m_maxVel;
		}
	}
	
	// Utility Methods
	void ApplyGravity( float strength )
	{
		m_rigidbody.velocity += m_gravity * strength;
	}

	float prevBob = 0.0f;
	void ApplyHeadbob( float strength )
	{
		m_cameraMomentum += m_rigidbody.velocity.magnitude * strength;

		float xBob = Mathf.Sin((0.5f + m_cameraMomentum) * m_settings.headbobFrequency);
		if (Mathf.Abs (xBob) >= 0.9f && Mathf.Abs (prevBob) < 0.9f) {
			m_soundFootstep.Play();
		}
		prevBob = xBob;

		float moveFactor = 1.0f;

		if (fsm.CurrentState == "Run") {
			moveFactor = InputManager.instance.CurrentInput.movement.magnitude;
		}

		m_cameraHeadbob.x = xBob * 0.5f * m_settings.headbobXFactor * moveFactor;
		m_cameraHeadbob.y = -Mathf.Abs( Mathf.Sin( m_cameraMomentum * m_settings.headbobFrequency ) ) * 0.5f * m_settings.headbobYFactor * moveFactor;
	}

	void Jump()
	{
		var jumpVel = InputManager.instance.CurrentInput.movement.normalized * m_maxVel;
		jumpVel.y = m_settings.jumpDeltaV;
		jumpVel = m_capsuleCollider.transform.rotation * jumpVel;
		m_rigidbody.velocity = jumpVel;
	}

	void GroundPush()
	{
		if (m_groundPush && !m_grounded) {
			var pushVel = m_verticalVelocity;
			pushVel += m_gravity * m_settings.groundPushForce;
			m_rigidbody.velocity = m_lateralVelocity + pushVel;
		}
	}

	void WallPush()
	{
		var runInVel = -m_wallrunNormal * m_settings.climbOverDeltaV;
		m_rigidbody.velocity = runInVel + m_verticalVelocity;
	}

	void AirControl()
	{
		if (m_yawOffset != 0.0f) {
			return;
		}

		// Calculate velocity change from directional input, rotate based on collider facing
		var deltaV = m_capsuleCollider.transform.rotation * InputManager.instance.CurrentInput.movement * m_settings.airDeltaV;
		
		// Separate lateral and vertical velocities
		var vertVel = m_verticalVelocity;
		var latVel = m_lateralVelocity;
		
		// If the player is inputting movement, add lateral velocity change
		if (deltaV.sqrMagnitude > 0.0f ) {
			latVel += deltaV;
		}

		// Clamp to max speed
		if (latVel.magnitude > m_maxVel) {
			latVel = latVel.normalized * m_maxVel;
		}
		
		// Recombine lateral and vertical velocities and apply
		m_rigidbody.velocity = latVel + vertVel;
	}
	
	void CheckWallrun()
	{
		Vector3 forward = m_capsuleCollider.transform.rotation * Vector3.forward;
		
		Vector3 leftRayDir = m_capsuleCollider.transform.rotation * Quaternion.Euler( 0.0f, -m_settings.wallrunRayAngle, 0.0f ) * Vector3.forward;
		RaycastHit leftHitInfo = new RaycastHit();
		bool leftHit = RaycastWall (forward, leftRayDir, out leftHitInfo, m_settings.wallrunRayHeightTop, m_settings.wallrunRayHeightBottom, m_settings.wallrunStickDistance);
		Vector3 leftWallrunDirection = Vector3.zero;
		Vector3 leftWallrunTiltAxis = Vector3.zero;
		
		Vector3 rightRayDir = m_capsuleCollider.transform.rotation * Quaternion.Euler( 0.0f, m_settings.wallrunRayAngle, 0.0f ) * Vector3.forward;
		float leftSignedDot = Vector3.Dot( forward, -leftHitInfo.normal ) * Mathf.Sign(Vector3.Cross(forward, -leftHitInfo.normal).y);
		if (leftSignedDot < 0.0f) {
			leftWallrunTiltAxis = Vector3.Cross (-leftHitInfo.normal, -m_gravity);
			leftWallrunDirection = -leftWallrunTiltAxis;
		} else {
			leftHit = false;
		}
		
		RaycastHit rightHitInfo = new RaycastHit();
		bool rightHit = RaycastWall (forward, rightRayDir, out rightHitInfo, m_settings.wallrunRayHeightTop, m_settings.wallrunRayHeightBottom, m_settings.wallrunStickDistance);
		Vector3 rightWallrunDirection = Vector3.zero;
		Vector3 rightWallrunTiltAxis = Vector3.zero;
		
		float rightSignedDot = Vector3.Dot( forward, -rightHitInfo.normal ) * Mathf.Sign(Vector3.Cross(forward, -rightHitInfo.normal).y);
		if (rightSignedDot > 0.0f) {
			rightWallrunTiltAxis = Vector3.Cross (-rightHitInfo.normal, -m_gravity);
			rightWallrunDirection = rightWallrunTiltAxis;
		} else {
			rightHit = false;
		}
		
		if (leftHit && rightHit) {
			if( leftHitInfo.distance < rightHitInfo.distance )
			{
				rightHit = false;
			}
			else if( leftHitInfo.distance > rightHitInfo.distance )
			{
				leftHit = false;
			}
			else
			{
				leftHit = false;
				rightHit = false;
			}
		}
		
		Vector3 normal = new Vector3();
		Vector3 tiltAxis = new Vector3();
		Vector3 direction = new Vector3();
		
		if (leftHit) {
			normal = leftHitInfo.normal;
			tiltAxis = leftWallrunTiltAxis;
			direction = leftWallrunDirection;
		}
		
		if (rightHit) {
			normal = rightHitInfo.normal;
			tiltAxis = rightWallrunTiltAxis;
			direction = rightWallrunDirection;
		}
		
		bool hit = leftHit || rightHit;
		bool wallrunInput = InputManager.instance.CurrentInput.dash || fsm.CurrentState == "Dash";
		bool towardWall = Vector3.Dot (m_rigidbody.velocity.normalized, -normal) > 0.0f;
		bool fastEnough = m_rigidbody.velocity.magnitude > m_settings.wallrunStickSpeed;
		
		if (hit && wallrunInput && towardWall && fastEnough) {
			bool sideways = Vector3.Angle (-normal, forward) > 30.0f;
			
			// Set normal and direction
			m_wallrunNormal = normal;
			m_wallrunDirection = direction;
			m_wallrunTiltAxis = tiltAxis;
			
			if (sideways) {
				if(m_grounded)
				{
					return;
				}
				
				fsm.SwitchState ("WallrunSide");
			} else {
				// Prevent activation if the player is falling, or if they are grounded and not dashing
				if (m_grounded && !(fsm.CurrentState == "Dash" || fsm.CurrentState == "DashStop")) {
					return;
				}
				
				fsm.SwitchState ("WallrunUp");
			}
		}
	}
	
	void CheckWallkick()
	{
		Vector3 forward = m_capsuleCollider.transform.rotation * Vector3.forward;
		
		Vector3 leftRayDir = m_capsuleCollider.transform.rotation * Quaternion.Euler( 0.0f, -90.0f, 0.0f ) * Vector3.forward;
		RaycastHit leftHitInfo = new RaycastHit();
		bool leftHit = RaycastWall (forward, leftRayDir, out leftHitInfo, -0.25f, 0.25f, m_settings.wallrunStickDistance );
		
		Vector3 rightRayDir = m_capsuleCollider.transform.rotation * Quaternion.Euler( 0.0f, 90.0f, 0.0f ) * Vector3.forward;
		RaycastHit rightHitInfo = new RaycastHit();
		bool rightHit = RaycastWall (forward, rightRayDir, out rightHitInfo, -0.25f, 0.25f, m_settings.wallrunStickDistance );
		
		if (leftHit && rightHit) {
			if( leftHitInfo.distance < rightHitInfo.distance )
			{
				rightHit = false;
			}
			else if( leftHitInfo.distance > rightHitInfo.distance )
			{
				leftHit = false;
			}
			else
			{
				leftHit = false;
				rightHit = false;
			}
		}
		
		Vector3 normal = new Vector3();
		
		if (leftHit) {
			normal = leftHitInfo.normal;
		}
		
		if (rightHit) {
			normal = rightHitInfo.normal;
		}
		
		bool hit = leftHit || rightHit;

		bool moveAway = Vector3.Dot ( m_capsuleCollider.transform.rotation * InputManager.instance.CurrentInput.movement, normal ) > 0.25f;
		bool wallkickInput = moveAway && InputManager.instance.PressInput.jump;

		if (hit && wallkickInput) {
			m_soundLand.Play();
			fsm.SwitchState ("Jump");
		}
	}
	
	bool RaycastWall( Vector3 localForward, Vector3 rayDir, out RaycastHit hitInfo, float top, float bottom, float distance )
	{		
		bool hit = Physics.Raycast (m_capsuleCollider.transform.position, rayDir, out hitInfo);
		
		if (hit) {
			Vector3 hitNorm = -hitInfo.normal;
			hit = Physics.Raycast( m_capsuleCollider.transform.position, hitNorm, out hitInfo, distance );
			
			if(hit) {
				Vector3 adjacentAxis = Vector3.Cross ( hitNorm, -m_gravity );

				RaycastHit topLeftHitInfo = new RaycastHit();
				RaycastHit topRightHitInfo = new RaycastHit();
				RaycastHit bottomLeftHitInfo = new RaycastHit();
				RaycastHit bottomRightHitInfo = new RaycastHit();
				
				bool topLeftHit = Physics.Raycast( m_capsuleCollider.transform.position + (-adjacentAxis * m_capsuleCollider.radius) + ( -m_gravity * m_capsuleCollider.height * top ), hitNorm, out topLeftHitInfo, distance );
				bool topRightHit = Physics.Raycast( m_capsuleCollider.transform.position + (adjacentAxis * m_capsuleCollider.radius) + ( -m_gravity * m_capsuleCollider.height * top ), hitNorm, out topRightHitInfo, distance );
				bool bottomLeftHit = Physics.Raycast( m_capsuleCollider.transform.position + (-adjacentAxis * m_capsuleCollider.radius) + ( -m_gravity * m_capsuleCollider.height * -bottom ), hitNorm, out bottomLeftHitInfo, distance );
				bool bottomRightHit = Physics.Raycast( m_capsuleCollider.transform.position + (adjacentAxis * m_capsuleCollider.radius) + ( -m_gravity * m_capsuleCollider.height * -bottom ), hitNorm, out bottomRightHitInfo, distance );
				
				if( topLeftHit && topRightHit && bottomLeftHit && bottomRightHit )
				{
					// Fetch hit distances and round them to 4 decimal places to account for floating point error
					var tl = decimal.Round( (decimal)topLeftHitInfo.distance, 4, System.MidpointRounding.ToEven );
					var tr = decimal.Round( (decimal)topRightHitInfo.distance, 4, System.MidpointRounding.ToEven );
					var bl = decimal.Round( (decimal)bottomLeftHitInfo.distance, 4, System.MidpointRounding.ToEven );
					var br = decimal.Round( (decimal)bottomRightHitInfo.distance, 4, System.MidpointRounding.ToEven );
					
					// If the surface is non-tilted (all distances equal), allow the wallrun
					if( tl == tr && tr == bl && bl == br && br == tl )
					{
						return true;
					}
				}
			}
		}
		
		return false;
	}

	void ResetBaseRotation( bool animate )
	{
		m_targetBase = Quaternion.FromToRotation( Vector3.up, -m_gravity );
		if (!animate) {
			m_baseRotation = m_targetBase;
		}
	}

	void ResetCameraRotation()
	{
		m_yaw = m_capsuleCollider.transform.rotation.eulerAngles.y;
		m_pitch = m_capsuleCollider.transform.rotation.eulerAngles.x;
	}

	void ResetGravity()
	{
		// Reset gravity and target base rotation
		m_gravity = m_defaultGravity;
	}
	
	void TransferPitchOffset()
	{
		m_pitch += m_pitchOffset;
		m_pitchOffset = 0.0f;
		m_targetPitchOffset = 0.0f;
	}
	
	void TransferYawOffset()
	{
		m_yaw += m_yawOffset;
		m_yawOffset = 0.0f;
		m_targetYawOffset = 0.0f;
	}
}
