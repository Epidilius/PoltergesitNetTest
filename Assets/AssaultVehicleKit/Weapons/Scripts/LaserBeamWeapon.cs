using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  The LaserBeamWeapon is an augmented Weapon to display
	//  a continuous laser beam as it fires.
	//
	public class LaserBeamWeapon : Weapon 
	{
		private LaserBeam laserBeam;

		private bool firing = false;

		protected override void Awake () 
		{
			base.Awake();

			laserBeam = GetComponentInChildren<LaserBeam>();
			if(laserBeam) laserBeam.gameObject.SetActive(false);
		}

		public override void Fire()
		{
			// Display laser beam
			firing = true;

			// Fire weapon
			base.Fire();
		}

		void LateUpdate()
		{
			if(!firing && laserBeam) laserBeam.gameObject.SetActive(false);
			else laserBeam.gameObject.SetActive(true);

			// Default firing is off, but this will get set again next frame in Fire() if still firing.
			firing = false;
		}
	}
}
