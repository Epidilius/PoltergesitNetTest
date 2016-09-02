using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Camera Steering Controller.
	//  Steers the vehicle in the direction the camera is facing,
	//  along with throttle and braking.
	//
	public class CameraSteering : SteeringController 
	{		
		private VehicleInput vehicleInput;
		
		public override void Initialize(ref ControlReferences references)
		{
		}
		
		public override VehicleInput UpdateSteering(ref ControlReferences references)
		{
			// Camera move direction based on the player driver camera rig forward vector.
			if(references.cameraRigTransform) vehicleInput.moveDirection = references.cameraRigTransform.forward;

			// Set vehicle inputs based on player input.
			vehicleInput.forwardThrottle = PlayerInput.vehicleVertical;
			vehicleInput.sideThrottle = PlayerInput.vehicleHorizontal;
			vehicleInput.handBrake = PlayerInput.handBrake;
			vehicleInput.speedBoost = PlayerInput.speedBoost;

			return vehicleInput;
		}
		
		public override void Exit(ref ControlReferences references)
		{
		}
	}
}
