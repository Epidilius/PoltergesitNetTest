using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Camera Aiming Controller.
	//  Aims the turrets at the target obtained along the camera's forward vector,
	//  along with primary and secondary firing inputs.
	//
	public class CameraAiming : AimingController 
	{		
		public float maxTargetingDistance = 200;				// The max distance to cast a ray out of the camera to find a target.
		public LayerMask aimLayerMask;							// The layer mask to use while aiming for targets to shoot.


		private TurretInput turretInput;
		
		public override void Initialize(ref ControlReferences references)
		{
		}
		
		public override TurretInput UpdateAiming(ref ControlReferences references)
		{
			if(references.driverCamera)
			{
				// Store reference to the driver's camera transform.
				Transform cameraTransform = references.driverCamera.transform;

				RaycastHit hit;
				// Aim out the front of the camera to maxTargetingDistance.
				// Set turrent input aimpoint to anything hit or at max distance along camera forward vector if nothing hit.
				if(Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxTargetingDistance, aimLayerMask))
				{
					turretInput.aimPoint = hit.point;
				}
				else
				{
					turretInput.aimPoint =  cameraTransform.position + cameraTransform.forward * maxTargetingDistance;
				}
			}
			
			// Set turret firing input.
			turretInput.primaryFire = PlayerInput.primaryFire;
			turretInput.secondaryFire = PlayerInput.secondaryFire;
			
			return turretInput;
		}
		
		public override void Exit(ref ControlReferences references)
		{
		}
	}
}
