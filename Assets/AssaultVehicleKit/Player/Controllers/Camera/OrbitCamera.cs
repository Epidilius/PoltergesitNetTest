using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Orbit Camera Controller (World Up).
	//  Orbits the camera around the pivot point of the vehicle using the world up (+Y) axis,
	//  keeping the camera orientation level at all times, regardless of what the vehicle is doing.
	//
	public class OrbitCamera : CameraController 
	{
		public float initialHorizontalAngle = 180;				// The initial horizontal angle of the orbit camera relative to the vehicle (0 in front, 180 behind the vehicle)
		public float initialVerticalAngle = 10;					// The initial vertical angle of the orbit camera, positive values up.

		public Vector3 offset = Vector3.zero;					// Offset of the pivot away from the vehicle's pivot point.

		public float orbitCameraDistanceMultiplier = 1;			// The multiplier of the base orbit camera distance of the vehicle.

		public float cameraCollisionOffset = 1f;				// The collision offset for the camera, so the camera keeps a distance off walls and terrain when colliding.

		public LayerMask cameraCollisionLayerMask;				// The layer mask to use for camera collision;


		private float horizontalAngle;
		private float verticalAngle;
		private CameraInput cameraInput;


		public override void Initialize(ref ControlReferences references)
		{
			// Obtain reference to vehicle and if not currently set, just return.
			Vehicle vehicle = references.vehicle;
			if(!vehicle) return;

			// Initialize the horizontal and vertical angles of the orbit camera based on the current vehicle forward direction.
			Vector3 vehicleForward = vehicle.forward;
			float vehicleAngle = Mathf.Atan2(vehicleForward.x, vehicleForward.z) * Mathf.Rad2Deg;
			horizontalAngle = initialHorizontalAngle + vehicleAngle;
			verticalAngle = initialVerticalAngle;

			// Set camera type (for use elsewhere)
			references.currentCameraType = CameraType.Orbit;
		}
		
		public override CameraInput UpdateCamera(ref ControlReferences references)
		{
			// Obtain reference to vehicle and if not currently set, just return current input.
			Vehicle vehicle = references.vehicle;
			if(!vehicle) return cameraInput;
			
			// Update camera orbit angles based on player input.
			horizontalAngle += PlayerInput.cameraHorizontal;
			horizontalAngle %= 360;
			verticalAngle = Mathf.Clamp(verticalAngle + PlayerInput.cameraVertical, vehicle.orbitCameraMinVerticalAngle, vehicle.orbitCameraMaxVerticalAngle);

			// Get adjusted pivot point based on offset and rotation.
			Vector3 pivot = vehicle.orbitCameraPivotBase + vehicle.orbitCameraPivotOffset + Quaternion.Euler(0, horizontalAngle, 0) * offset;
			
			// Calculate camera position based on orbit angles and distance
			Vector3 targetPosition = pivot + Quaternion.Euler(-verticalAngle,horizontalAngle,0) * Vector3.forward * vehicle.orbitCameraDistance * orbitCameraDistanceMultiplier;

			// Modify camera position if it goes below vehicle's orbit camera floor (cap it at the floor bottom).
			Vector3 cameraVector = targetPosition - pivot;
			if(cameraVector.y < vehicle.orbitCameraFloor)
			{
				float ratio = vehicle.orbitCameraFloor/cameraVector.y;
				targetPosition = pivot + cameraVector * ratio;
				cameraVector = targetPosition - pivot;
			}
			
			// Keep camera from hitting anything
			RaycastHit hit;
			if(Physics.Raycast(pivot, cameraVector, out hit, cameraVector.magnitude + cameraCollisionOffset, cameraCollisionLayerMask))
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
