using UnityEngine;

namespace hebertsystems.AVK
{
	//  The base class for all vehicles.  Provides an abstraction layer
	//  between the vehicles and player input allowing for a
	//  modular system where vehicles can be swapped in and out with ease.
	//
	public abstract class Vehicle : MonoBehaviour
	{
		// The following are parameters supplied uniquely for each
		// vehicle for use by the camera system of the player driver.

		// Orbit Camera parameters
		[Header("Orbit Camera Parameters")]
		public float orbitCameraMinVerticalAngle = -25;					// The minimum vertical angle around the pivot point (should be negative, camera below the pivot point).
		public float orbitCameraMaxVerticalAngle = 45;					// The maximum vertical angle around the pivot point (should be positive, camera above the pivot point).
		public float orbitCameraFloor = -1;								// The floor of the orbiting camera (below the pivot point) that the orbit camera cannot go below.
		public float orbitCameraDistance = 7;							// The distance of the orbiting camera from the pivot point.
		public abstract Vector3 orbitCameraPivotBase {get;}				// The pivot base for orbiting cameras.
		public abstract Vector3 orbitCameraPivotOffset {get;}			// The offset from the pivot base for orbit camera pivot point.  Can be used for world offset or local offset when multiplied by vehicle rotation.

		// Cockpit Camera parameters
		[Header("Cockpit Camera Parameter")]
		public float cockpitCameraMinVerticalAngle = -45;				// The minimum vertical angle for the cockpit camera.
		public float cockpitCameraMaxVerticalAngle = 45;				// The maximum vertical angle for the cockpit camera.
		public float cockpitCameraMinHorizontalAngle = -90;				// The minimum horizontal angle for the cockpit camera.
		public float cockpitCameraMaxHorizontalAngle = 90;				// The maximum horizontal angle for the cockpit camera.
		public Transform cockpitTransorm;								// The cockpit transform for the vehicle.

		public abstract Vector3 forward 	{get;}						// The forward direction of the vehicle.
		public abstract Vector3 up 			{get;}						// The up direction of the vehicle
		public abstract Vector3 right		{get;}						// The right direction of the vehicle
		public abstract Vector3 position 	{get;}						// The postion of the vehicle.
		public abstract Quaternion rotation {get;}						// The rotation of the vehicle.

		public abstract VehicleInput input {set;}						// The vehicle input supplied to the vehicle (throttle, move direction, etc.)

		protected Rigidbody mRigidbody;

		protected virtual void Awake () 
		{
			// Obtain reference to the Rigidbody.
			mRigidbody = GetComponentInChildren<Rigidbody>();
		}
	}
}

