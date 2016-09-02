using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Provides sound response for the RollerBotVehicle.
	//
	[RequireComponent(typeof(RollerBotVehicle))]
	public class RollerBotSound : MonoBehaviour 
	{
		// Engine sound settings.
		public AudioSource engineSound;

		public float engineMinPitch = .5f;
		public float engineMaxPitch = 1.25f;
		public float engineMinVolume = .1f;
		public float engineMaxVolume = .2f;

		// Speed boost sound
		public AudioSource speedBoost;

		public float boostMinPitch = .5f;
		public float boostMaxPitch = 1;
		public float boostMinVolume = .01f;
		public float boostMaxVolume = .5f;

		private RollerBotVehicle vehicle;

		void Awake () 
		{
			// Obtain reference to RollerBotVehicle
			vehicle = GetComponentInChildren<RollerBotVehicle>();
			if(!vehicle) Debug.Log("No RollerBotVehicle found for RollerBotSound on " + name);
		}

		void Update () 
		{
			if(!vehicle) return;

			// Calculate the ratio of angular velocity to the max angular velocity
			float t = Mathf.InverseLerp(0, vehicle.maxAngularVelocity, vehicle.angularVelocity.magnitude);

			// Alter pitch and volume of engine.
			if(engineSound)
			{
				engineSound.pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, t);
				engineSound.volume = Mathf.Lerp(engineMinVolume, engineMaxVolume, t);
			}

			// Alter pitch and volume of speed boost.
			if(speedBoost)
			{
				if(vehicle.speedBoostFactor > 0)
				{
					if(!speedBoost.isPlaying) speedBoost.Play();
					speedBoost.pitch = Mathf.Lerp(boostMinPitch, boostMaxPitch, vehicle.speedBoostFactor);
					speedBoost.volume = Mathf.Lerp(boostMinVolume, boostMaxVolume, vehicle.speedBoostFactor);
				}
				else if (speedBoost.isPlaying) speedBoost.Stop();
			}
		}
	}
}
