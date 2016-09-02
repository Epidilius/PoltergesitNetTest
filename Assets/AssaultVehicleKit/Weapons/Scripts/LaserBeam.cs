using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hebertsystems.AVK
{
	//  Provides a LaserBeam effect with a LineRenderer.
	//
	[RequireComponent(typeof(LineRenderer))]
	public class LaserBeam : MonoBehaviour 
	{
		public Transform laserImpactParticles;					// Impact particle Transform reference.  Moves the particles to point of impact.

		public float maxLaserDistance = 1000;					// The max distance to cast the laser.
		public float uTilingPerMeter = 80;						// The texture tiling along the beam per meter.	
		public float uOffsetRate = -5;							// The U offset rate (scrolling beam effect).
		public string textureName = "_MainTex";					// Name of texture to modify UV offset for.

		private LineRenderer lineRenderer;
		private Material[] materials = new Material[0];

		protected List<Collider> sourceColliders = new List<Collider>();
		protected AudioSource[] audioSources;
		
		void Awake () 
		{
			// Obtain reference to LineRenderer.
			lineRenderer = GetComponent<LineRenderer>();
			if(!lineRenderer) Debug.LogWarning("Unable to find LineRenderer for LaserBeam on " + name);

			if(lineRenderer)
			{
				materials = lineRenderer.materials;
			}

			// Obtain references to any AudioSources - will play when OnEnable and stop when OnDisable.
			audioSources = GetComponentsInChildren<AudioSource>();

			// Get colliders from our entity source, if there is one, so we can avoid them.
			Entity source = GetComponentInParent<Entity>();
			if(source)
			{
				sourceColliders = new List<Collider>(source.GetComponentsInChildren<Collider>());
			}
		}

		void OnEnable()
		{
			foreach(AudioSource audioSource in audioSources) 
			{
				audioSource.Play();
			}
		}

		void OnDisable()
		{
			foreach(AudioSource audioSource in audioSources) 
			{
				audioSource.Stop();
			}
		}
		
		void Update () 
		{
			if(lineRenderer)
			{
				// Obtain distance to first hit along beam.

				// Initialize distance assuming no hit.
				float distance = maxLaserDistance / transform.lossyScale.z;

				RaycastHit closestValidHit = new RaycastHit();
				float closestDistance = float.MaxValue;
				bool closestFound = false;
				
				// Obtain all collisions along the beam.
				RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, maxLaserDistance);
				foreach(RaycastHit hit in hits)
				{
					// Make sure the collider does not belong to the Entity source
					if(!sourceColliders.Contains(hit.collider))
					{
						// If this is the closest hit so far, save it.
						if(hit.distance < closestDistance)
						{
							closestDistance = hit.distance;
							closestValidHit = hit;
							closestFound = true;
						}
					}
				}
				
				// If a closest collision was dected, update distance.
				if(closestFound)
				{
					distance = closestValidHit.distance / transform.lossyScale.z;
				}

				// Calculate the texture tiling along the beam and set beam end position.
				float tiling = distance * uTilingPerMeter;
				lineRenderer.SetPosition(1, new Vector3(0,0,distance));

				// Move laser beam impact particles to impact point.
				if(laserImpactParticles)
				{
					laserImpactParticles.transform.localPosition = Vector3.forward * distance;
					laserImpactParticles.gameObject.SetActive(closestFound);
				}

				// Do texture scrolling effect along beam and keep texture tiling sized correctly.
				if(materials.Length > 0)
				{
					foreach(Material material in materials)
					{
						material.SetTextureScale(textureName, new Vector2( tiling, 1));
						material.SetTextureOffset(textureName, new Vector2(Time.time * uOffsetRate, 0));
					}
				}
			}
		}
	}
}
