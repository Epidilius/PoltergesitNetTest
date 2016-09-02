using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Will destroy a gameobject after all child ParticleSystems have completed.
	//
	public class DestroyAfterParticlesComplete : MonoBehaviour 
	{
		private ParticleSystem[] particles;

		void Awake () 
		{
			particles = GetComponentsInChildren<ParticleSystem>();
		}

		void Update () 
		{
			bool anyAlive = false;
			foreach(ParticleSystem particle in particles)
			{
				if(particle.IsAlive())
				{
					anyAlive = true;
					break;
				}
			}

			if(!anyAlive) Destroy(gameObject);
		}
	}
}
