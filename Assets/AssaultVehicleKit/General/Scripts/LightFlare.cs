using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Behavior to briefly flash a light with a specified
	//  light duration and intensity curve.  Light should be a child of this game object.
	//  Useful for explosion prefabs or similar effects.
	//
	public class LightFlare : MonoBehaviour 
	{
		public Light lightSource;								// The Light to flare.  Can be found in children if not specified.
		public AnimationCurve durationIntensityCurve;			// Intensity curve for the Light during lifetime.
		public float intensityCurveMultiplier = 1;				// A multiplier to apply to the intensity curve.
		public float lightDuration = 1;							// Duration of the light.

		private float startTime;
		private float disableTime;

		IEnumerator Start () 
		{
			// If no keys on the intensity curve, add keys for a linear dropoff.
			if(durationIntensityCurve.keys.Length == 0)
			{
				durationIntensityCurve.AddKey(new Keyframe(0,1));
				durationIntensityCurve.AddKey(new Keyframe(1,0));
			}

			// If no manual Light specified, try to find one.
			if(!lightSource) lightSource = GetComponentInChildren<Light>();

			if(lightSource)
			{
				// Enable light and set initial intensity.
				lightSource.enabled = true;
				lightSource.intensity = durationIntensityCurve.Evaluate(0) * intensityCurveMultiplier;

				disableTime = Time.time + lightDuration;

				// While below the disableTime, alter the lights intensity based on curve.
				while(Time.time <= disableTime)
				{
					yield return null;

					float t = Mathf.InverseLerp(disableTime - lightDuration, disableTime, Time.time);
					lightSource.intensity = durationIntensityCurve.Evaluate(t) * intensityCurveMultiplier;
				}

				// We are done, disable the light.
				lightSource.enabled = false;
			}
			else Debug.LogWarning("No Light found for LightFlare on " + name);
		}
	}
}
