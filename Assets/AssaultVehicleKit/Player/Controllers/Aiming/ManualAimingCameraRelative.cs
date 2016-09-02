using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace hebertsystems.AVK
{
	//  Manual Aiming (Camera Relative) Controller.
	//  Aims the turrets along the camera relative vector, controlled by player camera input,  
	//  along with primary and secondary firing inputs.
	//
	public class ManualAimingCameraRelative : AimingController 
	{
		public float inputThreshold = .2f;				// Threshold of player input before an aiming direction is determined.
		public float aimDistanceFromVehicle = 20;		// The distance to aim the turret from the vehicle's position.

		public Image reticle;							// The aiming reticle on screen (will be turned off for this aiming controller).

		private TurretInput turretInput;
		private float playerHorizontalInput = 0;
		private float playerVerticalInput = 1;
		private bool reticleActiveState = true;
		
		public override void Initialize(ref ControlReferences references)
		{
			// Obtain the aiming reticle's current active state, then turn off for this controller.
			if(reticle) 
			{
				reticleActiveState = reticle.IsActive();
				reticle.gameObject.SetActive(false);
			}
		}
		
		public override TurretInput UpdateAiming(ref ControlReferences references)
		{
			// Verify there is a vehicle and cemeraRigTransform before proceeding.
			if(!references.vehicle || !references.cameraRigTransform) return turretInput;

			// If player input is past the threshold, re-obtain input for aiming direction calculation.
			if(Mathf.Abs(PlayerInput.cameraHorizontal) > inputThreshold || Mathf.Abs(PlayerInput.cameraVertical) > inputThreshold)
			{
				playerHorizontalInput = PlayerInput.cameraHorizontal;
				playerVerticalInput = PlayerInput.cameraVertical;
			}

			// Camera RIGHT/LEFT relative input.
			Vector3 cameraRight = playerHorizontalInput * references.cameraRigTransform.right;
			
			// Camera FORWARD/BACK relative input
			// First calculate XY plane vectors for camera forward and up (will use up if camera is looking straight up or down).
			Vector3 cameraHorizontal = references.cameraRigTransform.forward;
			cameraHorizontal.y = 0;
			Vector3 cameraVertical = references.cameraRigTransform.up;
			cameraVertical.y = 0;
			// Final foward/back relative input
			Vector3 cameraForward = -playerVerticalInput * (cameraHorizontal == Vector3.zero ? cameraVertical.normalized : cameraHorizontal.normalized);

			// Obtain the final aim vector based on player input (average of left/right and forward/back input)
			Vector3 aimVector = (cameraForward + cameraRight)/2;

			// Turret Input
			//---------------------------------------
			// Set the turret aimpoint off of the vehicle's position and aimDistanceFromVehicle along the aim vector.
			turretInput.aimPoint = references.vehicle.position + aimVector.normalized * aimDistanceFromVehicle;
			
			// Set turret firing input.
			turretInput.primaryFire = PlayerInput.primaryFire;
			turretInput.secondaryFire = PlayerInput.secondaryFire;
			
			return turretInput;
		}
		
		public override void Exit(ref ControlReferences references)
		{
			// Return reticle to the previous active state.
			if(reticle) reticle.gameObject.SetActive(reticleActiveState);
		}
	}
}
