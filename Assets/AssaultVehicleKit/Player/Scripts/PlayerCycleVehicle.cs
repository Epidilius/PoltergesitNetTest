using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Will cycle the current vehicle controlled by the PlayerDriver from
	//  a list of provided prefabs.
	//
	[RequireComponent(typeof(PlayerDriver))]
	public class PlayerCycleVehicle : MonoBehaviour 
	{		
		public VehiclePrefabs playerVehiclePrefabs;		// List of player vehicle prefabs to use for cycling.  Can be found on the same game object.

		private PlayerDriver playerDriver;

		void Awake()
		{
			// Get reference to the VehiclePrefabs which contains a list of the available prefabs for the player.
			if(!playerVehiclePrefabs) playerVehiclePrefabs = GetComponentInChildren<VehiclePrefabs>();
			if(!playerVehiclePrefabs) Debug.LogWarning("No player VehiclePrefabs specified or found for PlayerRespawner on " + name);

			// Get reference to PlayerDriver
			playerDriver = GetComponentInChildren<PlayerDriver>();
			if(!playerDriver) Debug.LogWarning("Unable to find PlayerDriver for PlayerCycleVehicle on " + name);
		}

		void Update()
		{
			// If cycling a vehicle, start that up.
			// Verify we have all references needed before proceeding.
			if(PlayerInput.cycleVehicle && playerDriver && playerDriver.vehicle && playerVehiclePrefabs && playerVehiclePrefabs.count > 0)
			{
				CycleVehicle();
			}
		}
		
		// Cycle vehicle demonstration...
		void CycleVehicle()
		{			
			// Obtain reference to old vehicle.
			Vehicle oldVehicle = playerDriver.vehicle;

			// Obtain the name of the old vehicle, minus any potential post affix "(Clone)", to find its prefab index.
			string oldVehicleName = oldVehicle.gameObject.name;
			oldVehicleName = oldVehicleName.Replace("(Clone)","");

			// Obtain the prefab index of the next vehicle to instantiate (+1 of the old index).
			int nextVehiclePrefabIndex = playerVehiclePrefabs.GetPrefabNameIndex(oldVehicleName) + 1;
			
			// Save position and velocity of the old vehicle.
			Vector3 position = oldVehicle.transform.position + Vector3.up * .5f;
			Rigidbody oldRb = oldVehicle.GetComponentInChildren<Rigidbody>();
			Vector3 velocity = (oldRb != null ? (oldRb.velocity.sqrMagnitude < .01f ? Vector3.forward : oldRb.velocity) : Vector3.forward);
			
			// Get the old vehicle entity if one exists so we can save Entity health, shield, etc.
			Entity oldEntity = oldVehicle.GetComponent<Entity>();
			
			// Disable old vehicle to avoid interaction with new one.
			oldVehicle.gameObject.SetActive(false);
			
			// Instantiate next vehicle
			Vehicle newVehicle = Instantiate(playerVehiclePrefabs.GetPrefabAtIndex(nextVehiclePrefabIndex), position, Quaternion.LookRotation(velocity)) as Vehicle;
			
			// Copy rigidbody velocity if the new vehicle has one
			Rigidbody newRb = newVehicle.GetComponentInChildren<Rigidbody>();
			if(newRb) newRb.velocity = velocity;
			
			// Obtain Entity on new vehicle
			Entity newEntity = newVehicle.GetComponent<Entity>();
			
			// If old and new vehicles are Entities (typically the case), copy status.
			if(oldEntity && newEntity)
			{
				newEntity.health = oldEntity.health;
				newEntity.shield = oldEntity.shield;
				newEntity.GainExperience(null, oldEntity.experience);
			}

			// Send event for new vehicle to set it as the player vehicle.
			Events.setPlayerVehicle(newVehicle, false);
			
			// Destroy old vehicle.
			Destroy(oldVehicle.gameObject);
		}
	}
}
