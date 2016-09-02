using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Will display damage text particles for an Entity.  This behavior can either be on the Entity
	//  or a child of one for it to find the Entity and display damage text particles for it.
	//
	public class DamageTextParticle : MonoBehaviour 
	{
		public TextParticle textParticlePrefab;								// The TextParticle prefab to use to display damage text and critical messages.
		public float maxDistanceFromCamera = 50;							// The max distance from the reference camera that damage text is still visible.
		public Camera referenceCamera;										// The reference camera to display damage text to. If none specified, main camera will be used.

		public float normalMaxSpreadAngle = 25;								// Max spread angle to apply to impact normal vector for particle velocity (adds random direction).
		public bool flipNormalVelocity = false;								// Whether to flip impact normal direction for velocity calculations.

		public float particleGravity = 4;									// Downward gravity to use for particle (negative values fall upwards).
		public AnimationCurve particleScaleOverLifetime;					// AnimationCurve representing scale over lifetime for particle.
		public AnimationCurve particleTransparencyOverLifetime;				// AnimationCurve representing transparency over lifetime for partic.
		public Color standardDamageColor = Color.white;						// Standard damage color.
		public Color criticalDamageColor = Color.yellow;					// Critical damage color.

		public float upVelocityMin = 2;										// Upwards velocity minimum to add to particle velocity.
		public float upVelocityMax = 3;										// Upwards velocity maximum to add to particle velocity.
		public float normalVelocityMin = .5f;								// Velocity minimum of particle along normal (normal of impact point).
		public float normalVelocityMax = 2;									// Velocity maximum of particle along normal.

		public bool showCriticalMessage = true;								// Whether to show a critical damage message when damage is a crit.
		public float criticalMessageGravity = .5f;							// Gravity applied to critical message.
		public Color criticalMessageColor = Color.red;						// Critical message color.
		public string criticalMessage = "Critical!";						// Critical message to display.

		public AnimationCurve criticalScaleOverLifetime;					// AnimationCurve representing scale over lifetime for critical message.
		public AnimationCurve criticalTransparencyOverLifetime;				// AnimationCurve representing transparency over lifetime for critical message.

		public float criticalVelocityMultiplier = .5f;						// Multiplier for particle velocity of critical message (to distinguish).
		
		private Entity entity;
		
		void Awake () 
		{			
			// Obtain needed references and warn if any not found.
			entity = gameObject.GetComponentInParent<Entity>();

			if(!entity) Debug.LogWarning("No Entity found in gameobject for DamageTextParticle on " + name);
			if(!referenceCamera) referenceCamera = Camera.main;
			if(!referenceCamera) Debug.LogWarning("No main camera found for DamageTextParticle on " + name);
			if(!textParticlePrefab) Debug.LogWarning("No textParticlePrefab specified for DamageTextParticle on " + name);
		}
		
		void OnEnable()
		{
			// Subscribe to Entity's damageTaken event
			if(entity) entity.damageTaken += OnDamageTaken;
		}
		
		void OnDisable()
		{
			// Unsubscribe to Entity's damageTaken event when disabled.
			if(entity) entity.damageTaken -= OnDamageTaken;
		}
		
		public void OnDamageTaken(Entity source, DamageInfo damageInfo)
		{
			Vector3 point = damageInfo.point.HasValue ? damageInfo.point.Value : transform.position;
			Vector3 normal = damageInfo.normal.HasValue ? damageInfo.normal.Value : transform.forward;
			
			if(referenceCamera)
			{
				if( (point - referenceCamera.transform.position).sqrMagnitude > maxDistanceFromCamera * maxDistanceFromCamera)
					return;
			}

			// Verify a TextParticle prefab is specified
			if(textParticlePrefab)
			{
				// Instantiate a TextParticle
				TextParticle clone = Instantiate(textParticlePrefab, point, Quaternion.LookRotation(-normal)) as TextParticle;
				
				// Set TextParticle's text, color, and gravity values
				clone.text = damageInfo.damage.ToString();
				clone.color = damageInfo.critical ? criticalDamageColor : standardDamageColor;
				clone.gravity = particleGravity;
				
				// Calculate and set startVelocity for TextParticle based on impact normal (-clone.transform.forward) and random values
				Vector3 velocity = clone.transform.forward.RandomSpread(normalMaxSpreadAngle) * Random.Range(normalVelocityMin,normalVelocityMax) * (flipNormalVelocity ? 1 : -1);
				velocity.y += Random.Range(upVelocityMin, upVelocityMax);
				clone.startVelocity = velocity;
				
				// If scale and transparency AnimationCurves specified, set them for the TextParticle
				if(particleScaleOverLifetime.keys.Length > 0) clone.scaleOverLifetime = particleScaleOverLifetime;
				if(particleTransparencyOverLifetime.keys.Length > 0) clone.transparencyOverLifetime = particleTransparencyOverLifetime;
				
				// Show a corresponding Critical Message if needed
				if(damageInfo.critical && showCriticalMessage)
				{
					// Clone a new TextParticle for critical message
					clone = Instantiate(textParticlePrefab, point, Quaternion.LookRotation(-normal)) as TextParticle;
					
					// Set critical message text, color and gravity values
					clone.text = criticalMessage;
					clone.color = criticalMessageColor;
					clone.gravity = criticalMessageGravity;
					
					// Calculate and set velocity as usual, then multiply by criticalVelocityMultiplier to accentuate this particle
					velocity = clone.transform.forward.RandomSpread(normalMaxSpreadAngle) * Random.Range(normalVelocityMin,normalVelocityMax) * (flipNormalVelocity ? 1 : -1);
					velocity.y += Random.Range(upVelocityMin, upVelocityMax);
					velocity *= criticalVelocityMultiplier;
					clone.startVelocity = velocity;
					
					// Set AnimationCurves if provided
					if(criticalScaleOverLifetime.keys.Length > 0) clone.scaleOverLifetime = criticalScaleOverLifetime;
					if(criticalTransparencyOverLifetime.keys.Length > 0) clone.transparencyOverLifetime = criticalTransparencyOverLifetime;
				}
			}
		}
	}
}
