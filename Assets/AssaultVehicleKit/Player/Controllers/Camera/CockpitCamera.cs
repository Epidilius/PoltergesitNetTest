using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Cockpit Camera Controller.
	//  Represents a camera located in the driver's seat of the vehicle looking out.
	//
	public class CockpitCamera : CameraController 
	{
		public float cockpitRotationSmoothTime = .5f;					// Smooth time applied to camera's base rotation to keep up with vehicle reference cockpit rotation.
		public bool allowCameraRotation = true;							// Flag to indicate the player can manually rotate the camera within the vehicle or not.


		private float horizontalAngle;
		private float verticalAngle;
		private Quaternion cockpitRotation = Quaternion.identity;
		private CameraInput cameraInput;


		public override void Initialize(ref ControlReferences references)
		{
			// Initialize camera rotation angles.
			horizontalAngle = 0;
			verticalAngle = 0;
			
			// Initialize the base cockpit transform from the vehicle if specified.
			if(references.vehicle && references.vehicle.cockpitTransorm)
				cockpitRotation = references.vehicle.cockpitTransorm.rotation;
			else Debug.LogWarning("No Cockpit Transform specified with vehicle for CockpitCamera on " + name);

			// Set camera type (for use elsewhere)
			references.currentCameraType = CameraType.Cockpit;
		}
		
		public override CameraInput UpdateCamera(ref ControlReferences references)
		{
			// Obtain reference to vehicle and if not currently set, just return current input.
			Vehicle vehicle = references.vehicle;
			if(!vehicle || !vehicle.cockpitTransorm) return cameraInput;
			
			// Update camera orbit angles based on player input and within min/max ranges.
			if(allowCameraRotation)
			{
				horizontalAngle = Mathf.Clamp(horizontalAngle + PlayerInput.cameraHorizontal, vehicle.cockpitCameraMinHorizontalAngle, vehicle.cockpitCameraMaxHorizontalAngle);
				verticalAngle = Mathf.Clamp(verticalAngle + PlayerInput.cameraVertical, vehicle.orbitCameraMinVerticalAngle, vehicle.orbitCameraMaxVerticalAngle);
			}

			// Update final position for camera.
			cameraInput.position = vehicle.cockpitTransorm.position;

			// Smoothly follow vehicle base cockpit rotation
			cockpitRotation = cockpitRotationSmoothTime > 0 ? Quaternion.Slerp(cockpitRotation, vehicle.cockpitTransorm.rotation, Time.deltaTime / cockpitRotationSmoothTime) : vehicle.cockpitTransorm.rotation;
			// Update final rotation for camera base on cockpit base rotation and player input.
			cameraInput.rotation = cockpitRotation * Quaternion.Euler(verticalAngle,horizontalAngle,0);

			return cameraInput;
		}

		public override void Exit(ref ControlReferences references)
		{
		}
	}
}
