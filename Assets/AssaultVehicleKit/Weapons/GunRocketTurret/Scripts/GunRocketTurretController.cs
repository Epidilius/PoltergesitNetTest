using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Behavior to control the GunRocketTurret prefab.
	//
	public class GunRocketTurretController : Turret 
	{
		public Transform turretPivot;					// Reference to the main turret pivot Transform.
		public Transform turretSight;					// Reference to the turret sight Transform.
		public Transform turretPitchArm;				// Reference to the pitch arm Transform on the right side of the turret.
		public Transform turretGunPivot;				// Reference to the gun pivot base Transform.
		public Transform turretGun;						// Reference to the final gun Transform itself.
		public float gunPitchForwardLimit = 25;			// The pitch forward angle limit for the gun.
		public float gunPitchBackLimit = 40;			// The pitch back angle limit for the gun.

		public float turretTurnSpeed = 600;				// Turn speed of the main turret pivot.

		public override TurretInput input				// Process TurretInput
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
			if(!turretPivot || !turretSight || !turretPitchArm || !turretGunPivot || !turretGun)
				Debug.LogWarning("Missing Transforms for GunRocketTurretController on " + name);

			// Obtain references to weapons.
			weapons = GetComponentsInChildren<Weapon>();
		}

		void LateUpdate () 
		{
			// If we don't have all Transform references, do nothing.
			if(!turretPivot || !turretSight || !turretPitchArm || !turretGunPivot || !turretGun) return;

			// Get the aim point in local coordinate and remove any Y component.
			Vector3 localPoint = transform.InverseTransformPoint(aimPoint);
			localPoint.y = 0;
			
			// Intially turn the main turretPivot Transform directly at the point.
			// All other Transforms will be set as if we are aimed at the point, then
			// we go back and slowly pivot the entire turret at turretTurnSpeed.
			turretPivot.localRotation = Quaternion.LookRotation(localPoint);

			// Now get the aimPoint in the base turretPivot local coordinates.
			localPoint = turretPivot.InverseTransformPoint(aimPoint);
			
			// Aim turret sight at the point
			turretSight.transform.localRotation = Quaternion.LookRotation(localPoint - turretSight.transform.localPosition);

			// Get the aimPoint in the turretPitchArm's local coordinates and remove any Y component.
			localPoint = turretPitchArm.InverseTransformPoint(aimPoint);
			localPoint.y =0;

			// Aim the turretGunPivot at the point.
			turretGunPivot.localRotation = Quaternion.LookRotation(localPoint);

			// Finally get the aimPoint in turretGunPivot's local coordinate to calculate pitch angle for the gun.
			localPoint = turretGunPivot.InverseTransformPoint(aimPoint) - turretGun.transform.localPosition;

			// Calculate pitch angle for the gun and clamp it between pitch forward and backward limits, then set the local rotation for the gun.
			float gunPitchAngle = Mathf.Clamp(-Mathf.Atan2 (localPoint.y, localPoint.z) * Mathf.Rad2Deg, -gunPitchBackLimit, gunPitchForwardLimit);
			turretGun.transform.localRotation = Quaternion.AngleAxis(gunPitchAngle, Vector3.right);

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
