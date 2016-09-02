using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Base class for all Player Controllers (Camera, Steering, Aiming)
	//
	public abstract class PlayerController : MonoBehaviour 
	{
		// Initialize the controller.
		// Called when the controller is about to be used and should initialize any state and/or variables needed.
		public abstract void Initialize(ref ControlReferences references);

		// Exit the controller.
		// Called when the controller is no longer used - perform any cleanup necessary.
		public abstract void Exit(ref ControlReferences references);
	}
}
