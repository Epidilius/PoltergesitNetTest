using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace hebertsystems.AVK
{
	//  This behavior provides the text particle functionality in world space.
	//  Text particles have a start velocity, can be affected by gravity, have a
	//  lifetime and will set text scale and transparency over the life of the particle
	//  based on curves.
	//
	public class TextParticle : MonoBehaviour 
	{
		public float gravity = 9.80665f;								// The gravity affecting the particle.
		public Vector3 startVelocity = Vector3.up;						// The initial velocity of the particle.
		public float lifetime = 2;										// The life span of the particle.
		public AnimationCurve scaleOverLifetime;						// The scale curve to apply over the lifetime of the particle.
		public AnimationCurve transparencyOverLifetime;					// The transparency curve to apply over the lifetime of the particle.

		public string text {set {if(textUI) textUI.text = value;} }		// Text setter property.
		public Color color {set {if(textUI) textUI.color = value;} }	// Text color setter property.

		private float startTime;
		private float stopTime;
		[SerializeField]
		private Text textUI;											// The Text UI component to control.

		void Awake () 
		{
			// Validate the curves have keys, and if not, add linear curve keys.
			if(scaleOverLifetime.keys.Length == 0)
			{
				scaleOverLifetime.AddKey(0,1);
				scaleOverLifetime.AddKey(1,1);
			}

			if(transparencyOverLifetime.keys.Length == 0)
			{
				transparencyOverLifetime.AddKey(0,1);
				transparencyOverLifetime.AddKey(1,0);
			}

			// Obtain textUI component if not specified manually.
			if(!textUI) textUI = GetComponentInChildren<Text>();
			if(!textUI) Debug.LogWarning("No Text component found for TextParticle on " + name);
		}

		void Start()
		{
			// Set start and stop times to lerp between.
			startTime = Time.time;
			stopTime = Time.time + lifetime;
		}

		void Update () 
		{
			// If particle still active, update
			if(Time.time < stopTime)
			{
				// Update velocity and position.
				startVelocity += Vector3.down * gravity * Time.deltaTime;
				transform.position += startVelocity * Time.deltaTime;

				// Lerp scale and transparency between start and stop times.
				float t = Mathf.InverseLerp(startTime, stopTime, Time.time);
				transform.localScale = Vector3.one * scaleOverLifetime.Evaluate(t);
				if(textUI)
				{
					Color color = textUI.color;
					color.a = transparencyOverLifetime.Evaluate(t);
					textUI.color = color;
				}
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
}
