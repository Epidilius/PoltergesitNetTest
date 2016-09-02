using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Provides weapon firing behavior and will instantiate a
	//  Projectile prefab upon firing, giving it an initial velocity.
	//  The Weapon class also simulates kickback force as well as
	//  weapon muzzle heat and accuracy based on heat.
	//
	public class Weapon : MonoBehaviour 
	{
		// Weapon type
		public enum WeaponType
		{
			Primary,												// Primary weapon type (default).
			Secondary												// Secondary weapon type.
		}

		public event System.Action<GameObject> weaponFired;				// The event for when the weapon has been fired.

		public WeaponType type = WeaponType.Primary;					// The weapon type (primary, secondary).
		public GameObject projectile;									// The Projectile prefab to instantiate upon firing.
		public float muzzleVelocity = 200;								// The muzzle velocity of the projectile along +Z of the Weapon.
		public bool inheritParentVelocity = true;						// Whether to inherit the parent rigidbody velocity along +Z of the Weapon.
		public float maxLifeDuration = 50;								// The max life of the instantiated projectile.
		public float shotsPerSec = 5;									// The number of shots per second.
		public float maxAccuracySpreadAngle = 4;						// The max spread angle of the aiming vector to simulate inaccuracy.
		public AnimationCurve accuracyHeatCurve;						// The accuracy and muzzle heat curve - the more the weapon is fired, the more inaccurate it is.
		public float coolDownTime = 5;									// Time it takes the muzzle to fully cool down from max heat.
		public float maxHeat = 10;										// The max heat of the muzzle.
		public float heatAddedPerShot = 1;								// The heat added per shot.

		public float muzzleHeat		{get {return heat;} }				// The current muzzle heat.

		public float kickbackForceMin = .01f;							// The kickback force minimum when a shot is fired.
		public float kickbackForceMax = .1f;							// The kickback force maximum when a shot is fired.

		public bool controlFireEffects = true;							// Whether the Weapon should attempt to find and control any effects (Sounds, ParticleSystems, Light, etc.).

		private AudioSource fireSound;
		private ParticleSystem[] fireParticles = new ParticleSystem[0];
		private Light shotLight;

		private float nextFireTime = 0;

		private Rigidbody parentRigidBody;
		private Collider[] parentColliders;

		private float heat = 0;

		private Entity parentEntity;

		protected virtual void Awake () 
		{
			if(controlFireEffects)
			{
				// Obtain references assumed to be in the Weapon hierarchy.
				fireSound = GetComponent<AudioSource>();
				fireParticles = GetComponentsInChildren<ParticleSystem>();
				shotLight = GetComponentInChildren<Light>();
				if(shotLight) shotLight.enabled = false;
			}

			parentEntity = GetComponentInParent<Entity>();
			parentRigidBody = GetComponentInParent<Rigidbody>();

			// Get any colliders that are a part of the parent vehicle if present.
			Vehicle vehicle = GetComponentInParent<Vehicle>();
			if(vehicle)
			{
				parentColliders = vehicle.GetComponentsInChildren<Collider>();
			}
		}

		protected virtual void Update()
		{
			// Cool the muzzle down.
			heat = coolDownTime > 0 ? Mathf.MoveTowards(heat, 0, maxHeat * Time.deltaTime / coolDownTime) : 0;
		}

		protected IEnumerator DisableBehaviourAfterDelay(Behaviour behaviour, float delay)
		{
			yield return new WaitForSeconds(delay);
			behaviour.enabled = false;
		}

		// Primary method of firing the Weapon.  Will only fire if able based on firing rate.
		public virtual void Fire()
		{
			// If a projectile specified and we are ready to fire again, fire!
			if(projectile && Time.time >= nextFireTime)
			{
				// Reset next fire time.
				nextFireTime = Time.time + (shotsPerSec > 0 ? 1/shotsPerSec : 10);

				// If there is a light, turn it on briefly.
				if(shotLight)
				{
					shotLight.enabled = true;
					StartCoroutine(DisableBehaviourAfterDelay(shotLight, .1f));
				}

				// Obtain parent rigidbody velocity
				Vector3 parentVelocity = Vector3.zero;
				if(inheritParentVelocity && parentRigidBody) 
				{
					// Obtain relative velocity to the Weapon, and cancel out any X and Y component.
					parentVelocity = parentRigidBody.velocity;
					Vector3 relativeVelocity = transform.InverseTransformDirection(parentVelocity);
					relativeVelocity.x = 0;
					relativeVelocity.y = 0;
					// If the Z component is negative, cancel that out as well.
					if(relativeVelocity.z < 0)
					{
						relativeVelocity.z = 0;
					}

					// Transform relative velocity back into world space.
					parentVelocity = transform.TransformDirection(relativeVelocity);
				}

				// Alter the accuracy of the shot based on muzzle heat and the accuracyHeatCurve.
				float t = Mathf.InverseLerp(0, maxHeat, heat);
				float accuracyAngle = accuracyHeatCurve.Evaluate(t) * maxAccuracySpreadAngle;
				// Calculate the projectile velocity vector as a result of aiming accuracy and inherited parent velocity.
				Vector3 projectileVelocity = transform.forward.RandomSpread(accuracyAngle) * muzzleVelocity + parentVelocity;

				// Instantiate the Projectile and pass along the Entity source of the projectile.
				GameObject projectileClone = Instantiate(projectile, transform.position, Quaternion.LookRotation(projectileVelocity)) as GameObject;
				// If the projectile is an actual "Projectile", we can do extra stuff with it.
				Projectile projectilePrefab = projectileClone.GetComponent<Projectile>();

				// If a Projectile type, set damage info
				if(projectilePrefab) projectilePrefab.info = new DamageInfo(0, false, null, null, parentEntity);

				// Obtain all colliders of the Projectile and inform the Physics system to ignore
				// collisions between the Projectile and the parent Vehicle
				Collider[] projectileColliders = projectileClone.GetComponentsInChildren<Collider>();

				foreach(Collider projectileCollider in projectileColliders)
				{
					if(projectileCollider.enabled && !projectileCollider.isTrigger)
					{
						foreach(Collider parentCollider in parentColliders)
						{
							if(parentCollider.enabled && !parentCollider.isTrigger)
								Physics.IgnoreCollision(projectileCollider, parentCollider);
						}
					}
				}

				// Set the velocity of the Projectile and set it to destroy at maxLifeDuration (will most likely die before that).
				if(projectilePrefab) projectilePrefab.velocity = projectileVelocity;
				Destroy(projectileClone.gameObject, maxLifeDuration);

				// Play the fireSound if available.
				if(fireSound) 
				{
					if(fireSound.isPlaying) fireSound.Stop();
					fireSound.Play();
				}

				// Play fire particles.
				foreach(ParticleSystem particle in fireParticles)
				{
					particle.Play();
				}

				// If a kickback force specifed, apply it to parent rigidbody.
				if(parentRigidBody && kickbackForceMax != 0)
				{
					parentRigidBody.AddForceAtPosition(transform.forward * -Random.Range(kickbackForceMin, kickbackForceMax), transform.position, ForceMode.Impulse);
				}

				// Add heat to muzzle.
				heat = Mathf.MoveTowards(heat, maxHeat, heatAddedPerShot);

				// Send weaponFired event.
				if(weaponFired != null) weaponFired(projectileClone);
			}
		}
	}
}
