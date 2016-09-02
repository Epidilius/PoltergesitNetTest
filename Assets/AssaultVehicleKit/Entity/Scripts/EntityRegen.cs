using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Will regenerate health or shield for the Entity after a damage reset time
	//  and at a specified tick rate.
	//
	[RequireComponent(typeof(Entity))]
	public class EntityRegen : MonoBehaviour 
	{
		public enum RegenType
		{
			Health,												// Regenerate health.
			Shield												// Regenerate shield.
		}

		public RegenType type = RegenType.Health;				// The type of regen (health, shield)

		public float damageResetTime = 5;						// The time it takes to start regeneration after damage has been taken.
		public float regenTickTime = .1f;						// The amount of time between regeneration ticks.
		public int regenPerTick = 10;							// The amount to regenerate each tick.

		private Entity entity;
		private bool regenEnabled = true;
		private float regenEnableTime = 0;

		IEnumerator Start ()
		{
			// Get Entity reference to regenerate.
			entity = GetComponent<Entity>();

			if(!entity)
			{
				Debug.LogWarning("No Entity for HealthRegen on " + name);
			}
			else
			{
				// Subscribe to Entity's damageTaken event so as to pause regeneration when damaged.
				entity.damageTaken += OnEntityDamaged;

				while(true)
				{
					// Wait for regenTickTime
					yield return new WaitForSeconds(regenTickTime);

					// If we are past the regenEnableTime, enable regeneration.
					if(Time.time >= regenEnableTime) regenEnabled = true;
					// If regeneration enabled, regen health or shield based on type.
					if(regenEnabled) 
					{
						if(type.Equals(RegenType.Health)) entity.health += regenPerTick;
						if(type.Equals(RegenType.Shield)) entity.shield += regenPerTick;
					}
				}
			}
		}
		
		void OnEntityDamaged(Entity source, DamageInfo info)
		{
			// Reset regen enabled timer and disable regeneration for now.
			regenEnableTime = Time.time + damageResetTime;
			regenEnabled = false;
		}
	}
}
