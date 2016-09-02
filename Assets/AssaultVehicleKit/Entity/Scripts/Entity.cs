using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace hebertsystems.AVK
{
	//  Entities are objects that have health, experience, etc. and can be harmed and die.
	//
	public class Entity : MonoBehaviour 
	{
		public event Action<Entity> statusChanged;							// Event for when the Entity's status (health, shield, xp, etc.) has changed.
		public event Action<Entity, DamageInfo> damageTaken;				// Event for when the Entity takes damage.
		public event Action<Entity, Entity, int> experienceGained;			// Event for when the Entity receives experience.
		public event Action<Entity, Dictionary<Entity, int>> death;			// Event for when the Entity has died.
		public event Action<Entity> destroyed;								// Event when the Entity gameobject is being destroyed.

		public GameObject deathPrefab;										// The prefab to instantiate upon the Entity's death.
		public int xpGivenOnDeath = 50;										// The experience gained upon killing this Entity, given to the Entity which has done the most damage.

		public int experience	{get {return mExperience;} }				// The experience this Entity has.

		public int maxHealth = 20000;										// The maximum health for this Entity.
		public int health													// The current health of the Entity
		{
			get {return mHealth;} 
			set 
			{
				int newHealth = Mathf.Clamp(value, 0, maxHealth);
				if(newHealth != mHealth)
				{
					// Set new health.
					mHealth = newHealth;
					// Send statusChanged event.
					StatusChanged();
				}
			} 
		}

		public int maxShield = 5000;										// The maximum shield for this Entity.
		public int shield													// The current shield of the Entity.
		{
			get {return mShield;} 
			set 
			{
				int newShield = Mathf.Clamp(value, 0, maxShield);
				if(newShield != mShield)
				{
					// Set new shield.
					mShield = newShield;
					// Send statusChanged event.
					StatusChanged();
				}
			} 
		}

		protected Rigidbody mRigidbody;

		private int mExperience;
		private int mHealth;
		private int mShield;
		private Dictionary<Entity, int> damageSources = new Dictionary<Entity, int>();


		protected virtual void Awake () 
		{
			// Initialize status variables.
			mExperience = 0;
			mHealth = maxHealth;
			mShield = maxShield;

			// Obtain reference to the Rigidbody.
			mRigidbody = GetComponentInChildren<Rigidbody>();
		}

		protected virtual void Start()
		{
			// Send statusChanged event to initialize any status indicators.
			StatusChanged();
		}

		protected virtual void StatusChanged()
		{
			// Send status changed event.
			if(statusChanged != null) statusChanged(this);
		}
		
		public virtual void LateUpdate()
		{
			// Check for death.		
			if(mHealth <= 0)
			{
				// Unsubscribe to any damageSources death events that we would've subscribed to.
				foreach(var pair in damageSources)
				{
					pair.Key.death -= OnOtherEntityDeath;
				}

				// Award XP to Entity who is still alive and did the most damage.
				if(damageSources.Count > 0)
				{
					List<Entity> sortedEntities = damageSources.OrderByDescending(p=>p.Value).Select(p=>p.Key).ToList();
					if(sortedEntities.Count > 0) sortedEntities[0].GainExperience(this, xpGivenOnDeath);
				}

				// Send death event
				if(death != null) death(this, damageSources);
				
				// Instantiate our deathPrefab if specified.
				if(deathPrefab) 
				{
					GameObject clone = Instantiate(deathPrefab, transform.position, transform.rotation) as GameObject;
					// Copy our rigidbody velocity to deathPrefab's rigidbodies.
					if(mRigidbody)
					{
						Rigidbody[] cloneRBs = clone.GetComponentsInChildren<Rigidbody>();
						foreach(Rigidbody rb in cloneRBs)
						{
							if(!rb.isKinematic) rb.velocity = mRigidbody.velocity;
						}
					}
				}

				// Goodbye cruel world!
				Destroy(gameObject);
			}
		}
		
		public virtual void TakeDamage(DamageInfo damageInfo)
		{
			// Take the damage to the shield first, if any, then to health.
			int damage = damageInfo.damage;
			if(mShield > 0)
			{
				if(damage <= mShield)
				{
					shield -= damage;
					damage = 0;
				}
				else
				{
					damage -= mShield;
					shield = 0;
				}
			}
			health -= damage;

			// Add damage amount to the Entity source, if specified, in our damageSources dictionary.
			if(damageInfo.source != null)
			{
				if(!damageSources.ContainsKey(damageInfo.source)) damageSources.Add(damageInfo.source, 0);
				damageSources[damageInfo.source] += damageInfo.damage;

				// Subscribe to death event of this source so we can remove them from damageSources dictionary on their death.
				damageInfo.source.death += OnOtherEntityDeath;
			}

			// Send damage taken event.
			if(damageTaken != null) damageTaken(this, damageInfo);

			// Send status changed event.
			StatusChanged();
		}

		public virtual void GainExperience(Entity provider, int amount)
		{
			// Add experience amount.
			mExperience += amount;

			// Send experienceGained event.
			if(experienceGained != null) experienceGained(this, provider, amount);

			// Send statusChanged event.
			StatusChanged();
		}

		protected virtual void OnOtherEntityDeath(Entity source, Dictionary<Entity, int> culprits)
		{
			// Remove Entity that just died from our damage source dictionary.
			damageSources.Remove(source);
		}

		protected virtual void OnDestroy()
		{
			// Send out destroyed event.
			if(destroyed != null) destroyed(this);
		}
	}
}
