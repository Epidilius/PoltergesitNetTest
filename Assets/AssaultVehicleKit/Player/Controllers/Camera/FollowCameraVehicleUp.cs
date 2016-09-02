using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Follow Camera Controller (Vehicle Up).
	//  This follow camera will follow the vehicle, orbiting about the vehicle's relative up axis.
	//
	public class FollowCameraVehicleUp : CameraController 
	{
		public float targetHorizontalAngle = 180;				// The initial horizontal angle of the orbit camera relative to the vehicle (0 in front, 180 behind the vehicle)
		public float targetVerticalAngle = 10;					// The initial vertical angle of the orbit camera, positive values up.

		public float fowardSmoothTime = .2f;					// The smooth time applied to forward follow vector.

		public float upSmoothTime = .75f;						// The smooth time applied to the up follow vector (smooths out bumps).

		public float orbitCameraDistanceMultiplier = 1;			// The multiplier of the base orbit camera distance of the vehicle.
		
		public float cameraCollisionOffset = 1f;				// The collision offset for the camera, so the camera keeps a distance off walls and terrain when colliding.
		
		public LayerMask cameraCollisionLayerMask;				// The layer mask to use for camera collision;
		

		private Quaternion lastRotation;
		private Vector3 lastUp;
		private Vector3 targetFollowVector;
		private Vector3 followVector;
		private Vector3 vehicleForward;
		private Vector3 vehicleUp;
		private CameraInput cameraInput;


		public override void Initialize(ref ControlReferences references)
		{
			// Obtain reference to vehicle and if not currently set, just return.
			Vehicle vehicle = references.vehicle;
			if(!vehicle) return;

			// Calculate the target follow vector (based on target angles)
			targetFollowVector = Quaternion.Euler(-targetVerticalAngle, targetHorizontalAngle, 0) * Vector3.forward;
			// Calculate initial follow vector based on target and vehicle's forward.
			followVector = Quaternion.LookRotation(vehicle.forward, vehicle.up) * targetFollowVector;

			// Init up vectors and last frame rotation to defaults.
			vehicleUp = Vector3.up;
			lastRotation = Quaternion.identity;
			lastUp = Vector3.up;

			// Set camera type (for use elsewhere)
			references.currentCameraType = CameraType.Orbit;
		}

		// Similar to Quaternion.FromToRotation, but seems to be smoother in spots.
		// Calculates the rotation needed to bring the from axis to the to axis.
		private Quaternion FromToRotation(Vector3 from, Vector3 to)
		{
			from.Normalize();
			to.Normalize();
			
			if(from == to) return Quaternion.identity;
			if(from == -to)
			{
				Vector3 tangent = Vector3.Cross(from, Vector3.forward);
				if(tangent.sqrMagnitude == 0) tangent = Vector3.Cross(from, Vector3.up);
				
				return Quaternion.AngleAxis(180, tangent);
			}
			
			return Quaternion.AngleAxis(Vector3.Angle(from, to), Vector3.Cross(from, to));
		}
		
		public override CameraInput UpdateCamera(ref ControlReferences references)
		{
			// Obtain reference to vehicle and if not currently set, just return current input.
			Vehicle vehicle = references.vehicle;
			if(!vehicle) return cameraInput;

			// Smoothly update vehicle forward and up vector followers.
			vehicleForward = fowardSmoothTime > 0 ? Vector3.Slerp(vehicleForward, vehicle.forward, Time.deltaTime/fowardSmoothTime) : vehicle.forward;
			vehicleUp = upSmoothTime > 0 ? Vector3.Slerp(vehicleUp, vehicle.up, Time.deltaTime/upSmoothTime) : vehicle.up;

			// Update the followVector based on smoothed vehicle forward and up follow vectors.
			followVector = Quaternion.LookRotation(vehicleForward, vehicleUp) * targetFollowVector * vehicle.orbitCameraDistance * orbitCameraDistanceMultiplier;

			// Calculate vehicle's up rotation (based on smoothed vehicleUp and the last frames settings - incremental updates.
			Quaternion vehicleUpRotation = FromToRotation(lastUp, vehicleUp) * lastRotation;
			lastRotation = vehicleUpRotation;
			lastUp = vehicleUp;

			// Obtain current vehicle pivot point.
			Vector3 pivot = vehicle.orbitCameraPivotBase + vehicleUpRotation * vehicle.orbitCameraPivotOffset;

			// Calculate camera position based on orbit angles and distance
			Vector3 targetPosition = pivot + followVector;

			Vector3 cameraVector = targetPosition - pivot;

			// Keep camera from hitting anything
			RaycastHit hit;
			if(Physics.Raycast(pivot, cameraVector, out hit, cameraVector.magnitude + cameraCollisionOffset, cameraCollisionLayerMask))
			{
				targetPosition = hit.point - cameraVector.normalized * cameraCollisionOffset;
			}
			
			// Update final position and rotation for camera.
			cameraInput.position = targetPosition;
			cameraInput.rotation = Quaternion.LookRotation(-cameraVector, vehicleUp);
			
			return cameraInput;
		}

		public override void Exit(ref ControlReferences references)
		{
		}
	}
}
