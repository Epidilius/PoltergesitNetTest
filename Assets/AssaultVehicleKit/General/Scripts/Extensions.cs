using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Extensions class to contain any extension needed for the project.
	//
	public static class Extensions
	{
		// Vector3 extension applying a random spread to a vector.
		// Useful for random spreads on weapon aiming vectors to simulate inaccuracy.
		public static Vector3 RandomSpread(this Vector3 vector, float maxAngle)
		{
			return 	Quaternion.LookRotation(vector) * 
					Quaternion.AngleAxis(Random.Range(0f, 360), Vector3.forward) * 
					Quaternion.AngleAxis(Random.Range(0, maxAngle/2f), Vector3.right) * 
					Vector3.forward;
		}
	}
}
