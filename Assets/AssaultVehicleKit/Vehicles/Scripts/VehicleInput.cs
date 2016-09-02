using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  The VehicleInput structure provides the means to communicate
	//  with Vehicles.  All members are nullable so that controllers
	//  can send only the information they need to.  The Vehicle processes
	//  only the information passed to it.  This could also facilitate multiple
	//  controllers providing input to the vehicle without interfering with 
	//  each other.  Passing the input in a structure like this also allows
	//  for easy extensibility in that future additions can be added to the
	//  structure without breaking existing implementations.
	//
	public struct VehicleInput
	{
		public float? forwardThrottle;				// Accellerate the vehicle forward/backward (-1 to 1, with positive values forward).
		public float? sideThrottle;					// Accellerate the vehicle left/right (-1 to 1, with positive values right).
		public float? brake;						// Standard Vehicle brakes (0 to 1).
		public bool? handBrake;						// Hand brake (on or off).
		public bool? speedBoost;					// Speed boost (on or off).
		public Vector3? moveDirection;				// The move direction for the vehicle in world space.
		public Vector3? relativeMoveDirection;		// The move direction for the vehicle in local space.
	}
}
