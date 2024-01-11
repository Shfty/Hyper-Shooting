using UnityEngine;
using System.Collections;

using XInputDotNetPure;

public class InputManager : MonoBehaviour {
	public static InputManager instance;

	public class InputData
	{

		public Vector2 menuMovement;
		public bool select;
		public bool cancel;
		public bool pause;

		public Vector3 movement;
		public Vector2 look;
		public bool jump;
		public bool crouch;
		public bool dash;
		public bool quickturn;
		public bool fire;
		public bool lockOn;
		public bool weaponSlot1;
		public bool weaponSlot2;
		public bool weaponSlot3;
		public int mousewheel;
	}

	// Member Variables
	InputData m_input = new InputData();
	InputData m_prevInput = new InputData();
	InputData m_downInput = new InputData();
	InputData m_upInput = new InputData();
	
	bool xInputConnected = false;
	PlayerIndex xInputIndex;
	GamePadState xInputState;
	GamePadState prevXInputState;

	// Public Variables
	public InputData CurrentInput {
		get {
			return m_input;
		}
	}

	public InputData PreviousInput {
		get {
			return m_prevInput;
		}
	}

	public InputData PressInput {
		get {
			return m_downInput;
		}
	}
	
	public InputData ReleaseInput {
		get {
			return m_upInput;
		}
	}

	// Use this for initialization
	void Start () {
		instance = this;
	}

	// Update is called once per frame
	void Update()
	{
		if (Time.timeScale == 0.0f) {
			PollInput();
		}

		m_input.mousewheel = (int)Input.GetAxisRaw ("Mousewheel");
	}
	
	// Update is called once per physics frame
	void FixedUpdate () {
		if (Time.timeScale != 0.0f) {
			PollInput();
		}
	}

	void PollInput() {
		// Copy current input state into previous input state
		m_prevInput.menuMovement = m_input.menuMovement;
		m_prevInput.select = m_input.select;
		m_prevInput.cancel = m_input.cancel;
		m_prevInput.pause = m_input.pause;
		
		m_prevInput.movement = m_input.movement;
		m_prevInput.look = m_input.look;
		m_prevInput.jump = m_input.jump;
		m_prevInput.dash = m_input.dash;
		m_prevInput.crouch = m_input.crouch;
		m_prevInput.quickturn = m_input.quickturn;
		m_prevInput.fire = m_input.fire;
		m_prevInput.lockOn = m_input.lockOn;
		
		// Update current input state
		// Find a PlayerIndex, for a single player game
		// Will find the first controller that is connected ans use it
		if (!xInputConnected || !prevXInputState.IsConnected)
		{
			for (int i = 0; i < 4; ++i)
			{
				PlayerIndex testPlayerIndex = (PlayerIndex)i;
				GamePadState testState = GamePad.GetState(testPlayerIndex);
				if (testState.IsConnected)
				{
					xInputIndex = testPlayerIndex;
					xInputConnected = true;
				}
			}
		}
		
		prevXInputState = xInputState;
		xInputState = GamePad.GetState(xInputIndex);
		
		// XZ Movement = WASD
		m_input.movement.x = Input.GetAxisRaw ("Horizontal") ;
		m_input.movement.z = Input.GetAxisRaw ("Vertical");
		
		// Camera look rotation = Mouse
		m_input.look.x = 0.0f;
		m_input.look.y = 0.0f;
		
		if (Cursor.lockState == CursorLockMode.Locked) {
			m_input.look.x += Input.GetAxisRaw ("Mouse X") * SaveManager.instance.ControlSettings.mouseSensitivity;
			m_input.look.y += -Input.GetAxisRaw ("Mouse Y") * SaveManager.instance.ControlSettings.mouseSensitivity;
		}
		
		// Key Inputs
		m_input.menuMovement = Vector2.zero;
		m_input.select = Input.GetAxisRaw ("Select") > 0.0f;
		m_input.cancel = Input.GetAxisRaw ("Cancel") > 0.0f;
		m_input.pause = m_input.cancel;
		
		m_input.jump = Input.GetAxisRaw ("Jump") > 0.0f;
		m_input.crouch = Input.GetAxisRaw ("Crouch") > 0.0f;
		m_input.dash = Input.GetAxisRaw ("Dash") > 0.0f;
		m_input.quickturn = Input.GetAxisRaw ("Quickturn") > 0.0f;
		m_input.fire = Input.GetMouseButton (0);
		m_input.lockOn = Input.GetMouseButton (1);
		m_input.weaponSlot1 = Input.GetKey( "1" );
		m_input.weaponSlot2 = Input.GetKey( "2" );
		m_input.weaponSlot3 = Input.GetKey( "3" );
		
		if (xInputConnected) {
			// XZ Movement = Left Stick
			m_input.movement.x += xInputState.ThumbSticks.Left.X;
			m_input.movement.z += xInputState.ThumbSticks.Left.Y;
			
			// Camera look rotation = Right Stick
			Vector2 rightStick = new Vector2( xInputState.ThumbSticks.Right.X, xInputState.ThumbSticks.Right.Y );
			
			m_input.look.x += rightStick.x * Mathf.Abs( rightStick.x ) * SaveManager.instance.ControlSettings.analogSensitivity;
			m_input.look.y -= rightStick.y * Mathf.Abs( rightStick.y ) * SaveManager.instance.ControlSettings.analogSensitivity;
			
			// Button Inputs
			if( xInputState.DPad.Up == ButtonState.Pressed )
				m_input.menuMovement += Vector2.up;
			
			if( xInputState.DPad.Down == ButtonState.Pressed )
				m_input.menuMovement -= Vector2.up;
			
			if( xInputState.DPad.Left == ButtonState.Pressed )
				m_input.menuMovement -= Vector2.right;
			
			if( xInputState.DPad.Right == ButtonState.Pressed )
				m_input.menuMovement += Vector2.right;
			
			m_input.select |= xInputState.Buttons.A == ButtonState.Pressed;
			m_input.cancel |= xInputState.Buttons.B == ButtonState.Pressed;
			m_input.pause |= xInputState.Buttons.Start == ButtonState.Pressed;
			
			m_input.jump |= xInputState.Buttons.LeftShoulder == ButtonState.Pressed;
			m_input.crouch |= xInputState.Buttons.RightStick == ButtonState.Pressed;
			m_input.dash |= xInputState.Buttons.RightShoulder == ButtonState.Pressed;
			m_input.quickturn |= xInputState.Buttons.LeftStick == ButtonState.Pressed;
			m_input.fire |= xInputState.Triggers.Right > 0.1f;
			m_input.lockOn |= xInputState.Triggers.Left > 0.1f;
			m_input.weaponSlot1 |= xInputState.DPad.Up == ButtonState.Pressed;
			m_input.weaponSlot2 |= xInputState.DPad.Right == ButtonState.Pressed;
			m_input.weaponSlot3 |= xInputState.DPad.Left == ButtonState.Pressed;
		} else {
			// XZ Movement = Left Stick
			m_input.movement.x += Input.GetAxisRaw ("DInput Left Stick X");
			m_input.movement.z += Input.GetAxisRaw ("DInput Left Stick Y");
			
			// Camera look rotation = Right Stick
			Vector2 rightStick = new Vector2( Input.GetAxisRaw ("DInput Right Stick X"), Input.GetAxisRaw ("DInput Right Stick Y") );
			m_input.look.x += rightStick.x * Mathf.Abs( rightStick.x ) * SaveManager.instance.ControlSettings.analogSensitivity;
			m_input.look.y += rightStick.y * Mathf.Abs( rightStick.y ) * SaveManager.instance.ControlSettings.analogSensitivity;
			
			// Button Inputs
			m_input.menuMovement.x = Input.GetAxisRaw ("DInput DPad X");
			m_input.menuMovement.y = Input.GetAxisRaw ("DInput DPad Y");
			
			m_input.select |= Input.GetAxisRaw ("DInput Select") > 0.0f;
			m_input.cancel |= Input.GetAxisRaw ("DInput Cancel") > 0.0f;
			m_input.pause |= Input.GetAxisRaw ("DInput Pause") > 0.0f;
			
			m_input.jump |= Input.GetAxisRaw ("DInput Jump") > 0.0f;
			m_input.crouch |= Input.GetAxisRaw ("DInput Crouch") > 0.0f;
			m_input.dash |= Input.GetAxisRaw ("DInput Dash") > 0.0f;
			m_input.quickturn |= Input.GetAxisRaw ("DInput Quickturn") > 0.0f;
			m_input.fire |= Input.GetAxisRaw ("DInput Triggers") > 0.1f;
			m_input.lockOn |= Input.GetAxisRaw ("DInput Triggers") < -0.1f;
			m_input.weaponSlot1 |= m_input.menuMovement.y > 0.0f;
			m_input.weaponSlot2 |= m_input.menuMovement.x > 0.0f;
			m_input.weaponSlot3 |= m_input.menuMovement.x < 0.0;
		}
		
		m_input.movement = Mathf.Min (m_input.movement.magnitude, 1.0f) * m_input.movement.normalized;
		
		// Update Press and Release states
		m_downInput.select = m_input.select && !m_prevInput.select;
		m_downInput.cancel = m_input.cancel && !m_prevInput.cancel;
		m_downInput.pause = m_input.pause && !m_prevInput.pause;
		
		m_downInput.jump = m_input.jump && !m_prevInput.jump;
		m_downInput.dash = m_input.dash && !m_prevInput.dash;
		m_downInput.crouch = m_input.crouch && !m_prevInput.crouch;
		m_downInput.quickturn = m_input.quickturn && !m_prevInput.quickturn;
		m_downInput.fire = m_input.fire && !m_prevInput.fire;
		m_downInput.lockOn = m_input.lockOn && !m_prevInput.lockOn;
		
		m_upInput.select = !m_input.select && m_prevInput.select;
		m_upInput.cancel = !m_input.cancel && m_prevInput.cancel;
		m_upInput.pause = !m_input.pause && m_prevInput.pause;
		
		m_upInput.jump = !m_input.jump && m_prevInput.jump;
		m_upInput.dash = !m_input.dash && m_prevInput.dash;
		m_upInput.crouch = !m_input.crouch && m_prevInput.crouch;
		m_upInput.quickturn = !m_input.quickturn && m_prevInput.quickturn;
		m_upInput.fire = !m_input.fire && m_prevInput.fire;
		m_upInput.lockOn = !m_input.lockOn && m_prevInput.lockOn;
	}
}
