using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  The Turret input structure provides the means to communicate
	//  with Turrets.  All members are nullable so that controllers
	//  can send only the information they need to.  The Turret processes
	//  only the information passed to it.  This could also facilitate multiple
	//  controllers providing input to the turret without interfering with 
	//  each other.  Passing the input in a structure like this also allows
	//  for easy extensibility in that future additions can be added to the
	//  structure without breaking existing implementations.
	//
	public struct TurretInput
	{
		public Vector3? aimPoint;
		public bool? primaryFire;
		public bool? secondaryFire;
	}
}

