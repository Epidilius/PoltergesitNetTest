using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace hebertsystems.AVK
{
	//  Manual Aiming Controller.
	//  Aims the turrets along the vector of the reticle controlled by player input,
	//  along with primary and secondary firing inputs.
	//
	public class ManualAimingScreenCursor : AimingController 
	{		
		public float maxTargetingDistance = 200;				// The max distance to cast a ray out of the camera to find a target.
		public Image reticle;									// The aiming reticle on screen.

		public float targetMoveSpeed = 1;						// The speed at which the player moves the reticle for aiming.
		
		public LayerMask aimLayerMask;							// The layer mask to use while aiming for targets to shoot.
		
		
		private TurretInput turretInput;
		private Vector2 targetPos = Vector2.zero;
		private Vector2 reticleOriginalPos;
		private float screenWidth;
		private float screenHeight;


		void Awake()
		{
			if(!reticle) Debug.LogWarning("No reticle specified for ManualAiming controller on " + name);
		}
		
		public override void Initialize(ref ControlReferences references)
		{
			// If a reticle specified, obtain it's initial position in screen space.
			if(reticle) reticleOriginalPos = reticle.rectTransform.anchoredPosition;

			// Save screen width and height (in pixels) of the driver's camera.
			if(references.driverCamera)
			{
				// Obtain screen width and height to set initial reticle position.
				screenWidth = references.driverCamera.pixelWidth;
				screenHeight = references.driverCamera.pixelHeight;

				// Set initial position of reticle to the middle of the screen.
				targetPos = new Vector2(screenWidth/2f, screenHeight/2f);
			}
		}
		
		public override TurretInput UpdateAiming(ref ControlReferences references)
		{
			if(references.driverCamera)
			{
				// Get reference to driver's camera.
				Camera driverCamera = references.driverCamera;

				// Update screen width and height since the screen size can change.
				screenWidth = driverCamera.pixelWidth;
				screenHeight = driverCamera.pixelHeight;

				// Calculate new position of reticle based on player input.
				targetPos.x = Mathf.Clamp(targetPos.x + PlayerInput.cameraHorizontal * targetMoveSpeed, 0f, screenWidth);
				targetPos.y = Mathf.Clamp(targetPos.y - PlayerInput.cameraVertical * targetMoveSpeed, 0f, screenHeight);

				// Place reticle at new position.
				if(reticle) reticle.rectTransform.anchoredPosition = targetPos - new Vector2(screenWidth/2f, screenHeight/2f);

				// Cast a ray out of the driver's camera to find a target.
				RaycastHit hit;
				Ray ray = driverCamera.ScreenPointToRay(targetPos);

				// Aim out the front of the camera to maxTargetingDistance.
				// Set turrent input aimpoint to anything hit or at max distance along camera forward vector if nothing hit.
				if(Physics.Raycast(ray, out hit, maxTargetingDistance, aimLayerMask))
				{
					turretInput.aimPoint = hit.point;
				}
				else
				{
					turretInput.aimPoint =  ray.GetPoint(maxTargetingDistance);
				}
			}
			
			// Set turret firing input.
			turretInput.primaryFire = PlayerInput.primaryFire;
			turretInput.secondaryFire = PlayerInput.secondaryFire;
			
			return turretInput;
		}
		
		public override void Exit(ref ControlReferences references)
		{
			// Return the reticle to its original position.
			if(reticle) reticle.rectTransform.anchoredPosition = reticleOriginalPos;
		}
	}
}
