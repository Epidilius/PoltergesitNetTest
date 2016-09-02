using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  The Wheel behavior controls the visual representation of a wheel
	//  based on a WheelCollider activity.  It also obtains useful information
	//  about the WheelCollider as it opperates.
	//  It is expected to place this behavior on the wheel mesh representation
	//  directly as the Wheel controls the local transform to simulate
	//  wheel movement.
	//
	public class Wheel : MonoBehaviour 
	{
		public WheelCollider wheelCollider;														// The WheelCollider to gather info about and visually represent.

		public bool steering = false;															// Indicator that this wheel accepts steering input.
		public bool power = false;																// Indicator that this wheel is used for power input.
		public bool braking = false;															// Indicator that this wheel is used for braking.
		public bool handBraking = false;														// Indicator that this wheel is used for handbraking.

		public bool isGrounded				{get {UpdateState(); return mIsGrounded;}}			// Whether the WheelCollider is grounded.
		public Vector3 groundNormal			{get {UpdateState(); return mGroundNormal;}}		// The normal at the point of impact with the ground.
		public float compression			{get {UpdateState(); return mCompression;}}			// The amount of compression of the suspension (1 is fully compressed).
		public float compressionRate		{get {UpdateState(); return mCompressionRate;}}		// The rate of the compression per frame (not smoothed currently).
		public float force					{get {UpdateState(); return mForce;}}				// The force at the point of impact with the ground.
		public float forceRate				{get {UpdateState(); return mForceRate;}}			// The rate of the force per frame.
		public float wheelSpeedOnGround		{get {UpdateState(); return mWheelSpeedOnGround;}}	// The wheel speed on the ground based on wheel size and rpm.
		public float forwardSlip			{get {UpdateState(); return mForwardSlip;}}			// The forward slip of the wheel on the ground.
		public float sidewaysSlip			{get {UpdateState(); return mSidewaysSlip;}}		// The side slip of the wheel on the ground.
		public float rpm																		// The rpm of the wheel.
		{
			get
			{
				if(wheelCollider) return wheelCollider.rpm;
				return 0;
			}
		}
		public float steerAngle																	// The steer angle of the wheel.
		{
			set
			{
				if(wheelCollider)
				{
					wheelCollider.steerAngle = value * steerAngleFactor;
				}
			}
		}
		public float steerAngleFactor = 1;														// The factor to apply to the steerAngle (negative values turn in opposite direction).
		public float motorTorque																// motoTorque to apply to wheel.
		{
			set
			{
				if(wheelCollider)
				{
					wheelCollider.motorTorque = value;
				}
			}
		}
		public float brakeTorque																// brakeTorque to apply to wheel.
		{
			set
			{
				if(wheelCollider)
				{
					wheelCollider.brakeTorque = value;
				}
			}
		}
		public bool freshContact		{get {return mFreshContact;} }							// Whether the wheel has made fresh contact with the ground this frame.

		private float mWheelRotationAngle = 0f;
		private Vector3 mOriginalLocalPosition = Vector3.zero;
		private Vector3 mLocalSuspensionDelta = Vector3.zero;
		private int mLastFrame = -1;
		private WheelHit mHit;
		private bool mIsGrounded;
		private bool mLastIsGrounded = false;
		private Vector3 mGroundNormal = Vector3.up;
		private float mCompression = 0;
		private float mLastCompression = 0;
		private float mCompressionRate = 0;
		private float mForce = 0;
		private float mLastForce = 0;
		private float mForceRate =0;
		private float mWheelSpeedOnGround = 0;
		private bool mFreshContact = false;
		private float mForwardSlip = 0;
		private float mSidewaysSlip = 0;

		private readonly float MeterPerSecondToMPH = 2.2369356f;


		void Awake () 
		{
			if(!wheelCollider) Debug.LogWarning("No WheelCollider specified for Wheel on " + name);

			// Save original local position, assuming the wheel mesh is originally in the fully compressed position.
			mOriginalLocalPosition = transform.localPosition;
		}

		void Update()
		{
			// Update Wheel mesh every frame:

			// Set local position of wheel mesh based on suspensionDelta
			transform.localPosition = mOriginalLocalPosition + Vector3.up * mLocalSuspensionDelta.y;

			// Calculate rotation of wheel mesh based on current rpm
			mWheelRotationAngle += ((wheelCollider.rpm/60) * 360 * Time.deltaTime);
			mWheelRotationAngle %= 360;

			// Rotate wheel mesh and turn according to steerAngle
			transform.localRotation = Quaternion.Euler(mWheelRotationAngle, wheelCollider.steerAngle, 0);
		}

		void FixedUpdate()
		{
			// Update State every fixedUpdate
			UpdateState();
		}

		void UpdateState()
		{
			// Only udpate state once a frame.
			if(wheelCollider && mLastFrame != Time.frameCount)
			{
				// Handle any potential scaling of the vehicle model by using final scale at the wheel
				float wheelScale = wheelCollider.transform.lossyScale.y;

				// Calculate wheel position and suspension compression based on WheelHit
				if(wheelCollider.GetGroundHit(out mHit))
				{
					mIsGrounded = true;
					mForce = mHit.force;
					mForwardSlip = mHit.forwardSlip;
					mSidewaysSlip = mHit.sidewaysSlip;
					mGroundNormal = mHit.normal;

					mLocalSuspensionDelta = wheelCollider.transform.InverseTransformPoint( mHit.point + wheelCollider.transform.up * wheelCollider.radius * wheelScale);
					// Clamp localSuspensionDelta.y to appropriate values (Physics seems to overshoot at times)
					mLocalSuspensionDelta.y = Mathf.Clamp(mLocalSuspensionDelta.y, -wheelCollider.suspensionDistance, 0);

					mCompression = Mathf.InverseLerp(wheelCollider.suspensionDistance, 0, -mLocalSuspensionDelta.y);
				}
				// No WheelHit, calculate fully extended wheel
				else
				{
					mIsGrounded = false;
					mForce = 0;
					mForwardSlip = 0;
					mSidewaysSlip = 0;

					mLocalSuspensionDelta = wheelCollider.transform.InverseTransformPoint( wheelCollider.transform.position - wheelCollider.transform.up * wheelCollider.suspensionDistance * wheelScale);

					mCompression = 0;
				}

				// Calculate compression rate.
				mCompressionRate = (mCompression - mLastCompression)/Time.deltaTime;
				mLastCompression = mCompression;

				// Calculate force rate.
				mForceRate = (mForce - mLastForce)/Time.deltaTime;
				mLastForce = mForce;

				// Determine if this is fresh contact with the ground.
				if(!mLastIsGrounded && mIsGrounded)
				{
					mFreshContact = true;
				}
				else mFreshContact = false;
				mLastIsGrounded = mIsGrounded;

				// Calculate speed on the ground
				mWheelSpeedOnGround = (wheelCollider.rpm/60) * 2 * Mathf.PI * wheelCollider.radius * wheelScale * MeterPerSecondToMPH;

				// Save frameCount so we don't recalculate anything if called again within same frame.
				mLastFrame = Time.frameCount;
			}
		}
	}
}
