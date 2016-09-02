using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Simple behavior that alters the start color of a smoke ParticleSystem based
	//  on the muzzle heat of a Weapon.  This should fade the transparency of the
	//  muzzle smoke particles in and out based on muzzle heat.
	//
	[RequireComponent(typeof(Weapon))]
	public class MuzzleHeatAndSmoke : MonoBehaviour 
	{
		public ParticleSystem muzzleSmoke;		// The muzzle smoke ParticleSystem that will be altered as a result of muzzle heat.

		private Weapon weapon;

		void Awake () 
		{
			// Obtain reference to Weapon
			weapon = GetComponent<Weapon>();

			if(!weapon) Debug.LogWarning("No Weapon found for MuzzleHeatAndSmoke on " + name);
			if(!muzzleSmoke) Debug.LogWarning("No Muzzle Smoke specified for MuzzleHeatAndSmoke on " + name);
		}

		void Update () 
		{
			if(muzzleSmoke && weapon)
			{
				// Alter muzzle smoke ParticleSystem startColor based on ratio of muzzle heat to max heat.
				float t = Mathf.InverseLerp(0, weapon.maxHeat, weapon.muzzleHeat);
				muzzleSmoke.startColor = new Color(1,1,1,t);
			}
		}
	}
}
