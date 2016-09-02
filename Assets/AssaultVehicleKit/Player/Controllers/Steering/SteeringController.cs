using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Base class for all Steering Controllers used by the PlayerDriver.
	//
	public abstract class SteeringController : PlayerController 
	{
		public abstract VehicleInput UpdateSteering(ref ControlReferences references);
	}
}
