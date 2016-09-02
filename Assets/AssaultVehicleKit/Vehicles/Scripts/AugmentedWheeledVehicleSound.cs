using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Provides sound response for augmented wheeled vehicles
	//  on top of the standard wheeled vehicle sounds.
	//
	[RequireComponent(typeof(AugmentedWheeledVehicle))]
	public class AugmentedWheeledVehicleSound : WheeledVehicleSound 
	{
		// Speed boost thruster sound settings.
		public AudioSource speedThrusterSound;

		public float thrusterMinPitch = .35f;
		public float thrusterMaxPitch = 1.25f;
		public float thrusterMinVolume = .1f;
		public float thrusterMaxVolume = .2f;

		private AugmentedWheeledVehicle augmentedWheeledVehicle;

		protected override void Awake () 
		{
			base.Awake();

			// Obtain augmented wheeled vehicle reference.
			augmentedWheeledVehicle = GetComponent<AugmentedWheeledVehicle>();
			if(!augmentedWheeledVehicle) Debug.LogWarning("No AugmentedWheeledVehicle found for AugmentedWheeledVehicleSound on " + name);
		}

		protected override void Update () 
		{
			base.Update();

			if(!augmentedWheeledVehicle) return;

			if(augmentedWheeledVehicle.speedBoostFactor > 0)
			{
				if(!speedThrusterSound.isPlaying) speedThrusterSound.Play();

				// Modify boost thruster pitch and volume based on speed boost factor
				speedThrusterSound.pitch = Mathf.Lerp(thrusterMinPitch, thrusterMaxPitch, augmentedWheeledVehicle.speedBoostFactor);
				speedThrusterSound.volume = Mathf.Lerp(thrusterMinVolume, thrusterMaxVolume, augmentedWheeledVehicle.speedBoostFactor);
			}
			else if(speedThrusterSound.isPlaying) speedThrusterSound.Stop();
		}
	}
}
