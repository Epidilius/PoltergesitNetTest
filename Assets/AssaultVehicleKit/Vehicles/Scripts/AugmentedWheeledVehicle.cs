using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hebertsystems.AVK
{
	//  The AugmentedWheeledVehicle is a WheeledVehicle with "fun" based
	//  augmentations, like:
	//  
	//  Down force applied for increased stability.
	//  Correction in air for facing velocity and attempts to land back on wheels.
	//  Flips the vehicle back onto wheels if it landed on it's side or back.
	//  Alters the slip curves of the wheels for increase stability and fun control.
	//  Augments handbraking power slides.
	//
	public class AugmentedWheeledVehicle : WheeledVehicle 
	{
		[Header("Augmented WheeledVehicle Parameters")]

		public Wheel[] frontWheels;									// Front and Rear wheels of vehicle, used to modify friction curves.
		public Wheel[] rearWheels;

		public float maxDownforce = 20;								// Max downforce to be applied to vehicle to provide extra stability.
		public AnimationCurve downforceCurve;						// Curve of down force with respect to slope of ground being driven on.

		public float speedBoostFactor								// Current speed boost factor (read only).
		{
			get {return mSpeedBoostFactor;}
		}
		public float speedBoostSmoothTime = .5f;					// The speed boost dampening smooth time.
		public float maxTorqueWithBoost = 100;						// The max engine torque with speed boost.
		public float maxReverseTorqueWithBoost = 10;				// The max engine torque for reverse with speed boost (boost should act against going in reverse, so slower than usual).
		public float maxSpeedWithBoost = 100;						// The max speed with speed boost;
		public float maxReverseSpeedWithBoost = 5;					// The max reverse speed whith speed boost.

		public float maxHandbrakeSideForce = 1;						// The max force to help power slide the vehicle on handbrake turns.
		
		public float forwardSidewaysStiffnessMin = .3f;				// The forward wheel side stiffness minimum.
		public float forwardSidewaysStiffnessMax = .8f;				// The forward wheel side stiffness maximum.
		public float rearSidewaysStiffnessMin = .5f;				// The rear wheel side stiffness minimum.
		public float rearSidewaysStiffnessMax = 1f;					// The rear wheel side stiffness maximum.

		public float inAirAntiRollTorqueMax = 5;					// The max torque to apply to the vehicle to correct itself in air.
		public float flipOverTorqueMax = 110;						// The max impulse torque to apply to the vehicle to flip back over.

		public override VehicleInput input							// Process Vehicle input.
		{
			set
			{
				// Pass input to WheeledVehicle first.
				base.input = value;
				
				// Process AugmentedWheeledVehicle input specifics
				if(value.speedBoost.HasValue) mSpeedBoost = value.speedBoost.Value;
			}
		}


		// Internal class used to store friction curves of wheels.
		protected class WheelCurves
		{
			public WheelCollider wheelCollider;
			public WheelFrictionCurve forwardFrictionCurve;
			public WheelFrictionCurve sidewaysFrictionCurve;
			
			public WheelCurves(WheelCollider wheel, WheelFrictionCurve forwardCurve, WheelFrictionCurve sidewaysCurve)
			{
				wheelCollider = wheel;
				forwardFrictionCurve = forwardCurve;
				sidewaysFrictionCurve = sidewaysCurve;
			}
		}
		
		protected List<WheelCurves> mFrontCurves = new List<WheelCurves>();
		protected List<WheelCurves> mRearCurves = new List<WheelCurves>();

		protected int mLastFixedUpdateFrame = 0;
		protected int mLastCollisionFrame = 0;
		protected float mBodyCollisionTimer = 0;
		protected bool mBodyCollision = false;
		protected bool mSpeedBoost = false;
		protected float mSpeedBoostFactor = 0;
		private float mSpeedBoostFactorVelocity = 0;
		private float mOriginalMaxTorque;
		private float mOriginalMaxSpeedMPH;
		private float mOriginalMaxReverseSpeedMPH;
		
		protected override void Awake () 
		{
			base.Awake();

			mOriginalMaxTorque = maxTorque;
			mOriginalMaxSpeedMPH = maxSpeedMPH;
			mOriginalMaxReverseSpeedMPH = maxReverseSpeedMPH;
			
			// Obtain forward and sideways friction curves for front wheels
			foreach(Wheel wheel in frontWheels)
			{
				mFrontCurves.Add(new WheelCurves(wheel.wheelCollider, wheel.wheelCollider.forwardFriction, wheel.wheelCollider.sidewaysFriction));
			}
			
			// Obtain forward and sideways friction curves for rear wheels
			foreach(Wheel wheel in rearWheels)
			{
				mRearCurves.Add(new WheelCurves(wheel.wheelCollider, wheel.wheelCollider.forwardFriction, wheel.wheelCollider.sidewaysFriction));
			}

			// If no downforce curve provided, add linear dropoff values.
			if(downforceCurve.keys.Length == 0)
			{
				downforceCurve.AddKey(0,1);
				downforceCurve.AddKey(1,0);
			}
		}
		
		protected override void FixedUpdate()
		{
			// Calculate friction curves before calling base.FixedUpdate since the ApplyHandBrake() is called within base.FixedUpdate()
			// and we override ApplyHandBrake so as to alter the initially calculated curves.
			CalculateFrictionCurves();

			// Alter torque and max speeds based on speed boost.
			// Supplying a speed boost this way is much more controllable and feels better than applying
			// an actual thrust force behind the vehicle.
			maxTorque = mThrottle >= 0 ? Mathf.Lerp(mOriginalMaxTorque, maxTorqueWithBoost, mSpeedBoostFactor) : Mathf.Lerp(mOriginalMaxTorque, maxReverseTorqueWithBoost, mSpeedBoostFactor);
			maxSpeedMPH = Mathf.Lerp(mOriginalMaxSpeedMPH, maxSpeedWithBoost, mSpeedBoostFactor);
			maxReverseSpeedMPH = Mathf.Lerp(mOriginalMaxReverseSpeedMPH, maxReverseSpeedWithBoost, mSpeedBoostFactor);

			base.FixedUpdate();

			SetFrictionCurves();
			ApplySpeedBoost();
			ApplyDownforce();
			RollBackOntoWheels();
			PreserveDirectionInAir();
			
			// Save frameCount of this FixedUpdate
			mLastFixedUpdateFrame = Time.frameCount;
		}
		
		protected virtual void CalculateFrictionCurves()
		{
			// Obtain relative velocity of rigidbody.
			Vector3 rigidbodyVelocity = mRigidbody ? mRigidbody.velocity : Vector3.zero;
			Vector3 relativeVelocity = transform.InverseTransformDirection(rigidbodyVelocity);

			// Calculate speed factor of velocity between 0 and max speed in MPH.
			float speedFactor = Mathf.InverseLerp(0, maxSpeedMPH, rigidbodyVelocity.magnitude * 2.2369356f);
			
			// Set sideways stiffness values based on speed (front wheels) and steer angle relative to max turn angle (rear wheels).
			float frontStiffness = Mathf.Lerp(forwardSidewaysStiffnessMax, forwardSidewaysStiffnessMin, speedFactor);
			float rearStiffness = Mathf.Lerp(rearSidewaysStiffnessMax, rearSidewaysStiffnessMin, Mathf.Abs(steerAngleTarget)/currentMaxTurnAngle);

			// Set sideways stiffness values depending on movement direction (forward or reverse).
			// If moving forward
			if(relativeVelocity.z >= 0)
			{			
				foreach(WheelCurves wheelCurve in mFrontCurves)
				{
					wheelCurve.forwardFrictionCurve.stiffness = 1;
					wheelCurve.sidewaysFrictionCurve.stiffness = frontStiffness;
				}
				
				foreach(WheelCurves wheelCurve in mRearCurves)
				{
					wheelCurve.forwardFrictionCurve.stiffness = 1;
					wheelCurve.sidewaysFrictionCurve.stiffness = rearStiffness;
				}
			}
			// Moving backward
			else
			{
				foreach(WheelCurves wheelCurves in mFrontCurves)
				{
					wheelCurves.forwardFrictionCurve.stiffness = 1;
					wheelCurves.sidewaysFrictionCurve.stiffness = rearStiffness;
				}
				
				foreach(WheelCurves wheelCurves in mRearCurves)
				{
					wheelCurves.forwardFrictionCurve.stiffness = 1;
					wheelCurves.sidewaysFrictionCurve.stiffness = frontStiffness;
				}
				
			}
		}

		protected virtual void ApplySpeedBoost()
		{
			// Smooth out the speed boost force factor.
			mSpeedBoostFactor = Mathf.SmoothDamp(mSpeedBoostFactor, (mSpeedBoost ? 1f : 0f), ref mSpeedBoostFactorVelocity, speedBoostSmoothTime);
			if(!mSpeedBoost && mSpeedBoostFactor < .001f) mSpeedBoostFactor = 0;
		}
		
		protected override void ApplyHandBrake()
		{
			if(handBrake && mRigidbody)
			{
				// Obtain relative velocity of rigidbody used for calculations.
				Vector3 relativeVelocity = transform.InverseTransformDirection(mRigidbody.velocity);
				// Calculate ratio of forward velocity to a portion of the max speed for use in wheel friction curves.
				float t = Mathf.InverseLerp(0, maxSpeedMPH/4, Mathf.Abs(relativeVelocity.z) * 2.23694f);

				// Adjust friction curves to augment the handBrake slide
				foreach(WheelCurves wheelCurve in mFrontCurves)
				{
					wheelCurve.forwardFrictionCurve.stiffness *= t;
					wheelCurve.sidewaysFrictionCurve.stiffness *= .4f;
				}
				
				foreach(WheelCurves wheelCurve in mRearCurves)
				{
					wheelCurve.forwardFrictionCurve.stiffness = Mathf.Lerp(.05f,0, t);
					wheelCurve.sidewaysFrictionCurve.stiffness *= .4f;
				}

				// Just, just lock up the rear wheels.
				foreach(Wheel wheel in rearWheels)
				{
					wheel.motorTorque = 0;
					wheel.brakeTorque = maxBrakeTorque * 100;
				}

				// Determine if all wheels grounded.
				bool allGrounded = true;
				foreach(Wheel wheel in allWheels)
				{
					if(!wheel.isGrounded)
					{
						allGrounded = false;
						break;
					}
				}

				// If all wheels grounded, apply power slide force.
				if(allGrounded)
				{
					// Side Vector based on steer angle
					Vector3 sideVector = Quaternion.Euler(0,-steerAngleCurrent,0) * Vector3.forward;
					float signedXComponent = sideVector.x;
					float xComponent = Mathf.Abs(sideVector.x);
					// Reduce any forward component of side vector and apply a down component multiple of the X component for stability.
					sideVector.z *= .2f;
					sideVector.y = -xComponent*2;

					// Factor in velocity.
					sideVector *= mRigidbody.velocity.magnitude;
					// Bring sideVector to world space.
					sideVector = transform.TransformVector(sideVector);
					
					// Apply max hand brake force as a factor of the x component of side vector (the more we turn, the more side force).
					sideVector *= maxHandbrakeSideForce * xComponent;

					// Apply power slide forces.
					mRigidbody.AddForce(sideVector);
					mRigidbody.AddTorque(transform.up * -signedXComponent * maxHandbrakeSideForce * 10);
				}
			}
		}
		
		protected virtual void SetFrictionCurves()
		{
			// Set front wheel curves
			foreach(WheelCurves wheelCurve in mFrontCurves)
			{
				wheelCurve.wheelCollider.forwardFriction = wheelCurve.forwardFrictionCurve;
				wheelCurve.wheelCollider.sidewaysFriction = wheelCurve.sidewaysFrictionCurve;
			}
			
			// Set rear wheel curves
			foreach(WheelCurves wheelCurve in mRearCurves)
			{
				wheelCurve.wheelCollider.forwardFriction = wheelCurve.forwardFrictionCurve;
				wheelCurve.wheelCollider.sidewaysFriction = wheelCurve.sidewaysFrictionCurve;
			}
		}
		
		protected virtual void ApplyDownforce ()
		{
			// If no downforce, return
			if(maxDownforce == 0 || !mRigidbody) return;
			
			// Calculate the slope factor of any wheels on the ground (1 means level ground, 0 is 90 degree wall).
			float count = 0;
			float t = 1; // slope factor
			foreach(Wheel wheel in allWheels)
			{
				if(wheel.isGrounded)
				{
					t *= Vector3.Dot(wheel.groundNormal, Vector3.up);
					count++;
				}
			}
			// Add in relative orientation of the vehicle as well
			t *= Vector3.Dot(transform.up, Vector3.up);
			
			// Apply stabilizing down force if 2 or more wheels are on the ground.
			if (count >= 2)
			{
				// Apply down force according to downforceCurve and the slope factor (add a little extra when speed boost engaged)
				mRigidbody.AddForce(-transform.up * downforceCurve.Evaluate(1-t) * maxDownforce * Mathf.Lerp(1, 2, mSpeedBoostFactor) );
			}
		}
		
		protected virtual void RollBackOntoWheels()
		{
			// Simplistic algorithm for determining if we need to roll back onto wheels:
			// The angle of the vehicle with respect to up is over 45 degrees.
			// The vehicle movement is minimal.
			// And no body collisions in the last second.
			if(mRigidbody && Vector3.Dot(transform.up, Vector3.up) < 0.525322f && mRigidbody.velocity.magnitude < 1f && mBodyCollisionTimer > 1)
			{
				// Calculate roll torque to bring vehicle back to horizontal.
				Vector3 zeroRollVector = Quaternion.LookRotation(transform.forward) * Vector3.right;
				Vector3 relativeZeroRoll = transform.InverseTransformDirection(zeroRollVector);
				float rollAngle = Mathf.Atan2(relativeZeroRoll.y, relativeZeroRoll.x) * Mathf.Rad2Deg;
				float antiRollTorque = rollAngle/180 * flipOverTorqueMax;

				// Calculate pitch torque to bring vehicle from vertical to horizontal.
				Vector3 relativeZeroPitch = transform.InverseTransformDirection(Vector3.up);
				float pitchAngle = Mathf.Atan2(relativeZeroPitch.z, relativeZeroPitch.y) * Mathf.Rad2Deg;
				float antiPitchTorque = pitchAngle/180 * flipOverTorqueMax / 4;

				// Apply impulse roll over torque forces.
				mRigidbody.AddTorque( transform.forward * antiRollTorque, ForceMode.Impulse);
				mRigidbody.AddTorque( transform.right * antiPitchTorque, ForceMode.Impulse);

				// Reset body collision timer.
				mBodyCollisionTimer = 0;
			}
		}
		
		protected virtual void PreserveDirectionInAir()
		{
			// If we are in the air (no collision after the last FixedUpdate), preserve forward direction.
			// Note:  Attempted to use rigidbody.MoveRotation to correct rotation in air, but that method
			//        appeared to cause jittery behavior, even with rigidbodies in Interpolate mode.
			//        So adding torque in air instead seems much smoother.
			if(mRigidbody && mLastCollisionFrame != mLastFixedUpdateFrame)
			{
				// Calculate roll torque to apply to vehicle to get it back horizontal from a roll.
				Vector3 zeroRollVector = Quaternion.LookRotation(transform.forward) * Vector3.right;
				Vector3 relativeZeroRoll = transform.InverseTransformDirection(zeroRollVector);
				float rollAngle = Mathf.Atan2(relativeZeroRoll.y, relativeZeroRoll.x) * Mathf.Rad2Deg;
				float antiRollTorque = rollAngle/180 * inAirAntiRollTorqueMax;
				
				// Calculate yaw torque to get vehicle facing forward velocity
				Vector3 relativeZeroYaw = transform.InverseTransformDirection(mRigidbody.velocity);
				float yawAngle = Mathf.Atan2(relativeZeroYaw.x, relativeZeroYaw.z) * Mathf.Rad2Deg;
				float antiYawTorque = yawAngle/180 * inAirAntiRollTorqueMax;

				// Calculate pitch torque to get vehicle pitch ligned up with rigidbody velocity.
				float pitchAngle = Mathf.Atan2(-relativeZeroYaw.y, relativeZeroYaw.z) * Mathf.Rad2Deg;
				float pitchTorque = pitchAngle/180 * inAirAntiRollTorqueMax;

				// Apply mid-air torque forces
				mRigidbody.AddTorque( transform.up * antiYawTorque);
				mRigidbody.AddTorque( transform.forward * antiRollTorque);
				mRigidbody.AddTorque( transform.right * pitchTorque);
				// Reduce angular velocity
				mRigidbody.angularVelocity = Vector3.Lerp(mRigidbody.angularVelocity, Vector3.zero, Time.deltaTime);
			}
		}
		
		void OnCollisionStay(Collision collision)
		{
			mBodyCollision = false;
			foreach(ContactPoint contact in collision.contacts)
			{
				// If contact with a anything besides wheels, indicate body collision.
				if(!(contact.thisCollider is WheelCollider || contact.otherCollider is WheelCollider))
				{
					mBodyCollision = true;
				}
			}

			// Update body collision timer.
			if(mBodyCollision) mBodyCollisionTimer += Time.deltaTime;
			else mBodyCollisionTimer = 0;
			
			// Save frameCount of the last collision with anything.
			mLastCollisionFrame = Time.frameCount;
		}
	}
}
