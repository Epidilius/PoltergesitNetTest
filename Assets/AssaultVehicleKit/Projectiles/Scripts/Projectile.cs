using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace hebertsystems.AVK
{
	//  Represents projectiles fired from Weapons (which are attached to Turrets usually [primary/secondary])
	//
	public class Projectile : MonoBehaviour 
	{
		// Custom class used for specifying impact prefabs to be instantiated
		// upon the Projectile colliding with something.
		// When specifying valid object types for each impact prefab, the fastest
		// check is the Layer, then Tag, and finally MonoBehaviour type.
		// Also provides the ability to exclude MonoBehaviour types as well.
		[System.Serializable]
		public class ImpactPrefab
		{
			public LayerMask validLayers;							// The valid Layers of the collided object for this impact prefab.
			public string[] validTags;								// The valid Tags of the collided object for this impact prefab.
			public string[] validTypes;								// The valid MonoBehaviour types for the collided object for this prefab.
			public string[] excludeTypes;							// MonoBehaviour types to exclude for this impact prefab.
			public GameObject prefab;								// The prefab to instantiate if the above met.
			public bool attachToCollisionObject = false;			// Whether or not to parent the impact prefab to the collided object.
			public float prefabScale = 1;							// The scale of the prefab.
			public float prefabNormalOffset = .01f;					// The offset along the impact normal from the impact point to instantiate the prefab.
			public bool flipFaceDirection = false;					// Used to flip the facing direction of the prefab along the normal of impact.
			public float prefabRotationMin = 0;						// Minimum rotation to apply to the prefab around the normal vector.
			public float prefabRotationMax = 360;					// Maximum rotation to apply to the prefab ...
			public bool exclusive = false;							// If set, this impact prefab has to be the first instantiated in the list.  Does not prevent subsequent prefabs in the list that match criteria.
		}
		
		public DamageInfo info										// Set the damage info for the Projectile (the Entity source, etc.).
		{
			set
			{
				// Copy DamageInfo
				damageInfo = value;
				
				// If source specified, grab colliders of the source.
				if(damageInfo.source) 
				{
					sourceColliders = new List<Collider>(damageInfo.source.GetComponentsInChildren<Collider>());
				}
			}
		}
		
		public virtual Vector3 velocity								// The velocity of the projectile.
		{
			get {return mVelocity;} 
			set {mVelocity = value;} 
		}			

		public bool handleTrajectory = true;						// Whether or not this Projectile handles the trajectory.  If controlling trajectory externally, set this to false.
		public bool handleCollision = true;							// Whether or not this Projectile handles collision.  If controlling collision externally, set this to false.

		public ImpactPrefab[] impactPrefabs;						// The list of possible impact prefabs to instantiate upon collision.
		
		public int damageMin = 500;									// Minimum damage of the projectile to apply to an Entity.
		public int damageMax = 600;									// Maximum damage of the projectile ...
		public int criticalDamageMin = 1500;						// Critical damage minimum ...
		public int criticalDamageMax = 1600;						// Critical damage maximum ...
		public float criticalChance = .25f;							// The chance of the damage being critical.
		
		public float impactForce = 0;								// Force applied to rigidbodies upon impact.
		public float explosionForce = 0;							// Explosion force upon impact, if any.
		public float explosionRadius = 0;							// The radius of the explosion force, if any.  A setting of 0 indicates all rigidbodies feel the explosion.
		
		public bool impactImmediately = false;						// Whether or not to determine immediate impact without traveling.
		public float maxImmediateDistance = 1000;					// The maximum distance to do an immediate collision detection.
		public bool raycastPerFrame = true;							// Whether or not to cast a ray each frame to check for collisions.
		// Note:  The above seemed necessary for rigidbody collisions with Terrain, even
		//        with Continuous Dynamic collision detection.  On Terrain, fast moving
		//        projectiles would impact the Terrain, but the impact point was rarely accurate (under the ground).
		
		public bool unparentParticles = true;						// Whether or not to unparent any active ParticleSystems of this projectile (useful for rocket contrails and the like so they don't immediately disappear).
		
		public float postponeDestroyTime = 0;						// Time to delay the destruction of this Projectile.  This helps things like TrailRenderers follow the projectile all the way to impact point.
		
		protected Vector3 mVelocity = Vector3.zero;
		protected Vector3 previousPosition;
		protected List<Collider> childColliders;
		protected List<Collider> sourceColliders = new List<Collider>();
		
		protected DamageInfo damageInfo;
		
		protected Rigidbody mRigidbody;
		protected Collider mCollider;
		
		protected virtual void Awake()
		{
			// Save previous position to start Raycast per frame check.
			previousPosition = transform.position;
			
			// Obtain any colliders that are a part of the Projectile.
			childColliders = new List<Collider>(GetComponentsInChildren<Collider>());
			
			// Obtain reference to the Rigidbody.
			mRigidbody = GetComponentInChildren<Rigidbody>();
			
			// Obtain reference to the Collider.
			mCollider = GetComponentInChildren<Collider>();
		}
		
		protected virtual void Start()
		{
			// If this is an immediate impact projectile, perform the check.
			if(impactImmediately)
			{
				RaycastHit closestValidHit;
				// If we are handling collisions and detect an immediate collision, handle it as usual.
				if(handleCollision && DoRaycastCheck(transform.position, transform.forward, maxImmediateDistance, out closestValidHit))
				{
					HandleCollision(closestValidHit.collider, closestValidHit.point, closestValidHit.normal);
				}
				// Otherwise, destroy ourselves immediately.
				else
				{
					Destroy(gameObject);
				}
			}
			else
			{
				// Set rigidbody velocity if we have one and are handling trajectory.
				if(handleTrajectory && mRigidbody) mRigidbody.velocity = mVelocity;
			}
		}
		
		protected virtual void Update()
		{
			// Update velocity and position if handling trajectory.
			if(handleTrajectory && !mRigidbody)
			{
				transform.position += mVelocity * Time.deltaTime;
			}
			
			// If we are handling collisions and Raycasting per frame and have changed positions, do the Raycast check.
			if(handleCollision && raycastPerFrame && transform.position != previousPosition)
			{
				// Obtain last move vector.
				Vector3 moveVector = transform.position - previousPosition;
				
				RaycastHit closestValidHit;
				
				// Check for collisions along move vector
				if(DoRaycastCheck(previousPosition, moveVector, moveVector.magnitude, out closestValidHit))
				{
					HandleCollision(closestValidHit.collider, closestValidHit.point, closestValidHit.normal);
				}
			}
			
			// Save last known position.
			previousPosition = transform.position;
		}
		
		// Perform a raycast check for collisions and report back the closest RaycastHit if a collision detected.
		protected virtual bool DoRaycastCheck(Vector3 position, Vector3 vector, float distance, out RaycastHit hitResult)
		{
			hitResult = new RaycastHit();
			float closestDistance = float.MaxValue;
			bool closestFound = false;
			
			// Obtain all collisions along the move vector.
			RaycastHit[] hits = Physics.RaycastAll(position, vector, distance);
			foreach(RaycastHit hit in hits)
			{
				// Make sure the collider does not belong to the projectile itself or the Entity source
				if(!childColliders.Contains(hit.collider) && !sourceColliders.Contains(hit.collider))
				{
					// If this is the closest hit so far, save it.
					if(hit.distance < closestDistance)
					{
						closestDistance = hit.distance;
						hitResult = hit;
						closestFound = true;
					}
				}
			}
			
			return closestFound;
		}
		
		void OnCollisionEnter(Collision collision)
		{
			if(!handleCollision) return;

			// If the rigidbody reports a collision, handle the first one
			if(collision.contacts.Length > 0)
			{
				ContactPoint contact = collision.contacts[0];
				
				HandleCollision(contact.otherCollider, contact.point, contact.normal);
			}
		}
		
		// Utility function to check if a gameobject is a MonoBehaviour type,
		// either on the gameobject or a parent.
		protected bool ContainsBehaviour(GameObject go, string[] behaviours)
		{
			if(behaviours.Length == 0) return false;
			
			// Check if we can find the MonoBehaviour type in the object parent hierarchy, return true if so.
			foreach(string b in behaviours)
			{
				System.Type type = Assembly.GetExecutingAssembly().GetType(b);
				if(type != null && go.GetComponentInParent(type)) return true;
			}
			
			return false;
		}
		
		// Main method for handling collision and instantiating prefabs.
		protected virtual bool HandleCollision(Collider otherCollider, Vector3 point, Vector3 normal)
		{
			// If specified, unparent any active particles and allow them to continue
			if(unparentParticles)
			{
				ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
				
				foreach(ParticleSystem particle in particles)
				{
					if(particle.isPlaying)
					{
						// Stop the particle from continuing to add new particles.
						particle.Stop();
						
						particle.transform.parent = null;
						// Add the DestroyAfterParticlesComplete which will destroy the orphaned object when particles are done.
						particle.gameObject.AddComponent<DestroyAfterParticlesComplete>();
					}
				}
			}
			
			// Instantiate prefabs
			
			// Obtain layer and tag of collided object.
			int otherLayer = 1 << otherCollider.gameObject.layer;
			string otherTag = otherCollider.gameObject.tag;
			bool prefabAdded = false;
			// Iterate through each potential impact prefab and check if the collided object is valid for it to be instantiated.
			foreach(ImpactPrefab impactPrefab in impactPrefabs)
			{
				// If the collided objects meets the following criteria
				// Note:  The if statement is setup to short circuit with fastest checks up front and slower checks at the back.
				if(	impactPrefab.prefab && 														// If a prefab specified AND
				   !(prefabAdded & impactPrefab.exclusive) &&									// No prefab already instantiated while this one is exclusive AND
				   ((otherLayer & impactPrefab.validLayers) != 0 || 							// (  Valid layer  OR
				 System.Array.IndexOf(impactPrefab.validTags, otherTag) >= 0 || 				//    Valid tag OR
				 ContainsBehaviour(otherCollider.gameObject, impactPrefab.validTypes)) &&  	//    Valid MonoBehaviour type  )  AND
				   !ContainsBehaviour(otherCollider.gameObject, impactPrefab.excludeTypes))   	// Does not contain excluded MonoBehaviours
				{
					// Phew!  If we've passed the above, instantiate the prefab.
					
					// Calculate prefab rotation based on settings
					Quaternion prefabRotation = Quaternion.LookRotation(-normal*(impactPrefab.flipFaceDirection ? -1 : 1)) * Quaternion.Euler(0,0,Random.Range(impactPrefab.prefabRotationMin,impactPrefab.prefabRotationMax));
					GameObject clone = Instantiate(impactPrefab.prefab, point + normal * impactPrefab.prefabNormalOffset, prefabRotation) as GameObject;
					clone.transform.localScale = Vector3.one * impactPrefab.prefabScale;
					// Attach to collided object if specified
					if(impactPrefab.attachToCollisionObject) clone.transform.parent = otherCollider.transform;
					
					prefabAdded = true;
				}
			}
			
			// If impactForce specified and collision is with a rigidbody, add impact force.
			Rigidbody otherRigidbody = otherCollider.GetComponent<Rigidbody>();
			if(impactForce != 0 && otherRigidbody)
			{
				otherRigidbody.AddForceAtPosition(normal * -impactForce, point, ForceMode.Impulse);
			}
			
			// If we collided with an Entity, add damage to it.
			Entity entity = otherCollider.gameObject.GetComponentInParent<Entity>();
			if(entity)
			{
				// Add damage details of impact to damageInfo structure.
				damageInfo.critical = Random.Range(0f,1.0f) <= criticalChance;
				damageInfo.damage = damageInfo.critical ? Random.Range(criticalDamageMin, criticalDamageMax + 1) : Random.Range(damageMin, damageMax + 1);
				damageInfo.point = point;
				damageInfo.normal = normal;
				// Tell Entity to take damage.
				entity.TakeDamage(damageInfo);
			}
			
			// If explosionForce specified, add explosive force to any rigidbodies within explosionRadius
			if(explosionForce != 0)
			{
				// Save explosionRadius squared for comparison
				float explosionRadiusSquared = explosionRadius * explosionRadius;
				
				// Get all rigidbodies in scene and iterate through each to see if it is within explosionRadius
				Rigidbody[] allRigidBodies = GameObject.FindObjectsOfType<Rigidbody>();
				foreach(Rigidbody rb in allRigidBodies)
				{
					Vector3 vectorToRigidbody = rb.position - point;
					// If the rigidbody is within the explosionRadius, apply explosionForce
					if(explosionRadius == 0 || vectorToRigidbody.sqrMagnitude <= explosionRadiusSquared)
					{
						// Add explosion force.
						rb.AddExplosionForce(explosionForce, point, explosionRadius);
						
						// If the rigidbody is an entity and not the one of the direct hit, apply damage
						Entity rbEntity = rb.gameObject.GetComponentInParent<Entity>();
						if(rbEntity != null && rbEntity != entity)
						{
							// Calculate explosion damage based on proximity to explosion and radius.
							float explosionProximityRatio = Mathf.InverseLerp(explosionRadiusSquared, 0, vectorToRigidbody.sqrMagnitude);
							int explosiveDamage = (int)((float)Random.Range(damageMin, damageMax+1) * explosionProximityRatio);
							if(explosiveDamage > 0)
							{
								// Apply damage to Entity caught in explosion
								DamageInfo explosionDamageInfo = new DamageInfo(explosiveDamage, false, rb.position, vectorToRigidbody.normalized, damageInfo.source);
								rbEntity.TakeDamage(explosionDamageInfo);
							}
						}
					}
				}
			}
			
			// Stop this projectile
			mVelocity *= 0;
			
			if(mCollider) mCollider.isTrigger = true;
			if(mRigidbody)
			{
				mRigidbody.isKinematic = true;
				mRigidbody.position = point;
				Destroy(mRigidbody);
			}
			
			// Set position to that of the point of impact
			transform.position = point;
			// Stop raycasting per frame.
			raycastPerFrame = false;
			
			// Destroy this Projectile either immediately or with specified delay.
			if(postponeDestroyTime == 0) Destroy(gameObject);
			else Destroy(gameObject, postponeDestroyTime);
			
			return true;
		}
	}
}
