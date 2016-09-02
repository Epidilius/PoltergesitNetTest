using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Provides Turret behavior for the laser turret on RollerBots.
	//
	public class MiniLaserTurret : Turret 
	{		
		public Transform turretPivot;						// Reference to the main turret pivot Transform.
		public Transform turretGun;							// Reference to the turret gun Transform.
		
		public float turretTurnSpeed = 600;					// The turn speed of the main turret pivot
		
		public override TurretInput input					// Process TurretInput
		{
			set
			{
				if(value.primaryFire.HasValue) primaryFire = value.primaryFire.Value;
				if(value.secondaryFire.HasValue) secondaryFire = value.secondaryFire.Value;
				
				if(value.aimPoint.HasValue) aimPoint = value.aimPoint.Value;
			}
		}

		private Weapon[] weapons;
		private Vector3 aimPoint;
		private bool primaryFire = false;
		private bool secondaryFire = false;
		
		private float turretPivotAngle = 0;

		void Awake()
		{
			if(!turretPivot || !turretGun)
				Debug.LogWarning("Missing Transforms for MiniLaserTurret on " + name);
			
			// Obtain references to weapons.
			weapons = GetComponentsInChildren<Weapon>();
		}
		
		void LateUpdate()
		{
			// If we don't have all Transform references, do nothing.
			if(!turretPivot || !turretGun) return;

			// Get the aim point in local coordinate and remove any Y component.
			Vector3 localPoint = transform.InverseTransformPoint(aimPoint);
			localPoint.y = 0;

			// Intially turn the main turretPivot Transform directly at the point.
			// All other Transforms will be set as if we are aimed at the point, then
			// we go back and slowly pivot the entire turret at turretTurnSpeed.
			turretPivot.localRotation = Quaternion.LookRotation(localPoint);

			// Now get the aimPoint in the local coordinate position for the turretGun.
			localPoint = turretPivot.InverseTransformPoint(aimPoint) - turretGun.transform.localPosition;

			// Calculate pitch angle of turret gun at point.
			float pitchAngle = Mathf.Atan2 (localPoint.y, localPoint.z) * Mathf.Rad2Deg;
			
			// Set gun rotation to pitch angle.
			turretGun.transform.localRotation = Quaternion.Euler(-pitchAngle,0,0);
			
			// Now, go back and slowly pitch the whole turret to the point based on turretTurnSpeed.
			localPoint = transform.InverseTransformPoint(aimPoint);
			float pivotAngleTarget = Mathf.Atan2(localPoint.x, localPoint.z) * Mathf.Rad2Deg;

			// Calculate turret pivot angle based on turretTurnSpeed and set turret local rotation.
			turretPivotAngle = Mathf.MoveTowardsAngle(turretPivotAngle, pivotAngleTarget, turretTurnSpeed * Time.deltaTime);
			turretPivot.localRotation = Quaternion.AngleAxis(turretPivotAngle, Vector3.up);
			
			// Fire weapons based on input.
			foreach(Weapon weapon in weapons)
			{
				if( (primaryFire && weapon.type.Equals(Weapon.WeaponType.Primary)) ||
				   	(secondaryFire && weapon.type.Equals(Weapon.WeaponType.Secondary)) )
				{
					weapon.Fire();
				}
			}		 
		}
	}
}
