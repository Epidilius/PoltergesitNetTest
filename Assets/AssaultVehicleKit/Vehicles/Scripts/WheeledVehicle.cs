using UnityEngine;
using System.Collections;
using System.Linq;

namespace hebertsystems.AVK
{
	//  The WheeledVehicle is a SteerableVehicle with, well, wheels.
	//  It provides some of the basic characteristics of
	//  a wheeled vehicle:
	//
	//  Max turn angle and turn speed for steering.
	//  Engine gear ratios and RPM calculations.
	//  Max forward and reverse speed settings.
	//  Max engine torque and a torque speed curve to simulate engine torque limits.
	//
	public class WheeledVehicle : SteerableVehicle 
	{
		[Header("WheeledVehicle Parameters")]

		public Transform centerOfMass;								// The center of mass for the vehicle.

		public float maxTurnSpeed = 90;								// The maximum turn speed (angles per sec) of the steering system.
		public float maxTurnSpeedAtMax = 90;						// The maximum turn speed at vehicle max speed.

		public float currentMaxTurnSpeed
		{
			get
			{
				float t = Mathf.InverseLerp(0, mAverageWheelSpeed >= 0 ? maxSpeedMPH : maxReverseSpeedMPH, Mathf.Abs(mAverageWheelSpeed));
				return Mathf.Lerp(maxTurnSpeed, maxTurnSpeedAtMax, t);
			}
		}

		public float maxTurnAngle = 30;								// The maximum turn angle.
		public float maxTurnAngleAtMax = 30;						// The maximum turn angle at vehicle max speed.

		public float currentMaxTurnAngle
		{
			get
			{
				float t = Mathf.InverseLerp(0, mAverageWheelSpeed >= 0 ? maxSpeedMPH : maxReverseSpeedMPH, Mathf.Abs(mAverageWheelSpeed));
				return Mathf.Lerp(maxTurnAngle, maxTurnAngleAtMax, t);
			}
		}

		public float maxSpeedMPH = 80;								// The maximum forward speed in MPH.
		public float maxReverseSpeedMPH = 20;						// The maximum reverse speed in MPH.

		public float maxTorque = 40;								// The max engine torque to apply to power wheels.
		public float maxBrakeTorque = 90;							// The max brake torque to apply to the brake wheels.
		public AnimationCurve torqueSpeedCurve;						// The engine torque speed curve to calculate ratio of maxTorque based on speed.

		public float minRPM = 800;									// The minimum RPM of the engine (idle).
		public float maxRPM = 5000;									// The maximum RPM of the engine.
		public float[] gearRatios;									// Set of gear ratios for forward driving.
		public float reverseGearRatio = 1;							// The reverse gear ratio.
		public float finalGearRatio = 4;							// The final drive ratio.

		public float orbitCameraPivotHeight = 2;					// The orbit camera pivot height above the position of the vehicle.

		public override Vector3 orbitCameraPivotBase 				// The pivot base for orbiting cameras.
		{
			get 
			{
				return transform.position;
			}
		}

		public override Vector3 orbitCameraPivotOffset 				// The offset from the pivot base for orbit camera pivot point.
		{
			get 
			{
				return Vector3.up * orbitCameraPivotHeight;
			}
		}

		public int currentGear		{get {return mCurrentGear;}}	// Current gear.

		public float engineRPM 		{get {return mEngineRPM;}}		// Current engine RPM
		public virtual float engineRatio							// The ratio of the engine RPM between min and max RPM.
		{
			get 
			{
				return Mathf.InverseLerp(minRPM, maxRPM, mEngineRPM);
			}
		}

		public override float steerAngleCurrent {get {return mSteerAngleCurrent;}}		// The current steer angle.
		public virtual float brake 				{get {return mBrake;}}					// The current braking ratio (0 to 1).
		public virtual bool handBrake 			{get {return mHandBrake;}}				// The current handbraking (on or off).

		public override Vector3 forward {get {return transform.forward;}}		// The forward direction of the vehicle.
		public override Vector3 up {get {return transform.up;}}					// The up direction of the vehicle
		public override Vector3 right {get {return transform.right;}}			// The right direction of the vehicle
		public override Vector3 position {get {return transform.position;}}		// The postion of the vehicle.
		public override Quaternion rotation {get {return transform.rotation;}}	// The rotation of the vehicle.

		public override VehicleInput input							// Process Vehicle input.
		{
			set
			{
				// If the throttle is negative (go in reverse), but we are still moving forward,
				// override the input and apply hand braking until we have stopped before going in reverse.
				if(value.forwardThrottle.HasValue && value.forwardThrottle < 0 && mAverageWheelSpeed > 1)
				{
					value.handBrake = true;
					value.forwardThrottle = 0;
				}

				// Pass input to SteerableVehicle first.
				base.input = value;

				// Process WheeledVehicle input specifics
				if(value.brake.HasValue) mBrake = Mathf.Clamp01(value.brake.Value);
				if(value.handBrake.HasValue) mHandBrake = value.handBrake.Value;
			}
		}

		[HideInInspector] public Wheel[] allWheels;
		[HideInInspector] public Wheel[] steerWheels;
		[HideInInspector] public Wheel[] powerWheels;
		[HideInInspector] public Wheel[] brakeWheels;
		[HideInInspector] public Wheel[] handbrakeWheels;

		protected override float steerAngleTarget						// The target steer angle.
		{ 
			get {return mSteerAngleTarget;}
			set {float currentTurnAngleMax = currentMaxTurnAngle; mSteerAngleTarget = Mathf.Clamp(value, -currentTurnAngleMax, currentTurnAngleMax);}
		}

		protected float mSteerAngleCurrent = 0;
		protected float mEngineRPM = 0;
		protected float mBrake = 0;
		protected float mAverageWheelSpeed = 0;
		protected float mMaxWheelSpeed = 0;
		protected int mCurrentGear = 0;

		protected bool mHandBrake = false;

		protected override void Awake () 
		{
			base.Awake();

			// Find all wheels
			allWheels = GetComponentsInChildren<Wheel>();

			// Build other wheel lists (steering, power, brake, etc.)
			steerWheels = allWheels.Where(w=>w.steering).ToArray();
			powerWheels = allWheels.Where(w=>w.power).ToArray();
			brakeWheels = allWheels.Where(w=>w.braking).ToArray();
			handbrakeWheels = allWheels.Where(w=>w.handBraking).ToArray();

			// Set rigidbody centerOfMass if one provided
			if(centerOfMass && mRigidbody) mRigidbody.centerOfMass = centerOfMass.localPosition;

			// Engine RPM intially at minimum RPM
			mEngineRPM = minRPM;

			// If no gears specified, create one with a 1:1 ratio
			if(gearRatios.Length == 0) 
			{
				gearRatios = new float[1];
				gearRatios[0] = 1;
			}
			// Set gear to first
			mCurrentGear = 0;

			// If no TorqueSpeedCurve provided, create a simple one that ramps from 1 to 0
			if(torqueSpeedCurve.keys.Length == 0)
			{
				torqueSpeedCurve.AddKey(new Keyframe(0,1));
				torqueSpeedCurve.AddKey(new Keyframe(1,0));
			}
		}

		protected virtual void FixedUpdate()
		{
			// Tasks to perform every physics update:

			ApplyThrottle();
			ApplyBraking();
			ApplyHandBrake();
			SteerVehicle();
			CalculateWheelSpeed();
			CalculateEngineRPM();
			AutoShiftGears();
		}

		protected virtual void ApplyThrottle()
		{
			// The following is a simplistic model of engine torque based on vehicle wheel speed,
			// not on a more realistic engine torque curve and gear ratios.

			// Calculate ratio of wheel speed to max speed, depending on forward or reverse
			float t = Mathf.InverseLerp(0, (mThrottle >= 0 ? maxSpeedMPH : maxReverseSpeedMPH), Mathf.Abs(mMaxWheelSpeed));
			
			// Calculate engine torque based on speed curve
			float engineTorque = torqueSpeedCurve.Evaluate( t ) * maxTorque;

			// Apply torque to PowerWheels
			if(powerWheels.Length == 0) return;
			float motorTorqueAtWheel = engineTorque * mThrottle / powerWheels.Length;
			foreach(Wheel wheel in powerWheels)
			{
				wheel.motorTorque = motorTorqueAtWheel;
			}
		}
		
		protected virtual void ApplyBraking()
		{
			if(brakeWheels.Length == 0) return;

			// Apply braking to BrakeWheels
			float brakeTorqueAtWheel = maxBrakeTorque * mBrake / brakeWheels.Length;
			foreach(Wheel wheel in brakeWheels)
			{
				wheel.brakeTorque = brakeTorqueAtWheel;
			}
		}

		protected virtual void ApplyHandBrake()
		{
			if(!mHandBrake || handbrakeWheels.Length == 0) return;
			
			// Apply braking to BrakeWheels
			float brakeTorqueAtWheel = maxBrakeTorque / handbrakeWheels.Length;
			foreach(Wheel wheel in handbrakeWheels)
			{
				wheel.brakeTorque = brakeTorqueAtWheel;
			}
		}

		protected virtual void SteerVehicle()
		{
			// Move current steer angle towards target at maxTurnAnglePerSec
			mSteerAngleCurrent = Mathf.MoveTowards(mSteerAngleCurrent, mSteerAngleTarget, currentMaxTurnSpeed * Time.deltaTime);
			
			// Set steer angles for SteerWheels
			foreach(Wheel wheel in steerWheels)
			{
				wheel.steerAngle = mSteerAngleCurrent;
			}
		}

		protected virtual void CalculateWheelSpeed()
		{
			// Iterate through powered wheels to determine average and max wheel speed for use in calculating engine torque
			mAverageWheelSpeed = 0;
			mMaxWheelSpeed = 0;
			foreach(Wheel wheel in powerWheels)
			{
				mAverageWheelSpeed += wheel.wheelSpeedOnGround;
				mMaxWheelSpeed = Mathf.Max(mMaxWheelSpeed, Mathf.Abs(wheel.wheelSpeedOnGround));
			}
			if(powerWheels.Length > 0) mAverageWheelSpeed /= powerWheels.Length;
		}

		protected virtual void CalculateEngineRPM()
		{
			// Simplistic model of engine RPM tied directly to transmission RPM, based on wheel RPM and gear ratios

			// Iterate through powered wheels to average wheel RPM
			float transmisionRPM = 0;
			foreach(Wheel wheel in powerWheels)
			{
				transmisionRPM += wheel.rpm;
			}
			if(powerWheels.Length > 0) transmisionRPM /= powerWheels.Length;

			// Calculate transmision RPM based on gear ratios and final gear ratio
			transmisionRPM *= finalGearRatio * (throttle >= 0 ? gearRatios[mCurrentGear] : reverseGearRatio);

			// Clamp engine RPM between min and max
			mEngineRPM = Mathf.Clamp(Mathf.Abs(transmisionRPM), minRPM, maxRPM);
		}

		protected virtual void AutoShiftGears()
		{
			// Simplistic model of gear shifting based on max engine RPM.

			// If going forward, gear up when engine RPM reaches portion of maxRPM
			if (mThrottle > 0 && mEngineRPM >= maxRPM * (0.5f + 0.4f * mThrottle))
			{
				mCurrentGear++;
			}
			// Gear down when not throttling forward and engine RPM slows down
			else if (mThrottle <= 0 && mEngineRPM <= maxRPM * 0.25f)
			{
				mCurrentGear--;
			}

			// Make sure gear is clamped between range of available gears
			mCurrentGear = Mathf.Clamp(mCurrentGear, 0, gearRatios.Length - 1);
		}
	}
}
