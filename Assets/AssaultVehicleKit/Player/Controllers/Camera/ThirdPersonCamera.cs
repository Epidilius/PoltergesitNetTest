using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Third Person Camera Controller.
	//  Non-rotating camera that follows the vehicle's position.
	//
	public class ThirdPersonCamera : CameraController 
	{
		public float initialHorizontalAngle = 180;				// The initial horizontal angle of the orbit camera relative to the vehicle (0 in front, 180 behind the vehicle)
		public float initialVerticalAngle = 45;					// The initial vertical angle of the orbit camera, positive values up.

		public float orbitCameraDistanceMultiplier = 5;			// The multiplier of the base orbit camera distance of the vehicle.

		public float cameraCollisionOffset = 1f;				// The collision offset for the camera, so the camera keeps a distance off walls and terrain when colliding.
		
		public LayerMask cameraCollisionLayerMask;				// The layer mask to use for camera collision;

		
		private float horizontalAngle;
		private float verticalAngle;
		private CameraInput cameraInput;


		public override void Initialize(ref ControlReferences references)
		{
			// Initialize horizontal and vertial angles.
			horizontalAngle = initialHorizontalAngle;
			verticalAngle = initialVerticalAngle;

			// Set camera type (for use elsewhere)
			references.currentCameraType = CameraType.Orbit;
		}
		
		public override CameraInput UpdateCamera(ref ControlReferences references)
		{
			// Obtain reference to vehicle and if not currently set, just return current input.
			Vehicle vehicle = references.vehicle;
			if(!vehicle) return cameraInput;

			// Obtain current vehicle pivot point.
			Vector3 pivot = vehicle.orbitCameraPivotBase + vehicle.orbitCameraPivotOffset;

			// Calculate camera position based on orbit angles and distance
			Vector3 targetPosition = pivot + Quaternion.Euler(-verticalAngle,horizontalAngle,0) * Vector3.forward * vehicle.orbitCameraDistance * orbitCameraDistanceMultiplier;

			Vector3 cameraVector = targetPosition - pivot;
			
			// Keep camera from hitting anything
			RaycastHit hit;
			if(Physics.Raycast(vehicle.orbitCameraPivotBase, cameraVector, out hit, cameraVector.magnitude + cameraCollisionOffset, cameraCollisionLayerMask))
			{
				targetPosition = hit.point - cameraVector.normalized * cameraCollisionOffset;
			}
			
			// Update final position and rotation for camera.
			cameraInput.position = targetPosition;
			cameraInput.rotation = Quaternion.LookRotation(-cameraVector);

			return cameraInput;
		}

		public override void Exit(ref ControlReferences references)
		{
		}
	}
}
