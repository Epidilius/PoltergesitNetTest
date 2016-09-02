using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace hebertsystems.AVK
{
	//  Represents a simple waypoint path that can be queried
	//  for the waypoint positions at an index, as well
	//  as the closest waypoint to a position.  
	//
	//  The waypoints are obtained from any child GameObject's position
	//  and are sorted alphabetically by name.  All child gameobjects
	//  will be destroyed after their position has been acquired.
	//
	public class WaypointPath : MonoBehaviour 
	{
		public int count {get {return waypoints.Count;} }				// The number of waypoints.


		private List<Vector3> waypoints = new List<Vector3>();

		void Awake () 
		{
			// Get sorted (by name) positions of all children, considered to be waypoints.
			waypoints = transform.Cast<Transform>().OrderBy(t=>t.name).Select(t=>t.position).ToList();

			// Destroy all children now that we have their positions - clean up the scene a bit.
			transform.Cast<Transform>().Select(t=>t.gameObject).ToList().ForEach(o => Destroy(o));
		}

		// Get the waypoint at specified index.
		// This method takes care of overlap of the index
		// and just wraps the index around if needed.
		public Vector3 WaypointAtIndex(int index)
		{
			if(waypoints.Count == 0) return Vector3.zero;

			index = index % waypoints.Count;
			if(index < 0) index += waypoints.Count;

			return waypoints[index];
		}

		// Gets the index of the closest waypoint to a point.
		// Returns -1 if no waypoints exist.
		public int IndexClosestTo(Vector3 point)
		{
			if(waypoints.Count == 0) return -1;

			int index = 0;
			float minDistance = float.MaxValue;
			for(int i=0; i<waypoints.Count; i++)
			{
				float sqrDistance = (point-waypoints[i]).sqrMagnitude;
				if(sqrDistance < minDistance)
				{
					minDistance = sqrDistance;
					index = i;
				}
			}

			return index;
		}
	}
}
