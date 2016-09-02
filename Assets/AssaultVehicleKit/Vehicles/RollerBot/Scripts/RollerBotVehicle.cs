using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  This behavior provides Vehicle control for the RollerBot -
	//  basically just a sphere rolling around with torque.
	//
	public class RollerBotVehicle : Vehicle 
	{
		public override Vector3 orbitCameraPivotBase 				// The pivot base for orbiting cameras.
		{
			get 
			{
				if(cameraPivot) return cameraPivot.transform.position; 
				return transform.position;
			}
		}
		
		public override Vector3 orbitCameraPivotOffset 				// The offset from the pivot base for orbit camera pivot point.
		{
			get 
			{
				if(cameraPivot) return Vector3.zero; 
				return  Vector3.up * 2;
			}
		}

		[Header("RollerBot Parameters")]
		public float speedBoostOrbitCameraDistance = 10;			// The distance of the orbit camera when in full speed boost mode.

		public Transform cameraPivot;								// Reference to the orbit camera pivot Transform.

		public float maxTorque = 1000;								// Max torque to apply to sphere for movement.
		public float maxAngularVelocity = 30;						// Max angular velocity of sphere.
		public float minAngularVelocity = 5;						// Min angular velocity of sphere.

		public float turnSmoothTime = .2f;							// Turn dampening smooth time for changing direction.
				
		public float extraGravity = 1;								// Extra gravitational force to apply to the RollerBot for stability.

		public Vector3 angularVelocity								// Current angular velocity (read only).
		{
			get
			{
				if(mRigidbody) return mRigidbody.angularVelocity;
				return Vector3.zero;
			}
		}
		public float speedBoostFactor								// Current speed boost factor (read only).
		{
			get {return mSpeedBoostFactor;}
		}
		public float speedBoostSmoothTime = .5f;					// The speed boost dampening smooth time.
		public float maxAngularVelocityWithBoost = 50;				// Max angular velocity with speed boost.
		public ParticleSystem boostParticles;						// Reference to particle system to visualize speed boost for RollerBot.
		
		public Transform top;										// Reference to the top of the RollerBot.

		public override Vector3 forward {get {return currentMoveDirection;}}					// The forward direction of the vehicle.
		public override Vector3 up {get {return Vector3.up;}}									// The up direction of the vehicle
		public override Vector3 right {get {return Vector3.Cross(Vector3.up, currentMoveDirection);}} // The right direction of the vehicle
		public override Vector3 position {get {return transform.position;}}						// The postion of the vehicle.
		public override Quaternion rotation {get {return Quaternion.LookRotation(forward);}}	// The rotation of the vehicle.

		public override VehicleInput input							// Process Vehicle input.
		{
			set
			{
				if(value.forwardThrottle.HasValue) mForwardThrottle = Mathf.Clamp(value.forwardThrottle.Value, -1, 1);
				if(value.sideThrottle.HasValue) mSideThrottle = Mathf.Clamp(value.sideThrottle.Value, -1, 1);
				if(value.brake.HasValue) mBrake = Mathf.Clamp01(value.brake.Value);
				if(value.handBrake.HasValue) mHandbrake = value.handBrake.Value ? 0 : 1;
				if(value.speedBoost.HasValue) mSpeedBoost = value.speedBoost.Value;
				
				if(value.moveDirection.HasValue) 
				{
					moveDirection = value.moveDirection.Value;
					moveDirection.y = 0;
				}
				if(value.relativeMoveDirection.HasValue)
				{
					Vector3 relativeMoveDirection = value.relativeMoveDirection.Value;
					relativeMoveDirection.y = 0;
					if(!relativeMoveDirection.Equals(Vector3.zero))
					{
						moveDirection = Quaternion.LookRotation(currentMoveDirection) * Quaternion.LookRotation(relativeMoveDirection) * Vector3.forward;
					}
				}
			}
		}
		
		protected Vector3 moveDirection = Vector3.forward;
		protected Vector3 currentMoveDirection = Vector3.forward;

		protected float moveAngle = 0;
		protected float targetMoveAngle = 0;
		protected float moveAngleVelocity = 0;

		protected float mForwardThrottle = 0;
		protected float mSideThrottle = 0;
		protected float mBrake = 0;
		protected float mHandbrake = 1;
		protected bool mSpeedBoost = false;
		protected float mSpeedBoostFactor = 0;
		private float speedBoostFactorVelocity = 0;
		private float originalMaxAngularVelocity;
		private float originalOrbitCameraDistance;

		protected override void Awake () 
		{
			base.Awake();

			// Initialize current move direction and move angles.
			currentMoveDirection = transform.forward;

			moveAngle = Mathf.Atan2 (currentMoveDirection.x, currentMoveDirection.z) * Mathf.Rad2Deg;
			targetMoveAngle = moveAngle;

			// Save originals for lerping
			originalMaxAngularVelocity = maxAngularVelocity;
			originalOrbitCameraDistance = orbitCameraDistance;
		}

		protected virtual void Update () 
		{
			// Calculate target move angle based on intended move direction.
			targetMoveAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

			// Dampen the move angle toward target move angle with turnSmoothTime.
			moveAngle = Mathf.SmoothDampAngle(moveAngle, targetMoveAngle, ref moveAngleVelocity, turnSmoothTime);

			// Calculate the current move direction.
			currentMoveDirection = Quaternion.Euler(0, moveAngle, 0) * Vector3.forward;

			// Keep the top of the RollerBot up and facing the current move direction.
			if(top)
			{
				top.transform.rotation = Quaternion.LookRotation(currentMoveDirection);
			}

			// Adjust orbit camera distance based on speed boost factor
			orbitCameraDistance = Mathf.Lerp(originalOrbitCameraDistance, speedBoostOrbitCameraDistance, mSpeedBoostFactor);

			// Adjust speed boost particles
			if(mSpeedBoostFactor > 0)
			{
				Color boostEffectColor = Color.white;
				boostEffectColor.a = mSpeedBoostFactor;

				if(boostParticles)
				{
					if(!boostParticles.isPlaying) boostParticles.Play();
					boostParticles.startColor = boostEffectColor;
				}
			}
			else
			{
				if(boostParticles && boostParticles.isPlaying) boostParticles.Stop();
			}
			
		}
		
		protected virtual void FixedUpdate()
		{
			// Smooth out the speed boost force factor.
			mSpeedBoostFactor = Mathf.SmoothDamp(mSpeedBoostFactor, (mSpeedBoost ? 1f : 0f), ref speedBoostFactorVelocity, speedBoostSmoothTime);
			if(!mSpeedBoost && mSpeedBoostFactor < .001f) mSpeedBoostFactor = 0;

			// Adjust angular velocity based on speed boost
			maxAngularVelocity = Mathf.Lerp(originalMaxAngularVelocity, maxAngularVelocityWithBoost, mSpeedBoostFactor);

			// Calculate torque vectors based on currentMoveDirection
			Vector3 forwardTorqueVector = Quaternion.LookRotation(currentMoveDirection) * Vector3.right;
			Vector3 sideTorqueVector = Quaternion.LookRotation(currentMoveDirection) * -Vector3.forward;

			// Apply torque and forces to rigidbody.
			if(mRigidbody)
			{
				mRigidbody.maxAngularVelocity = Mathf.Max(maxAngularVelocity * Mathf.Clamp01(Mathf.Abs(mForwardThrottle) + Mathf.Abs(mSideThrottle)), minAngularVelocity) * mHandbrake;
				mRigidbody.AddTorque(forwardTorqueVector * maxTorque * mForwardThrottle);
				mRigidbody.AddTorque(sideTorqueVector * maxTorque * mSideThrottle);
				mRigidbody.AddForce(Physics.gravity * mRigidbody.mass * extraGravity);
			}
		}
	}
}
