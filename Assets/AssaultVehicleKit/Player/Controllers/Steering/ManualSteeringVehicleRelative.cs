using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Manual Steering Controller.
	//  Steers the vehicle using player manual steering input (A - D),
	//  along with throttle and braking.
	//  Steering input is provided relative to the vehicle.
	//
	public class ManualSteeringVehicleRelative : SteeringController 
	{		
		private VehicleInput vehicleInput;
		
		public override void Initialize(ref ControlReferences references)
		{
		}
		
		public override VehicleInput UpdateSteering(ref ControlReferences references)
		{
			// Set vehicle's relative move direction based on player left/right input.
			vehicleInput.relativeMoveDirection = Quaternion.AngleAxis(PlayerInput.vehicleHorizontal * 90, Vector3.up) * Vector3.forward;

			// Set vehicle inputs based on player input.
			vehicleInput.forwardThrottle = PlayerInput.vehicleVertical;
			vehicleInput.handBrake = PlayerInput.handBrake;
			vehicleInput.speedBoost = PlayerInput.speedBoost;
			
			return vehicleInput;
		}
		
		public override void Exit(ref ControlReferences references)
		{
		}
	}
}
