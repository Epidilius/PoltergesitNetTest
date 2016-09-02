using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hebertsystems.AVK
{
	//  Manages the selection of AimingControllers for the PlayerDriver.
	//  Will find all AimingControllers as children of the same GameObject
	//  and sends out events to switch between them based on player input (T key).
	//
	public class AimingControllerManager : MonoBehaviour 
	{
		private AimingController[] controllers = new AimingController[0];
		private int controllerIndex = 0;
		
		void Awake()
		{
			// Obtain references to all AimingControllers that are attached to this game object or children.
			controllers = GetComponentsInChildren<AimingController>();
		}
		
		void Start()
		{
			// Set the initial AimingController (the first in the list)
			if(controllers.Length > 0) Events.SetAimingController(controllers[controllerIndex]);
		}
		
		void Update () 
		{
			// Cycle AimingController if 'T' pressed.
			if(Input.GetKeyDown(KeyCode.T))
			{
				// Cycle index, looping around if necessary.  Set new AimingController if index changed.
				int newIndex = controllerIndex + 1;
				if(newIndex >= controllers.Length) newIndex = 0;
				if(newIndex != controllerIndex)
				{
					controllerIndex = newIndex;
					Events.SetAimingController(controllers[controllerIndex]);
				}
			}
		}
	}
}
