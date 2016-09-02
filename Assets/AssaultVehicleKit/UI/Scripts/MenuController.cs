using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace hebertsystems.AVK
{
	//  Main Help and Settings menu control.
	//
	[RequireComponent(typeof(PlayerInput))]
	public class MenuController : MonoBehaviour 
	{
		public GameObject mainMenuUI;							// Reference to the main menu UI to show/hide.
		public Slider mouseSensitivity;							// Reference to mouse sensitivity slider.
		public Toggle invertMouseY;								// Reference to invert mouse Y.
		public Slider joystickSensitivity;						// Reference to joystick sensitivity slider.
		public Toggle invertJoystickY;							// Reference to invert joystick Y.
		public Button exitApplicationButton;					// Reference to the Exit Application button - will be invisible on certain builds.

		public float mouseSensitivityMin = .5f;					// Min and Max mouse sensitivities
		public float mouseSensitivityMax = 20;
		public float joystickSensitivityMin = .5f;				// Min and Max joystick sensitivities.
		public float joystickSensitivityMax = 10;

		private PlayerInput playerInput;
		private bool showMenu = false;

		void Awake () 
		{
			// Obtain PlayerInput reference.
			playerInput = GetComponent<PlayerInput>();

			// Disable Exit Application for Editor, web player and mobile.
			if(exitApplicationButton && (Application.isEditor || Application.isWebPlayer || Application.isMobilePlatform))
			{
				exitApplicationButton.gameObject.SetActive(false);
			}
		}

		void Start () 
		{
			// Initialize menu to match PlayerInput.
			if(playerInput) 
			{
				if(mouseSensitivity) mouseSensitivity.value = Mathf.InverseLerp(mouseSensitivityMin, mouseSensitivityMax, playerInput.mouseXSensitivity);
				if(invertMouseY) invertMouseY.isOn = playerInput.invertMouseY;
				if(joystickSensitivity) joystickSensitivity.value = Mathf.InverseLerp(joystickSensitivityMin, joystickSensitivityMax, playerInput.axisXSensitivity);
				if(invertJoystickY) invertJoystickY.isOn = playerInput.invertAxisY;
			}

			// Disable menu initially.
			if(mainMenuUI) mainMenuUI.SetActive(false);
		}


		void Update () 
		{
			// Toggle main menu on/off when H is pressed
			if(Input.GetKeyDown(KeyCode.H))
			{
				showMenu = !showMenu;

				if(mainMenuUI) mainMenuUI.SetActive(showMenu);

				// Pause game if showing the menu.
				if(showMenu)
				{
					Time.timeScale = 0;
					if(playerInput) playerInput.pauseGame = true;

					Utilities.LockCursor(false);
				}
				// Unpause game.
				else
				{
					Time.timeScale = 1;
					if(playerInput) playerInput.pauseGame = false;
					Utilities.LockCursor(true);
				}
			}
		}

		// Menu callback methods:

		public void OnMouseSensitivityChanged(float value)
		{
			if(playerInput) 
			{
				playerInput.mouseXSensitivity = Mathf.Lerp(mouseSensitivityMin, mouseSensitivityMax, value);
				playerInput.mouseYSensitivity = Mathf.Lerp(mouseSensitivityMin, mouseSensitivityMax, value);
			}
		}

		public void OnInvertMouseY(bool value)
		{
			if(playerInput) playerInput.invertMouseY = value;
		}

		public void OnJoystickSensitivityChanged(float value)
		{
			if(playerInput) 
			{
				playerInput.axisXSensitivity = Mathf.Lerp(joystickSensitivityMin, joystickSensitivityMax, value);
				playerInput.axisYSensitivity = Mathf.Lerp(joystickSensitivityMin, joystickSensitivityMax, value);
			}
		}

		public void OnInvertJoystickY(bool value)
		{
			if(playerInput) playerInput.invertAxisY = value;
		}

		public void OnExitMenu()
		{
			showMenu = false;
			if(mainMenuUI) mainMenuUI.SetActive(false);
			Time.timeScale = 1;
			if(playerInput) playerInput.pauseGame = false;
			Utilities.LockCursor(true);
		}

		public void OnExitApplication()
		{
			Application.Quit();
		}
	}
}
