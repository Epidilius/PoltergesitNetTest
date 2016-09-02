using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Simple behavior to apply an exploding force to any child rigidbodies.
	//  Useful for destroyed vehicle prefabs.
	//
	public class ExplodingGameObject : MonoBehaviour 
	{
		public float explosionForce = 200;							// Force to apply for the explosion.
		public float explosionRadius = 20;							// Radius of the explosion.
		public float debrisLifetime = 20;							// Lifetime of this gameobject, and any child debris.
		public float explosionDelay = .2f;							// Delay to apply the explosion.

		IEnumerator Start () 
		{
			// Wait the delay.
			yield return new WaitForSeconds(explosionDelay);

			// Get the bounds of this gameobject.
			Bounds bounds = new Bounds(transform.position, Vector3.zero);
			MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
			foreach(MeshRenderer meshRenderer in meshRenderers)
			{
				bounds.Encapsulate(meshRenderer.bounds);
			}

			// Get all Rigidbody children and apply the explosion force to them.
			Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
			foreach(Rigidbody rb in rigidbodies)
			{
				// Randomize the position of the explosion within the bounds.
				rb.AddExplosionForce(explosionForce, transform.position + Random.insideUnitSphere * bounds.extents.magnitude, explosionRadius);
			}

			Destroy(gameObject, debrisLifetime);
		}
	}
}
