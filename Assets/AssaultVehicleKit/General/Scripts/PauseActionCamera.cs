using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  This component is included in AVK as an added bonus tool just for fun -
	//  used to pause game action in order to take screen shots from different angles
	//  and/or slow motion capture.  Also useful for debugging/tweaking certain effects.
	//  Place this component on the main camera to use.  When in pause action mode, this
	//  component will allow camera movement and rotation separate from normal controls,
	//  along with the time controls.  When pause action mode is off, the camera's local
	//  position and rotation will be returned to normal.
	//
	//  Controls:
	//
	//  Pause/Resume Action:  CAPS LOCK
	//        Speed Time Up:  Number Pad 6
	//       Slow Time Down:  Number Pad 4
	//            Stop Time:  Number Pad 5
	//
	//  Rotate Camera while holding Right-Mouse-Button:
	//  Translate Camera with WASDQE to move forward, left, backward, right, down, up, respectively.
	//
	public class PauseActionCamera : MonoBehaviour 
	{
		public float cameraSpeed = 5;						// Camera speed during pause action mode.
		public float speedBoostMultiplier = 4;				// Left Shift camera speed boost multiplier.
		public float mouseSensitivity = 5;					// Mouse sensitivity during pause action mode.
		public bool invertMouseY = false;					// Invert mouse Y controls during pause action mode.
		public float timeStep = .01f;						// Time step increase/decrease when in pause action mode.

		public GameObject HUD;								// A reference to the player HUD which can be turned on/off in pause action mode.

		private bool pauseAction = false;
		private PlayerInput playerInput;
		private float currentCameraSpeed;
		private float timeScale;
		private bool HUDDisplay = true;
		private Vector3 originalLocalPosition;
		private Quaternion originalLocalRotation;

		void Awake () 
		{
			// Obtain reference to PlayerInput to control game pause state for the rest of the game.
			playerInput = FindObjectOfType<PlayerInput>();
		}

		void Update()
		{
			// Toggle pause action mode.
			if(Input.GetKeyDown(KeyCode.CapsLock))
			{
				pauseAction = !pauseAction;

				// If pausing, set timescale to 0 and save original local position and rotaiton.
				if(pauseAction)
				{
					Time.timeScale = 0;
					timeScale = 0;
					HUDDisplay = true;
					originalLocalPosition = transform.localPosition;
					originalLocalRotation = transform.localRotation;
				}
				// Resuming action, set time back to 1 and reset local position and rotation.
				else
				{
					Time.timeScale = 1;
					transform.localPosition = originalLocalPosition;
					transform.localRotation = originalLocalRotation;
					if(HUD) HUD.SetActive(true);
				}

				if(playerInput) playerInput.pauseGame = pauseAction;
			}

			// If in pause action mode
			if(pauseAction)
			{
				// Camera speed boost key (LeftShift)
				if(Input.GetKey(KeyCode.LeftShift)) currentCameraSpeed = cameraSpeed * speedBoostMultiplier;
				else currentCameraSpeed = cameraSpeed;

				// WASDQE movement
				if(Input.GetKey(KeyCode.W)) transform.position += transform.forward * currentCameraSpeed * Time.unscaledDeltaTime;
				if(Input.GetKey(KeyCode.S)) transform.position += transform.forward * -currentCameraSpeed * Time.unscaledDeltaTime;
				if(Input.GetKey(KeyCode.A)) transform.position += transform.right * -currentCameraSpeed * Time.unscaledDeltaTime;
				if(Input.GetKey(KeyCode.D)) transform.position += transform.right * currentCameraSpeed * Time.unscaledDeltaTime;
				if(Input.GetKey(KeyCode.Q)) transform.position += transform.up * -currentCameraSpeed * Time.unscaledDeltaTime;
				if(Input.GetKey(KeyCode.E)) transform.position += transform.up * currentCameraSpeed * Time.unscaledDeltaTime;
			
				// Time controls:  speed up, slow down, stop time.
				if(Input.GetKey(KeyCode.Keypad6)) {timeScale = Mathf.Clamp(timeScale + timeStep, 0f, 100f); Time.timeScale = timeScale;}
				if(Input.GetKey(KeyCode.Keypad4)) {timeScale = Mathf.Clamp(timeScale - timeStep, 0f, 100f); Time.timeScale = timeScale;}
				if(Input.GetKey(KeyCode.Keypad5)) {timeScale = 0; Time.timeScale = timeScale;}

				// Toggle display of the player HUD
				if(Input.GetKeyDown(KeyCode.U)) 
				{
					HUDDisplay = !HUDDisplay;
					if(HUD) HUD.SetActive(HUDDisplay);
				}

				// Camera rotation
				if(Input.GetMouseButton(1))
				{
					transform.rotation = Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * (invertMouseY ? 1 : -1) * mouseSensitivity, transform.right) *
						Quaternion.AngleAxis(Input.GetAxis("Mouse X") * mouseSensitivity, Vector3.up) * transform.rotation;
				}
			}
		}
	}
}
