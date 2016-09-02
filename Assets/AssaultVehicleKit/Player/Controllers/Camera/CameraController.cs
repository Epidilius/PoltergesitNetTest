using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Base class for all Camera Controllers used by the PlayerDriver.
	//
	public abstract class CameraController : PlayerController 
	{
		public abstract CameraInput UpdateCamera(ref ControlReferences references);
	}
}
