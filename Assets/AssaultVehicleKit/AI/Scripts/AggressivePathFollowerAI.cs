using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  AI behavior that follows a waypoint path but will shoot at any other Entity within
	//  a specified range.  The behavior is expected to be put on a gameobject with
	//  corresponding Entity and Vehicle (and Turret) behaviors.
	//
	[RequireComponent(typeof(Vehicle))]
	public class AggressivePathFollowerAI : WaypointPathFollower 
	{
		[Header("Path Following")]
		public float nextWaypointDistance = 20;					// The distance at which the AI will target the next waypoint to move towards.
		public float slowForTurnDistance = 100;					// The distance from the current targeted waypoint the AI will begin to slow for a turn, if needed.
		public float turnTime = 1;								// The time to turn the move direction from the old waypoint to the next waypoint.
		public float maxThrottle = 1;							// The maximum forward throttle to apply to the vehicle.
		public float minThrottle = .1f;							// The minimum forward throttle to apply to the vehicle.
		public int waypointStartIndex = -1;						// The index of the waypoint to start moving towards.  A value less than 0 (default) will choose closest waypoint.

		[Header("Enemy Targeting")]
		public float maxAngleToTarget = 180;					// The maximum angle to the target from the vehicle's forward direction.
		public float maxTargetEnemyDistance = 200;				// The maximum distance to target enemies, the closest within the distance will be targeted.
		public float maxShootingDistance = 100;					// The maximum distance to shoot at enemies.
		public float retargetInterval = 5;						// The time interval for re-evaluating targets.

		[Header("Primary Weapon Firing")]
		public float firePrimaryIntervalMin = .5f;				// The minimum interval for shooting with primary weapon.
		public float firePrimaryIntervalMax = 1;				// The maximum interval...
		public float firePrimaryChance = .3f;					// The chance of firing the primary weapon.

		[Header("Secondary Weapon Firing")]
		public float fireSecondaryIntervalMin = 3;				// The minimum interval for shooting with secondary weapon.
		public float fireSecondaryIntervalMax = 5;				// The maximum interval...
		public float fireSecondaryChance = .7f;					// The chance of firing the secondary weapon.

		
		private Entity target;
		private Entity myEntity;
		private Vehicle vehicle;
		private Turret[] turrets = new Turret[0];

		private VehicleInput vehicleInput;
		private TurretInput turretInput;
		
		private int currentWaypointIndex = 0;
		private Vector3 currentWaypoint = Vector3.zero;
		private Vector3 nextWaypoint = Vector3.one;
		private float turnStartTime = 0;
		private float turnStopTime = 0;
		private Vector3 lastMoveDirection;

		private float reTargetTime = 0;
		private float firePrimaryDecisionTime = 0;
		private float fireSecondaryDecisionTime = 0;
		
		void Awake () 
		{
			// Obtain references to the Vehicle, Turrets, and Entity this behavior is a part of.
			vehicle = GetComponentInChildren<Vehicle>();
			turrets = GetComponentsInChildren<Turret>();
			myEntity = GetComponentInChildren<Entity>();
		}

		void Start () 
		{
			// If a path has been specified, initialize the waypoint we are moving towards.
			if(path) 
			{
				if(waypointStartIndex < 0) 
				{
					waypointStartIndex = path.IndexClosestTo(transform.position);
				}
				currentWaypointIndex = waypointStartIndex;

				// Obtain the current and next waypoint positions, initialize the last move direction.
				currentWaypoint = path.WaypointAtIndex(currentWaypointIndex);
				lastMoveDirection = currentWaypoint - transform.position;
				lastMoveDirection.y = 0;
				nextWaypoint = path.WaypointAtIndex(currentWaypointIndex+1);
			}
			else
			{
				Debug.LogWarning("No WaypointPath specified for AI on " + name);
			}
		}


		void Update () 
		{
			// If no vehicle (should never be the case) do nothing.
			if(!vehicle) return;

			DriveVehicle();
			TargetEnemies();
			ShootAtEnemies();

			// Send inputs to Vehicle and Turrets
			vehicle.input = vehicleInput;

			foreach(Turret turret in turrets)
			{
				turret.input = turretInput;
			}
		}

		void DriveVehicle()
		{
			// If a path specified, drive the vehicle along the path.
			if(path)
			{
				// Determine the current moveDirection within XZ plane towards current waypoint.
				Vector3 moveDirection = currentWaypoint - transform.position;
				moveDirection.y = 0;
				
				// If the move direction distance is within the threshold to go to the next waypoint, do so.
				if(moveDirection.sqrMagnitude < nextWaypointDistance*nextWaypointDistance)
				{
					// Save last move direction.
					lastMoveDirection = moveDirection;
					
					// Update waypoints and current move direction.
					currentWaypointIndex++;
					currentWaypoint = path.WaypointAtIndex(currentWaypointIndex);
					nextWaypoint = path.WaypointAtIndex(currentWaypointIndex+1);
					moveDirection = currentWaypoint - transform.position;
					moveDirection.y = 0;
					
					// Initialize turn towards new waypoint.
					turnStartTime = Time.time;
					turnStopTime = Time.time + turnTime;
				}
				
				// Determine the move direction between the current and next waypoints to see if we have a sharp turn ahead.
				Vector3 nextMoveDirection = nextWaypoint - currentWaypoint;
				nextMoveDirection.y = 0;
				
				// Assume vehicle throttle is initially full out.				
				float throttle = 1;
				
				// If we are within the slowForTurnDistance, start decreasing vehicle throttle if needed.
				if(moveDirection.magnitude < slowForTurnDistance)
				{
					// Determine the ratio between min and max throttle values depending on distance from waypoint and if a sharp turn is ahead.
					// If no sharp turn (straight on), no need to decrease throttle.
					float t = Mathf.InverseLerp(slowForTurnDistance,nextWaypointDistance,moveDirection.magnitude) * Mathf.InverseLerp(1,0,Vector3.Dot(moveDirection.normalized, nextMoveDirection.normalized));
					
					throttle = Mathf.Lerp(maxThrottle, minThrottle, t);
				}
				
				// Turn from lastMoveDirection to current moveDirection within turnTime.
				float turnLerp = Mathf.InverseLerp(turnStartTime, turnStopTime, Time.time);
				moveDirection = Vector3.Lerp(lastMoveDirection, moveDirection, turnLerp);
				
				// Update vehicle moveDirection and throttle values.
				vehicleInput.moveDirection = moveDirection;
				vehicleInput.forwardThrottle = throttle;
			}
		}

		void TargetEnemies()
		{
			// Time to re-evaluate potential targets.  The closest within range will be targeted.
			if(Time.time >= reTargetTime)
			{
				// Set time for next re-targeting.
				reTargetTime = Time.time + retargetInterval;
				
				// Reset current target and obtain list of all potential targets in scene.
				target = null;
				Entity[] targets = FindObjectsOfType<Entity>();
				
				// Find closest target within maxAngleToTarget and max targeting distance.
				Vector3 forward = vehicle.forward;
				float closestDistance = float.MaxValue;
				foreach(Entity entity in targets)
				{
					if(entity != myEntity) 
					{
						Vector3 vectorToTarget = entity.transform.position - myEntity.transform.position;
						float angleToTarget = Vector3.Angle(vectorToTarget, forward);
						float sqDist = vectorToTarget.sqrMagnitude;
						if(angleToTarget <= maxAngleToTarget && sqDist < maxTargetEnemyDistance*maxTargetEnemyDistance && sqDist < closestDistance)
						{
							target = entity;
							closestDistance = sqDist;
						}
					}
				}
			}
		}

		void ShootAtEnemies()
		{
			// If we have a target, decide to shoot at it or not.
			if(target)
			{
				// Aim turrets at the target.
				turretInput.aimPoint = target.transform.position;
				
				// Time to decide to fire primary weapon or not.
				if(Time.time >= firePrimaryDecisionTime)
				{
					// Reset time for next decision.
					firePrimaryDecisionTime = Time.time + Random.Range(firePrimaryIntervalMin, firePrimaryIntervalMax);

					// Get the distance to the target and decide to shoot if it is within range.
					Vector3 vectorToTarget = target.transform.position - transform.position;
					if(vectorToTarget.magnitude <= maxShootingDistance)
					{
						turretInput.primaryFire = (Random.Range(0f,1f) <= firePrimaryChance);
					}
					else turretInput.primaryFire = false;
				}
				
				// Time to decide to fire secondary weapon or not.
				if(Time.time >= fireSecondaryDecisionTime)
				{
					// Reset time for next decision.
					fireSecondaryDecisionTime = Time.time + Random.Range(fireSecondaryIntervalMin, fireSecondaryIntervalMax);

					// Get the distance to the target and decide to shoot if it is within range.
					Vector3 vectorToTarget = target.transform.position - transform.position;
					if(vectorToTarget.magnitude <= maxShootingDistance)
					{
						turretInput.secondaryFire = (Random.Range(0f,1f) <= fireSecondaryChance);
					}
					else turretInput.secondaryFire = false;
				}
			}
			else
			{
				// No target, so aim along our move direction and do not fire.
				turretInput.aimPoint = transform.position + vehicle.forward * 1000;
				turretInput.primaryFire = false;
				turretInput.secondaryFire = false;
			}
		}
	}
}
