using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Will destroy a projector gameobject after a specified delay and fade time.
	//  The Projector effect will fade out over the fade time before being destroyed.
	//  Ya never wan't too many projectors in a scene...
	//
	[RequireComponent (typeof(Projector))]
	public class FadeProjectorAndDestroy : MonoBehaviour 
	{
		public float delayBeforeFade = 10;												// The delay before starting to fade the Projector.
		public float fadeTime = 2;														// The time span to fade the Projector over after the initial delay.
		
		private Projector projector;
		private bool fadeFalloffTexture = false;
		private Material projectorMaterial;
		private Texture2D falloffTexture;
		private Color[] originalPixels;
		
		IEnumerator Start () 
		{
			// Obtain reference to Projector.
			projector = GetComponentInChildren<Projector>();
			
			// Wait the initial delay.
			yield return new WaitForSeconds(delayBeforeFade);
			
			// If we have a projector and the fadeTime is greater than 0, do the fade.
			if(projector && fadeTime > 0)
			{
				// Create a unique Material for this Projector so we can fade it's texture individually (won't affect Material in Asset database).
				projectorMaterial = new Material(projector.material);
				projector.material = projectorMaterial;
				// Get the texture for the material.
				falloffTexture = projectorMaterial.GetTexture("_FalloffTex") as Texture2D;
				if(falloffTexture) 
				{
					// Save the original pixels of the texture - will be used to fade over time.
					originalPixels = falloffTexture.GetPixels();
					// Create a unique copy of the falloff texture to fade so it is unique to this object (won't be shared between other Projectors).
					falloffTexture = new Texture2D(falloffTexture.width, falloffTexture.height, falloffTexture.format, false);
					falloffTexture.wrapMode = TextureWrapMode.Clamp;
					falloffTexture.SetPixels(originalPixels);
					falloffTexture.Apply();
					
					// Set flag indicating we have a unique falloff texture for the Projector and are good to go.
					fadeFalloffTexture = true;
				}

				// If we have a unique copy of the falloff texture to fade, proceed to fade.
				if(fadeFalloffTexture)
				{
					float fadeOutTime = Time.time + fadeTime;
					
					while(Time.time <= fadeOutTime)
					{
						// Wait for next frame.
						yield return null;
						
						// Calculate the amount of fade.
						float t = 1 - Mathf.InverseLerp(fadeOutTime - fadeTime, fadeOutTime, Time.time);
						
						// Make a copy of the original pixels and iterate over them, multiplying by fade factor.
						Color[] pixels = originalPixels.Clone() as Color[];
						for(int i=0; i<pixels.Length; i++)
						{
							pixels[i] = originalPixels[i] * t;
						}
						
						// Set modified pixels to texture and apply to projectors material.
						falloffTexture.SetPixels(pixels);
						falloffTexture.Apply();
						projector.material.SetTexture("_FalloffTex", falloffTexture);
					}
				}
			}
			
			Destroy(gameObject);
		}
	}
}
