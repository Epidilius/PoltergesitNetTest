using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hebertsystems.AVK
{
	//  This behavior will randomly spawn Entity prefabs along a path.
	//  It is expected for the Entity prefabs to also contain a
	//  WaypointPathFollower component so the Entity can be set on
	//  the path.
	//
	public class WaypointAISpawner : MonoBehaviour 
	{
		public WaypointPath path;									// The path to spawn AI prefabs along.  Can be found on the same game object.
		public VehiclePrefabs vehiclePrefabs;						// List of vehicle prefabs to spawn along the path.  Can be found on the same game object.

		public int spawnNumber = 10;								// The number of prefabs to keep active.
		public float spawnDelay = 1;								// The delay between spawns.


		private int spawnsLeft = 0;
		private float nextSpawnTime = 0;

		void Awake()
		{
			// Obtain reference to path if not already specified.
			if(!path) path = GetComponentInChildren<WaypointPath>();
			if(path) spawnsLeft = spawnNumber;
			else Debug.LogWarning("No WaypointPath specified or found for WaypointAISpawner on " + name);

			// Obtain reference to vehicle prefabs if not already specified.
			if(!vehiclePrefabs) vehiclePrefabs = GetComponentInChildren<VehiclePrefabs>();
			if(!vehiclePrefabs) Debug.LogWarning("No VehiclePrefabs specified or found for WaypointAISpawner on " + name);
		}

		void Start () 
		{
			// Set next spawn time.
			nextSpawnTime = Time.time + spawnDelay;
		}

		void Update () 
		{
			// While we have spawns left and have reached the next spawn time, spawn an Entity prefab.
			if(vehiclePrefabs && spawnsLeft > 0 && Time.time >= nextSpawnTime && vehiclePrefabs.count > 0)
			{
				// Decrement spawns left and reset next spawn time.
				spawnsLeft--;
				nextSpawnTime = Time.time + spawnDelay;

				// Get random waypoint index and spawn a prefab near it.
				int waypointIndex =  Random.Range(0, path.count);
				Vector3 placeVector = path.WaypointAtIndex(waypointIndex) - path.WaypointAtIndex(waypointIndex - 1);
				Vector3 position = path.WaypointAtIndex(waypointIndex) - placeVector.normalized + Vector3.up;
				Quaternion rotation = Quaternion.LookRotation(placeVector);

				// Instantiate random vehicle from aiVehiclePrefabs list.
				Vehicle aiVehicle = Instantiate(vehiclePrefabs.GetPrefabAtIndex(Random.Range(0, vehiclePrefabs.count)), position, rotation) as Vehicle;

				// Attempt to find a WaypointPathFollower component and if found, set it on it's path.
				WaypointPathFollower follower = null;
				if(aiVehicle) follower = aiVehicle.GetComponentInChildren<WaypointPathFollower>();
				if(follower) follower.path = path;

				// Subscribe to entity death event
				Entity entity = aiVehicle.GetComponent<Entity>();
				if(entity) entity.death += OnEntityDeath;
			}
		}

		void OnEntityDeath(Entity source, Dictionary<Entity, int> culprits)
		{
			// One of the Entity prefabs has died, spawn a new one.
			spawnsLeft++;
		}
	}
}
