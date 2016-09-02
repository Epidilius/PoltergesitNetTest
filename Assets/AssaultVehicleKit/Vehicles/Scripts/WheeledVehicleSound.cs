using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Provides standard sound response for WheeledVehicles.
	//
	[RequireComponent(typeof(WheeledVehicle))]
	public class WheeledVehicleSound : MonoBehaviour 
	{
		// Engine sound settings.
		public AudioSource engineSound;

		public float engineMinPitch = .35f;
		public float engineMaxPitch = 1.25f;
		public float engineMinVolume = .1f;
		public float engineMaxVolume = .2f;
		public float engineMinRPM = 1000;
		public float engineMaxRPM = 3000;

		// Tire noise sound settings.
		public AudioSource tirenoise;

		public float tireNoiseMinVolume = .1f;
		public float tireNoiseMaxVolume = .5f;
		public float tireNoiseMinPitch = .8f;
		public float tireNoiseMaxPitch = 1.5f;

		// Tire impact sound settings.
		public AudioSource tireImpact;

		public float tireThudMinForce = 0f;
		public float tireThudMaxForce = 30;
		public float tireThudMinPitch = .17f;
		public float tireThudMaxPitch = .3f;
		public float tireThudMinVolume = .1f;
		public float tireThudMaxVolume = .2f;

		// Body impact sound settings.
		public AudioSource bodyImpact;

		public float impactMin = 1;
		public float impactMax = 30;
		public float impactMinVolume = .5f;
		public float impactMaxVolume = 1;
		public float impactMinPitch = .8f;
		public float impactMaxPitch = 1;
		public float impactResetTime = .1f;
		private float nextImpactSoundTime = 1;

		// Wheel suspension sound settings.
		public AudioSource suspensionCompression;

		public float compressionMinDelta = .25f;
		public float compressionMaxDelta = 1f;
		public float suspensionMinPitch = .5f;
		public float suspensionMaxPitch = 1;
		public float suspensionMinVolume = .05f;
		public float suspensionMaxVolume = .1f;

		public float suspensionResetTime = .1f;

		// Tire scrape sound settings (not quite a skid...)
		public AudioSource tireScrape;

		public float scrapeSlipMin = 40;
		public float scrapeSlipMax = 120;
		public float scrapePitchMin = .8f;
		public float scrapePitchMax = 1.2f;
		public float scrapeVolumeMax = .5f;

		// Tire skid sound settings.
		public AudioSource tireSkid;
		
		public float skidSlipMin = 40;
		public float skidSlipMax = 120;
		public float skidPitchMin = .8f;
		public float skidPitchMax = 1.2f;
		public float skidVolumeMax = .5f;


		private WheeledVehicle wheeledVehicle;
		private bool wheelsGrounded = false;
		private float nextSuspensionSoundTime = 1;

		protected virtual void Awake () 
		{
			// Obtain Wheeled vehicle reference.
			wheeledVehicle = GetComponent<WheeledVehicle>();
			if(!wheeledVehicle) Debug.LogWarning("No WheeledVehicle found for WheeledVehicleSound on " + name);
		}

		protected virtual void Update () 
		{
			if(!wheeledVehicle) return;

			// Modify engine pitch and volume based on vehicle RPM readings between min and max (engineRatio of WheeledVehicle).
			engineSound.pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, wheeledVehicle.engineRatio);
			engineSound.volume = Mathf.Lerp(engineMinVolume, engineMaxVolume, wheeledVehicle.engineRatio);
		}

		protected virtual void FixedUpdate()
		{
			float t;

			wheelsGrounded = false;
			float averageSpeed = 0;
			float maxSkid = 0;

			// Iterate through all wheels and gather info for sounds.
			foreach(Wheel wheel in wheeledVehicle.allWheels)
			{
				if(wheel.isGrounded)
				{
					wheelsGrounded = true;
					averageSpeed += wheel.wheelSpeedOnGround;

					// Play tire impact if the wheel has fresh contact.
					if(wheel.freshContact) 
					{
						// Calculate ratio of tire impact between min and max force and apply that to volume and pitch.
						t = Mathf.InverseLerp(tireThudMinForce, tireThudMaxForce, wheel.force);
						tireImpact.volume = Mathf.Lerp(tireThudMinVolume, tireThudMaxVolume, t);
						tireImpact.pitch = Mathf.Lerp(tireThudMinPitch, tireThudMaxPitch, t);
						tireImpact.Play();
					}

					// Play wheel compression sound if compression rate great enough, and past the compression reset time.
					if(wheel.compressionRate >= compressionMinDelta && Time.time >= nextSuspensionSoundTime) 
					{
						// Calculate compression ratio between min and max and apply that to volume and pitch.
						t = Mathf.InverseLerp(compressionMinDelta, compressionMaxDelta, wheel.compressionRate);
						suspensionCompression.volume = Mathf.Lerp(suspensionMinVolume, suspensionMaxVolume, t);
						suspensionCompression.pitch = Mathf.Lerp(suspensionMinPitch, suspensionMaxPitch, t);
						suspensionCompression.Play();

						nextSuspensionSoundTime = Time.time + suspensionResetTime;
					}

					// Obtain the max slip of the tires, forward or side
					if(Mathf.Abs(wheel.sidewaysSlip) > maxSkid) maxSkid = Mathf.Abs(wheel.sidewaysSlip);
					if(Mathf.Abs(wheel.forwardSlip) > maxSkid) maxSkid = Mathf.Abs(wheel.forwardSlip);
				}
			}
			if(wheeledVehicle.allWheels.Length > 0) averageSpeed /= wheeledVehicle.allWheels.Length;

			// Vary the tire scrape sound volume and pitch based on min and max scrape slip settings.
			t = Mathf.InverseLerp(scrapeSlipMin, scrapeSlipMax, maxSkid);
			tireScrape.pitch = Mathf.Lerp(scrapePitchMin, scrapePitchMax, t);
			tireScrape.volume = Mathf.Lerp(0, scrapeVolumeMax, t);

			// Vary the tire skid sound volume and pitch based on min and max skid slip settings.
			t = Mathf.InverseLerp(skidSlipMin, skidSlipMax, maxSkid);
			tireSkid.pitch = Mathf.Lerp(skidPitchMin, skidPitchMax, t);
			tireSkid.volume = Mathf.Lerp(0, skidVolumeMax, t);

			// Vary the tire noise sound volume and pitch based on wheel speed up to maxSpeedMPH
			t = wheelsGrounded ? Mathf.InverseLerp(0.1f, wheeledVehicle.maxSpeedMPH, Mathf.Abs(averageSpeed)) : 0;
			tirenoise.volume = Mathf.Lerp(tireNoiseMinVolume, tireNoiseMaxVolume, t);
			tirenoise.pitch = Mathf.Lerp(tireNoiseMinPitch, tireNoiseMaxPitch, t);
		}

		protected virtual void OnCollisionEnter(Collision collisionInfo)
		{
			foreach(ContactPoint contact in collisionInfo.contacts)
			{
				// If impact is from something other than one of the wheels, play a body impact sound
				if(!(contact.thisCollider is WheelCollider || contact.otherCollider is WheelCollider))
				{
					// If we are ready to play body impact again, play it.
					if(Time.time >= nextImpactSoundTime)
					{
						// Vary volume and pitch based on ratio of impact velocity between min and max.
						float t = Mathf.InverseLerp(impactMin, impactMax, collisionInfo.relativeVelocity.magnitude);
						bodyImpact.volume = Mathf.Lerp(impactMinVolume, impactMaxVolume, t);
						bodyImpact.pitch = Mathf.Lerp(impactMinPitch, impactMaxPitch, t);
						bodyImpact.Play();

						// Reset next impact sound time 
						nextImpactSoundTime = Time.time + impactResetTime;
						break;
					}
				}
			}
		}
	}
}
