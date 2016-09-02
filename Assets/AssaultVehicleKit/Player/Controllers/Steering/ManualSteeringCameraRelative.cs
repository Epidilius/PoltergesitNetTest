using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Manual Steering (Camera Relative) Controller.
	//  Steers the vehicle using player manual steering input,
	//  along with throttle and braking.
	//  Steering input is provided relative to the camera's view.
	//
	public class ManualSteeringCameraRelative : SteeringController 
	{		
		private VehicleInput vehicleInput;
		
		public override void Initialize(ref ControlReferences references)
		{
		}
		
		public override VehicleInput UpdateSteering(ref ControlReferences references)
		{
			// Verify there is a cemeraRigTransform before proceeding.
			if(!references.cameraRigTransform) return vehicleInput;

			// Camera RIGHT/LEFT relative input.
			Vector3 cameraRight = PlayerInput.vehicleHorizontal * references.cameraRigTransform.right;

			// Camera FORWARD/BACK relative input
			// First calculate XY plane vectors for camera forward and up (will use up if camera is looking straight up or down).
			Vector3 cameraHorizontal = references.cameraRigTransform.forward;
			cameraHorizontal.y = 0;
			Vector3 cameraVertical = references.cameraRigTransform.up;
			cameraVertical.y = 0;
			// Final foward/back relative input
			Vector3 cameraForward = PlayerInput.vehicleVertical * (cameraHorizontal == Vector3.zero ? cameraVertical.normalized : cameraHorizontal.normalized);


			// Vehicle Input
			//---------------------------------------
			vehicleInput.moveDirection = (cameraForward + cameraRight)/2;

			// Throttle is the max of vehicle horizontal or vertical input (always forward in this mode currently).
			vehicleInput.forwardThrottle = Mathf.Max(Mathf.Abs(PlayerInput.vehicleHorizontal), Mathf.Abs(PlayerInput.vehicleVertical));

			vehicleInput.handBrake = PlayerInput.handBrake;
			vehicleInput.speedBoost = PlayerInput.speedBoost;
			
			return vehicleInput;
		}
		
		public override void Exit(ref ControlReferences references)
		{
		}
	}
}
