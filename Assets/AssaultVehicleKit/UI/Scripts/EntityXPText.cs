using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace hebertsystems.AVK
{
	//  Behavior for displaying an experience gained effect from destroying Entities.
	//
	[RequireComponent(typeof(Text))]
	public class EntityXPText : MonoBehaviour 
	{
		public float delayBeforeFade = .8f;							// The delay before fading the XP gained text.
		public float fadeTime = 1;									// The time to fade the XP gained text.
		public Vector2 fadeAcceleration = Vector2.zero;				// The fade acceleration to apply to the text (float up, etc.).

		public Vector2 initialPosition;								// The initial position of the Text within the containing Canvas (usually set by another script).

		public Text text;											// The Text component of this object, publicly exposed for external changes.

		private float fadeoutTime;
		private Vector2 velocity;

		void Awake()
		{
			// Get the Text reference.
			text = GetComponent<Text>();
		}

		IEnumerator Start () 
		{
			if(text)
			{
				// Set the Text's initial position.
				text.rectTransform.anchoredPosition = initialPosition;

				// Wait the specified delay.
				yield return new WaitForSeconds(delayBeforeFade);

				// Determine the time when we are done fading out.
				fadeoutTime = Time.time + fadeTime;

				// While we are still fading, move the text and change transparency.
				while(Time.time < fadeoutTime)
				{
					// Move the Text.
					velocity += fadeAcceleration * Time.deltaTime;
					text.rectTransform.anchoredPosition += velocity * Time.deltaTime;

					// Calculate the transparency (1 - t)
					float t = Mathf.InverseLerp(fadeoutTime - fadeTime, fadeoutTime, Time.time);

					// Set the Text's transparency.
					Color color = text.color;
					color.a = 1 - t;
					text.color = color;

					yield return null;
				}
			}

			Destroy(gameObject);
		}
	}
}
