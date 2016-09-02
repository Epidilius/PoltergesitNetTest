using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hebertsystems.AVK
{
	//  Manages the selection of CameraControllers for the PlayerDriver.
	//  Will find all CameraControllers as children of the same GameObject
	//  and sends out events to switch between them based on player input (C key).
	//
	public class CameraControllerManager : MonoBehaviour 
	{
		private CameraController[] controllers = new CameraController[0];
		private int controllerIndex = 0;

		void Awake()
		{
			// Obtain references to all CameraControllers that are attached to this game object or children.
			controllers = GetComponentsInChildren<CameraController>();
		}

		void Start()
		{
			// Set the initial CameraController (the first in the list)
			if(controllers.Length > 0) Events.SetCameraController(controllers[controllerIndex]);
		}


		void Update () 
		{
			// Cycle CameraController if 'C' pressed.
			if(Input.GetKeyDown(KeyCode.C))
			{
				// Cycle index, looping around if necessary.  Set new CameraController if index changed.
				int newIndex = controllerIndex + 1;
				if(newIndex >= controllers.Length) newIndex = 0;
				if(newIndex != controllerIndex)
				{
					controllerIndex = newIndex;
					Events.SetCameraController(controllers[controllerIndex]);
				}
			}
		}
	}
}
