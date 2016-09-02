using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Base class for all Aiming Controllers used by the PlayerDriver.
	//
	public abstract class AimingController : PlayerController 
	{
		public abstract TurretInput UpdateAiming(ref ControlReferences references);
	}
}
