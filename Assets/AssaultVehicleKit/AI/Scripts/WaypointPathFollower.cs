using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Base class for WaypointPathFollower AI, with public WaypointPath reference.
	//
	public abstract class WaypointPathFollower : MonoBehaviour 
	{
		public WaypointPath path;
	}
}
