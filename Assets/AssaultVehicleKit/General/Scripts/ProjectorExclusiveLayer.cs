using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Behavior to set a Projector to ignore all other layers than the
	//  layer of the parent gameobject the Projector is attached to.
	//
	[RequireComponent(typeof(Projector))]
	public class ProjectorExclusiveLayer : MonoBehaviour
	{
		public LayerMask exclusiveLayers = 0;					// Exclusive Layers mask.

		void Start () 
		{
			// If parented to an object with a layer selected in exclusiveLayers, set projector to ignore all other layers
			if(transform.parent != null && (1 << transform.parent.gameObject.layer & exclusiveLayers) != 0)
			{
				Projector projector = GetComponent<Projector>();

				if(projector) projector.ignoreLayers = ~(1 << transform.parent.gameObject.layer);
			}
		
		}
	}
}
