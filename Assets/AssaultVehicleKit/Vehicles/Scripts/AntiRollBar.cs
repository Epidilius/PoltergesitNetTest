using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  This behavior provides anti-roll forces between two Wheels depending on the
	//  speed of the wheels on ground and the relative compression between them.
	//  The anti-roll force is also affected by the slope of the ground the wheels are on - 
	//  max force is applied on flat ground and will decrease as the ground slopes towards vertical.
	//
	public class AntiRollBar : MonoBehaviour 
	{
		public Wheel leftWheel;								// The left wheel of the axle
		public Wheel rightWheel;							// The right wheel

		public float maxForce = 1f;							// The max force to apply at each wheel
		public float maxForceAtSpeed = 30f;					// Wheel speed on ground (averaged between both wheels) where max force is applied

		protected Rigidbody mRigidbody;

		void Awake () 
		{
			if(!leftWheel) Debug.LogWarning("No leftWheel specified for AntiRollBar on " + name);
			if(!rightWheel) Debug.LogWarning("No rightWheel specified for AntiRollBar on " + name);

			// Obtain reference to the Rigidbody.
			mRigidbody = GetComponentInChildren<Rigidbody>();
		}

		void FixedUpdate () 
		{
			// If we don't have both wheels and a rigidbody, do nothing.
			if(!leftWheel || !rightWheel || !mRigidbody) return;

			// If both wheels are on the ground, apply anti-roll force
			if(leftWheel.isGrounded && rightWheel.isGrounded)
			{
				// Determin the slope factor of anti-roll force based on ground normal of both wheels
				float slopeFactor = Vector3.Dot(leftWheel.groundNormal,Vector3.up) * Vector3.Dot(rightWheel.groundNormal,Vector3.up);

				// Obtain compression values for both wheels
				float leftCompression = leftWheel.compression;
				float rightCompression = rightWheel.compression;

				// Calculate the ratio of the max force based on the speed of the wheels on the ground and maxForceAtSpeed
				float speedFactor = Mathf.InverseLerp(0, maxForceAtSpeed, Mathf.Abs((leftWheel.wheelSpeedOnGround + rightWheel.wheelSpeedOnGround)/2));

				// Calculate force based on difference between wheel compression along with the slope and speed factors.
				float force = (rightCompression - leftCompression) * slopeFactor * speedFactor * maxForce;

				// Apply anti-roll force to wheel with least compression
				if(rightCompression > leftCompression) mRigidbody.AddForceAtPosition(-leftWheel.wheelCollider.transform.up * force, leftWheel.wheelCollider.transform.position);
				else mRigidbody.AddForceAtPosition(rightWheel.wheelCollider.transform.up * force, rightWheel.wheelCollider.transform.position);
			}
		}
	}
}
