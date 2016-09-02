using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Follow Camera Controller.
	//  Camera that follows the vehicle, orbiting about the world up axis (+Y).
	//
	public class FollowCamera : CameraController 
	{
		public float targetHorizontalAngle = 180;				// The initial horizontal angle of the orbit camera relative to the vehicle (0 in front, 180 behind the vehicle)
		public float targetVerticalAngle = 10;					// The initial vertical angle of the orbit camera, positive values up.

		public float horizontalSmoothTime = .2f;				// Smooth time applied to horizontal orbit angle.
		public float verticalSmoothTime = 1;					// Smooth time applied to vertical orbit angle.

		public bool followVertical = true;						// Whether or not to follow the vertical angle of the vehicle.

		public float orbitCameraDistanceMultiplier = 1;			// The multiplier of the base orbit camera distance of the vehicle.
		
		public float cameraCollisionOffset = 1f;				// The collision offset for the camera, so the camera keeps a distance off walls and terrain when colliding.
		
		public LayerMask cameraCollisionLayerMask;				// The layer mask to use for camera collision;

		
		private float horizontalAngle;
		private float horizontalAngleVelocity = 0;
		private float verticalAngle;
		private float verticalAngleVelocity = 0;
		private CameraInput cameraInput;

		
		public override void Initialize(ref ControlReferences references)
		{
			// Obtain reference to vehicle and if not currently set, just return.
			Vehicle vehicle = references.vehicle;
			if(!vehicle) return;

			// Calculate vehicle's current angles in world space (based on forward vector).
			Vector3 vehicleForward = vehicle.forward;
			float vehicleHorizontalAngle = Mathf.Atan2(vehicleForward.x, vehicleForward.z) * Mathf.Rad2Deg;
			float vehicleVerticalAngle = Mathf.Asin(-vehicleForward.y) * Mathf.Rad2Deg;

			// Calculate initial angles based on vehicle and target angles.
			horizontalAngle = vehicleHorizontalAngle + targetHorizontalAngle;
			if(followVertical) verticalAngle = vehicleVerticalAngle + targetVerticalAngle;
			else verticalAngle = targetVerticalAngle;

			// Set camera type (for use elsewhere)
			references.currentCameraType = CameraType.Orbit;
		}
		
		public override CameraInput UpdateCamera(ref ControlReferences references)
		{
			// Obtain reference to vehicle and if not currently set, just return current input.
			Vehicle vehicle = references.vehicle;
			if(!vehicle) return cameraInput;

			// Calculate vehicle's current angles in world space
			Vector3 vehicleForward = vehicle.forward;
			float vehicleHorizontalAngle = Mathf.Atan2(vehicleForward.x, vehicleForward.z) * Mathf.Rad2Deg;
			float vehicleVerticalAngle = Mathf.Asin(-vehicleForward.y) * Mathf.Rad2Deg;

			// Update camera follow angles smoothly towards target angles.
			horizontalAngle = Mathf.SmoothDampAngle(horizontalAngle, vehicleHorizontalAngle + targetHorizontalAngle, ref horizontalAngleVelocity, horizontalSmoothTime);
			if(followVertical) verticalAngle = Mathf.SmoothDamp(verticalAngle, vehicleVerticalAngle + targetVerticalAngle, ref verticalAngleVelocity, verticalSmoothTime);
			else verticalAngle = Mathf.SmoothDamp(verticalAngle, targetVerticalAngle, ref verticalAngleVelocity, verticalSmoothTime);

			// Obtain current vehicle pivot point.
			Vector3 pivot = vehicle.orbitCameraPivotBase + vehicle.orbitCameraPivotOffset;

			// Calculate camera position based on pivot position and orbit angles and distance
			Vector3 targetPosition = pivot + Quaternion.Euler(-verticalAngle,horizontalAngle,0) * Vector3.forward * vehicle.orbitCameraDistance * orbitCameraDistanceMultiplier;

			Vector3 cameraVector = targetPosition - pivot;
			
			// Keep camera from hitting anything
			RaycastHit hit;
			if(Physics.Raycast(pivot, cameraVector, out hit, cameraVector.magnitude + cameraCollisionOffset, cameraCollisionLayerMask))
			{
				targetPosition = hit.point - cameraVector.normalized * cameraCollisionOffset;
			}
			
			// Update final position of camera.
			cameraInput.position = targetPosition;
			// Update rotation, which is just a look at the pivot point rotation.
			cameraInput.rotation = Quaternion.LookRotation(-cameraVector, Vector3.up);

			// If looking straight up or down, the "up" axis for LookRotation will be based on the horizontal angle.
			Vector3 up = Mathf.Abs(verticalAngle) != 90 ? Vector3.up : Quaternion.Euler(0,horizontalAngle,0)*-Vector3.forward;
			// Calculate rotation (look at rotation).
			cameraInput.rotation = Quaternion.LookRotation(-cameraVector, up);

			return cameraInput;
		}

		public override void Exit(ref ControlReferences references)
		{
		}
	}
}
