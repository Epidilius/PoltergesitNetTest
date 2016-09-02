using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Orbit Camera Controller (Vehicle Up)
	//  Orbits the camera around the pivot point of the vehicle using the vehicle's relative up axis.
	//
	public class OrbitCameraVehicleUp : CameraController 
	{
		public float initialHorizontalAngle = 180;				// The initial horizontal angle of the orbit camera relative to the vehicle (0 in front, 180 behind the vehicle)
		public float initialVerticalAngle = 10;					// The initial vertical angle of the orbit camera, positive values up.
		
		public Vector3 offset = Vector3.zero;					// Offset of the pivot away from the vehicle's pivot point.
		
		public float orbitCameraDistanceMultiplier = 1;			// The multiplier of the base orbit camera distance of the vehicle.
		
		public float cameraCollisionOffset = 1f;				// The collision offset for the camera, so the camera keeps a distance off walls and terrain when colliding.

		public float upSmoothTime = .5f;						// The smooth time applied to the up follow vector (smooths out bumps).

		public LayerMask cameraCollisionLayerMask;				// The layer mask to use for camera collision;

		
		private float horizontalAngle;
		private float verticalAngle;
		private Vector3 vehicleUp;
		private Quaternion lastRotation;
		private Vector3 lastUp;
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
			
			// Update camera orbit angles based on player input.
			horizontalAngle += PlayerInput.cameraHorizontal;
			horizontalAngle %= 360;
			verticalAngle = Mathf.Clamp(verticalAngle + PlayerInput.cameraVertical, vehicle.orbitCameraMinVerticalAngle, vehicle.orbitCameraMaxVerticalAngle);

			// Smoothly update vehicle up vector follower.
			vehicleUp = upSmoothTime > 0 ? Vector3.Slerp(vehicleUp, vehicle.up, Time.deltaTime/upSmoothTime) : vehicle.up;

			// Calculate vehicle's up rotation (based on smoothed vehicleUp and the last frames settings - incremental updates.
			Quaternion vehicleUpRotation = FromToRotation(lastUp, vehicleUp) * lastRotation;
			lastRotation = vehicleUpRotation;
			lastUp = vehicleUp;

			// Obtain current vehicle pivot point.
			Vector3 pivot = vehicle.orbitCameraPivotBase + vehicleUpRotation * vehicle.orbitCameraPivotOffset + vehicleUpRotation * Quaternion.Euler(0, horizontalAngle, 0) * offset;

			// Calculate camera position based on orbit angles and distance
			Vector3 targetPosition = pivot + Quaternion.Euler(-verticalAngle,horizontalAngle,0) * Vector3.forward * vehicle.orbitCameraDistance * orbitCameraDistanceMultiplier;
			
			// Modify camera position if it goes below vehicle's camera floor (cap it at the floor bottom).
			Vector3 cameraVector = targetPosition - pivot;
			if(cameraVector.y < vehicle.orbitCameraFloor)
			{
				float ratio = vehicle.orbitCameraFloor/cameraVector.y;
				targetPosition = pivot + cameraVector * ratio;
				cameraVector = targetPosition - pivot;
			}

			// Update targetPosition based on the vehicle's Up rotation.
			targetPosition = pivot + vehicleUpRotation * cameraVector;
			
			// Keep camera from hitting anything
			RaycastHit hit;
			Vector3 targetVector = targetPosition - pivot;
			if(Physics.Raycast(pivot, targetVector, out hit, targetVector.magnitude + cameraCollisionOffset, cameraCollisionLayerMask))
			{
				targetPosition = hit.point - targetVector.normalized * cameraCollisionOffset;
			}
			
			// Update final position and rotation for camera.
			cameraInput.position = targetPosition;
			cameraInput.rotation = vehicleUpRotation * Quaternion.LookRotation(-cameraVector);
			
			return cameraInput;
		}
		
		public override void Exit(ref ControlReferences references)
		{
		}
	}
}
