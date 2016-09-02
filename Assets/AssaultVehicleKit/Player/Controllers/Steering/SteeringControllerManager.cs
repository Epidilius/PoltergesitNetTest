using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hebertsystems.AVK
{
	//  Manages the selection of SteeringControllers for the PlayerDriver.
	//  Will find all SteeringControllers as children of the same GameObject
	//  and sends out events to switch between them based on player input (R key).
	//
	public class SteeringControllerManager : MonoBehaviour 
	{
		private SteeringController[] controllers = new SteeringController[0];
		private int controllerIndex = 0;
		
		void Awake()
		{
			// Obtain references to all SteeringControllers that are attached to this game object or children.
			controllers = GetComponentsInChildren<SteeringController>();
		}
		
		void Start()
		{
			// Set the initial SteeringController (the first in the list)
			if(controllers.Length > 0) Events.SetSteeringController(controllers[controllerIndex]);
		}
		
		void Update () 
		{
			// Cycle SteeringController if 'R' pressed.
			if(Input.GetKeyDown(KeyCode.R))
			{
				// Cycle index, looping around if necessary.  Set new SteeringController if index changed.
				int newIndex = controllerIndex + 1;
				if(newIndex >= controllers.Length) newIndex = 0;
				if(newIndex != controllerIndex)
				{
					controllerIndex = newIndex;
					Events.SetSteeringController(controllers[controllerIndex]);
				}
			}
		}
	}
}
