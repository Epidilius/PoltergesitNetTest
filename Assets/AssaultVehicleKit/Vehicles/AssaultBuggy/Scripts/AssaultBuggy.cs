using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hebertsystems.AVK
{
	//  The AssualtBuggy is just an AugmentedWheeledVehicle with
	//  some specific behaviors.
	//
	//  Contains a steering wheel mesh to visualize the steering angle.
	//  Brake and Rear lights.
	//  Auto braking when no throttle provided.
	//  Speed boost effect (camera distance and particles).
	//
	public class AssaultBuggy : AugmentedWheeledVehicle 
	{
		[Header("AssaultBuggy Parameters")]

		public float speedBoostOrbitCameraDistance = 10;					// The distance of the orbit camera when in full speed boost mode.

		public ParticleSystem leftBoostThruster;							// Reference to the left and right boost thruster particle systems.
		public ParticleSystem rightBoostThruster;

		public Transform steeringWheel;										// Transform of the steering wheel, to be rotated along with wheels
		public float steeringWheelRotationFactor = 1;						// The rotation factor of steering wheel relative to wheels (1 would rotate same amount)

		[Range(0,1)] public float autoBrakingFactor = .25f;					// Brake factor to apply when no throttle is supplied


		public Transform cameraPivotBase;									// Camera pivot base to offset orbit camera pivot point from center of vehicle.

		public override Vector3 orbitCameraPivotBase 						// The pivot base for orbiting cameras.
		{
			get {return cameraPivotBase.position;}
		}

		public Light[] brakeLights;											// Brake lights, will light up when hand brake pressed.
		public Light[] rearLights;											// Rear lights, light up when in reverse.

		protected float originalOrbitCameraDistance;

		protected override void Awake () 
		{
			base.Awake();

			originalOrbitCameraDistance = orbitCameraDistance;
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();
		}

		void LateUpdate () 
		{
			// Rotate steering wheel based on current steer angle and steering wheel rotation factor
			steeringWheel.transform.localRotation = Quaternion.Euler(0, 0, steerAngleCurrent * steeringWheelRotationFactor);

			// Adjust orbit camera distance based on speed boost factor
			orbitCameraDistance = Mathf.Lerp(originalOrbitCameraDistance, speedBoostOrbitCameraDistance, mSpeedBoostFactor);

			// Adjust boost thruster effects.
			if(mSpeedBoostFactor > 0)
			{
				Color thrusterStartColor = Color.white;
				thrusterStartColor.a = mSpeedBoostFactor;

				if(leftBoostThruster)
				{
					if(!leftBoostThruster.isPlaying) leftBoostThruster.Play();
					leftBoostThruster.startColor = thrusterStartColor;
				}
				
				if(rightBoostThruster)
				{
					if(!rightBoostThruster.isPlaying) rightBoostThruster.Play();
					rightBoostThruster.startColor = thrusterStartColor;
				}
			}
			else
			{
				if(leftBoostThruster && leftBoostThruster.isPlaying) leftBoostThruster.Stop();
				if(rightBoostThruster && rightBoostThruster.isPlaying) rightBoostThruster.Stop();
			}

			// Lights
			// Brake lights (handbrake)
			foreach(Light brakeLight in brakeLights)
			{
				brakeLight.gameObject.SetActive(handBrake);
			}

			// Rear Lights
			foreach(Light rearLight in rearLights)
			{
				rearLight.gameObject.SetActive(throttle < 0);
			}

			// Apply auto-braking if no throttle, affected by the slope of the ground the wheels are on -
			// reduce auto-braking when on heavy slopes, otherwise the vehicle might flip over.
			mBrake = 0;
			if(mThrottle == 0)
			{
				float slopeFactor = 1;
				foreach(Wheel wheel in allWheels)
				{
					slopeFactor *= Vector3.Dot(wheel.groundNormal, Vector3.up);
				}
				mBrake = autoBrakingFactor * slopeFactor;
			}
		}
	}
}
