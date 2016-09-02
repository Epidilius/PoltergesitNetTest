using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Will destroy a gameobject after a specified delay.
	//
	public class DestroyAfterDelay : MonoBehaviour 
	{
		public float delaySeconds = 60;

		void Start()
		{
			Destroy(gameObject, delaySeconds);
		}
	}
}
