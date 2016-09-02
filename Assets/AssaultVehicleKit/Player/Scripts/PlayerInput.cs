using UnityEngine;
using System.Collections;
using System;

namespace hebertsystems.AVK
{
	//  The main class capturing player input and converting it to
	//  player actions.  The player actions are exposed as static 
	//  members, easily accessible anywhere within the project.
	//
	public class PlayerInput : MonoBehaviour 
	{
		public static Action<char, KeyCode> keyDown;				// Key down event.
		public static Action<char, KeyCode> keyUp;					// Key up event.

		public bool pauseGame										// Property to pause the game and player input.
		{
			get {return gamePaused;}
			set {gamePaused = value;}
		}
		
		public static float cameraVertical = 0;						// Vertical input of the camera.
		public static float cameraHorizontal = 0;					// Horizontal input of the camera.
		
		public static float vehicleVertical = 0;					// Vertical input of the vehicle (forward [postive] and backward [negative]).
		public static float vehicleHorizontal = 0;					// Horizontal input of the vehicle (right [positive] and left [negative]).

		public static bool handBrake = false;						// Hand brake input.
		public static bool speedBoost = false;						// Speed boost input.

		public static bool primaryFire = false;						// Fire primary weapon.
		public static bool secondaryFire = false;					// Fire secondary weapon.

		public static bool cycleVehicle = false;					// Cycle vehicles (like switching weapons in typical FPS).

		[Header("Mouse Input Config")]
		public float mouseXSensitivity = 5;							// Mouse X axis sensitivity.
		public float mouseYSensitivity = 5;							// Mouse Y axis sensitivity.
		public bool invertMouseY = false;							// Invert the Mouse Y camera input.
		public float mouseSmoothTime = .1f;							// Smooth time for mouse raw input dampening.
		public float mousePrecisionThreshold = 2;					// Mouse movement threshold where precision input is calculated.

		[Header("Joystick Input Config")]
		public float axisXSensitivity = 3;							// Joystick X axis sensitivity for camera.
		public float axisYSensitivity = 3;							// Joystick Y axis sensitivity for camera.
		public bool invertAxisY = false;							// Invert joystick Y camera input.

		private float smoothMouseX = 0;
		private float smoothMouseY = 0;
		private float mouseXVelocity = 0;
		private float mouseYVelocity = 0;

		private bool gamePaused = false;

		void Start () 
		{			
			// Lock screen cursor initially.
			Utilities.LockCursor(true);
		}

		void Update()
		{
			// If the game is paused, zero camera input (keep the rest as is for PauseActionCamera).
			if(gamePaused)
			{
				cameraHorizontal = 0;
				cameraVertical = 0;

				// If the cursor is locked, unlock it.
				if(Utilities.CursorLocked()) Utilities.LockCursor(false);
			}
			else
			{
				// Vehicle inputs.
				vehicleVertical = Input.GetAxis("Vertical");
				vehicleHorizontal = Input.GetAxis("Horizontal");

				// Primary and Secondary weapon inputs.
				primaryFire = Input.GetButton("Fire1") | Input.GetAxis("Axis10") > .1f;
				secondaryFire = Input.GetButton("Fire2") | Input.GetAxis("Axis9") > .1f;

				// Mouse input for horizontal (X) and vertical (Y) camera movement.  
				// Dampen the raw input over mouseSmoothTime.
				smoothMouseX = Mathf.SmoothDamp(smoothMouseX, Input.GetAxisRaw("Mouse X") * mouseXSensitivity, ref mouseXVelocity, mouseSmoothTime);
				smoothMouseY = Mathf.SmoothDamp(smoothMouseY, Input.GetAxisRaw("Mouse Y") * (invertMouseY ? 1 : -1) * mouseYSensitivity, ref mouseYVelocity, mouseSmoothTime);

				// Calculate the ratio of the mouse input to the mousePrecisionThreshold.
				// Before threshold, mouse input is decreased for fine control, after the threshold input is left as it is.
				float xPrecisionRatio = mousePrecisionThreshold <= 0 ? 1 : Mathf.InverseLerp(0, mousePrecisionThreshold, Mathf.Abs(smoothMouseX));
				float yPrecisionRatio = mousePrecisionThreshold <= 0 ? 1 : Mathf.InverseLerp(0, mousePrecisionThreshold, Mathf.Abs(smoothMouseY));

				// Calculate final mouse input
				float finalMouseX = Mathf.Lerp(0, smoothMouseX, xPrecisionRatio);
				float finalMouseY = Mathf.Lerp(0, smoothMouseY, yPrecisionRatio);

				// Calculate Camera horizontal and vertical actions based on mouse or joystick input.
				cameraHorizontal = finalMouseX + Input.GetAxis("Axis4") * axisXSensitivity;
				cameraVertical = finalMouseY + Input.GetAxis("Axis5") * axisYSensitivity * (invertAxisY ? -1 : 1);

				// Hand brake tied to standard "Jump" input.
				handBrake = Input.GetButton("Jump");

				// Speed boost.
				speedBoost = Input.GetButton("Speed Boost");

				// Cycle vehicles
				cycleVehicle = Input.GetButtonDown("Cycle Vehicle");

				// Start scene over
				if(Input.GetKeyDown(KeyCode.O))
					Application.LoadLevel(0);

				// Release cursor
				if(Input.GetKeyDown(KeyCode.Escape))
				{
					Utilities.LockCursor(false);
				}

				// Lock cursor if player clicks in window and the cursor isn't already locked.
				if(!Utilities.CursorLocked() && Input.GetMouseButtonDown(0)) Utilities.LockCursor(true);
			}
		}

		// Detect key events and send out corresponding keyDown or keyUp events.
		// Note:  	These events can be more convenient to use in certain situations instead of checking for 
		//			Input.GetKeyDown(Keycode...) every frame.  Also, components will receive these events even when inactive
		//			and can respond accordingly (toggle their active state for instance).
		//        
		void OnGUI()
		{
			if(Event.current.type == EventType.keyDown) SendKeyDown(Event.current.character, Event.current.keyCode);
			if(Event.current.type == EventType.keyUp) SendKeyUp(Event.current.character, Event.current.keyCode);
		}

		void SendKeyDown(char c, KeyCode keyCode)
		{
			if(keyDown != null) keyDown(c, keyCode);
		}

		void SendKeyUp(char c, KeyCode keyCode)
		{
			if(keyUp != null) keyUp(c, keyCode);
		}
	}
}
