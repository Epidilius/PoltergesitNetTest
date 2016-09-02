using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Simple behavior to oscillate UV coordinates on a material.
	//  Used for the RollerBot eye scan.
	//
	public class OscillateUV : MonoBehaviour 
	{
		public float uOffset = .1f;								// Max U offset (from -uOffset to +uOffset).
		public float vOffset = .1f;								// Max V offset (from -vOffset to +vOffset).
		public float oscillateTime = 1;							// The oscillation time.
		public string textureName = "_MainTex";					// Name of texture to modify UV offset for.

		private Material material;
		private Texture2D texture;

		void Awake () 
		{
			// Obtain references to the Material and Texture2D
			MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();

			if(meshRenderer) 
			{
				material = meshRenderer.material;
				if(material) texture = material.GetTexture(textureName) as Texture2D;
			}
		
		}

		void Update () 
		{
			if(material && texture)
			{
				// Oscillate the UV offset of the texture on the material.
				material.SetTextureOffset(textureName, 
				                          new Vector2( Mathf.Sin(Time.time/oscillateTime)*uOffset, Mathf.Sin(Time.time/oscillateTime)*vOffset));
			}
		
		}
	}
}
