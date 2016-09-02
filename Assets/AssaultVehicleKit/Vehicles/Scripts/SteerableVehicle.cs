using UnityEngine;

namespace hebertsystems.AVK
{
	//  Base class for all Steerable Vehicles.
	//  Steerable vehicles are those that are steered along a horizontal (ground) plane,
	//  and would have a steer angle relative to the forward direction of the vehicle,
	//  such as cars and boats.
	//
	public abstract class SteerableVehicle : Vehicle
	{
		public override VehicleInput input												// Process Vehicle input.
		{
			set
			{
				if(value.forwardThrottle.HasValue) mThrottle = Mathf.Clamp(value.forwardThrottle.Value, -1, 1);
				
				if(value.moveDirection.HasValue) SteerDirection(value.moveDirection.Value);
				if(value.relativeMoveDirection.HasValue) SteerDirectionRelative(value.relativeMoveDirection.Value);
			}
		}

		public abstract float steerAngleCurrent { get; }								// The current steer angle of the vehicle (read only).
		public virtual float throttle { get {return mThrottle;} }						// The current vehicle throttle [-1 to 1] (read only).

		// Protected members for use by derived classes.  External controllers should
		// use VehicleInput to control the vehicle as usual.

		protected float mSteerAngleTarget = 0;
		protected float mThrottle = 0;

		protected abstract float steerAngleTarget { get; set; }			// The target steer angle.

		protected override void Awake () 
		{
			base.Awake();
		}

		protected virtual void SteerDirection(Vector3 worldSteerDirection)
		{
			// Obtain relative steer direction to vehicle and process that.
			Vector3 relativeDirection = transform.InverseTransformDirection(worldSteerDirection);
			// If throttle is in reverse, mirror the x component of relative move direction.
			if(mThrottle < 0) relativeDirection.x *= -1;

			SteerDirectionRelative(relativeDirection);
		}

		protected virtual void SteerDirectionRelative(Vector3 localSteerDirection)
		{
			// Remove any y component of the localSteerDirection so direction is only in the local XZ plane.
			localSteerDirection.y = 0;
			
			// Calculate angle between forward and the localSteerDirection and set steer angle target
			steerAngleTarget = Mathf.Atan2(localSteerDirection.x, localSteerDirection.z) * Mathf.Rad2Deg;
		}
	}
}
