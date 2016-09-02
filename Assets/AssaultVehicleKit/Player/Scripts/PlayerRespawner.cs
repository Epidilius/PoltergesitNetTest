using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hebertsystems.AVK
{
	//  Behavior providing player respawning when the player dies.
	//  Subscribes to the Events.playerEntityStarted event to find the player, then
	//  subscribes to that player's death event in order to start the respawn timer 
	//  when the player dies.  Respawns the player after the timer is up.
	//
	public class PlayerRespawner : MonoBehaviour 
	{
		public Timer timer;									// Reference to the timer to start for player respawn.
		public int respawnDelay = 4;						// Respawn delay.
		public VehiclePrefabs playerVehiclePrefabs;			// List of player vehicle prefabs to use for respawning.  Can be found on the same game object.

		private Entity playerEntity;
		private Vehicle playerPrefab;

		private Vector3 originalPosition;
		private Quaternion originalRotation;
		private bool originalSet = false;

		void Awake()
		{
			// Get reference to the VehiclePrefabs which contains a list of the available prefabs for the player.
			if(!playerVehiclePrefabs) playerVehiclePrefabs = GetComponentInChildren<VehiclePrefabs>();
			if(!playerVehiclePrefabs) Debug.LogWarning("No player VehiclePrefabs specified or found for PlayerRespawner on " + name);

			// De-activate the timer initially, no need to see it now.
			if(timer) 
			{
				timer.gameObject.SetActive(false);
				// Subscribe to time complete event.
				timer.timerComplete += OnTimerComplete;
			}
			else Debug.LogWarning("No Timer set for PlayerRespawner on " + name);

			// Subscribe to player started event.
			Events.playerEntityStarted += OnPlayerEntityStarted;
		}

		void OnPlayerEntityStarted(PlayerEntity player)
		{
			// If we already have a player (perhaps more than one in scene), unsubscribe to first's death event.
			if(playerEntity != null) playerEntity.death -= OnPlayerEntityDeath;

			// Grab player reference and subscribe to player entity death event.
			playerEntity = player;
			playerEntity.death += OnPlayerEntityDeath;

			// Capture the original spawn position and rotation of the player, but only the first time (vehicle cycling could corrupt this otherwise)
			if(!originalSet)
			{
				originalPosition = playerEntity.transform.position;
				originalRotation = playerEntity.transform.rotation;
				originalSet = true;
			}
		}

		void OnPlayerEntityDeath(Entity source, Dictionary<Entity, int> culprits)
		{
			// Find current player entity in playerVehiclePrefabs.
			// Obtain the name of the current player, minus any potential post affix "(Clone)", to find its prefab index.
			string oldName = playerEntity.gameObject.name;
			oldName = oldName.Replace("(Clone)","");

			// Obtain reference to the current player's prefab to respawn.
			int prefabIndex = playerVehiclePrefabs.GetPrefabNameIndex(oldName);
			playerPrefab = playerVehiclePrefabs.GetPrefabAtIndex(prefabIndex);

			// Start the respawn timer if one set.
			if(timer)
			{
				timer.gameObject.SetActive(true);
				timer.StartTimer(respawnDelay);
			}
			// Otherwise, just respawn player now.
			else if(playerPrefab)
			{
				playerEntity = Instantiate(playerPrefab, originalPosition + Vector3.up * .5f, originalRotation) as Entity;
			}
		}

		void OnTimerComplete()
		{
			// When respawn timer is done, de-activate the timer and instantiate the player prefab.
			timer.gameObject.SetActive(false);

			// Set new player entity active.
			if(playerPrefab) playerEntity = Instantiate(playerPrefab, originalPosition + Vector3.up * .5f, originalRotation) as Entity;
		}
	}
}
