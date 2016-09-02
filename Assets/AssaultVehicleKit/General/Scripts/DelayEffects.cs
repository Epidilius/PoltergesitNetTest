using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Will play specified AudioSources and ParicleSystems after a delay.
	//
	public class DelayEffects : MonoBehaviour 
	{
		public AudioSource[] audioSources;
		public ParticleSystem[] particles;
		public float delayInSeconds;

		IEnumerator Start() 
		{
			yield return new WaitForSeconds(delayInSeconds);

			foreach(AudioSource audioSource in audioSources)
			{
				audioSource.Play();
			}

			foreach(ParticleSystem particle in particles)
			{
				particle.Play();
			}
		}
	}
}
