using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace hebertsystems.AVK
{
	//  Shows experienced gained text effect on the screen
	//  where the Entity died.
	//
	public class PlayerXPGainedUI : MonoBehaviour 
	{
		public EntityXPText experiencePrefab;							// The Text UI prefab to instantiate when the player gains experience.
		public Canvas parentCanvas;										// The parent Canvas for the Text.  If not specified, will attempt to find it.

		private Entity playerEntity;
		[SerializeField] private Camera referenceCamera;				// The reference camera used to determine the position of the Entity that died to give xp.

		// Use this for initialization
		void Awake () 
		{
			if(!referenceCamera) referenceCamera = Camera.main;
			if(!referenceCamera) Debug.LogWarning("No main camera found for PlayerXPGainedUI on " + name);

			if(!parentCanvas) parentCanvas = GetComponentInParent<Canvas>();
			if(!parentCanvas) Debug.LogWarning("No parent Canvas found for PlayerXPGainedUI on " + name);

			// Subscribe to player started event.
			Events.playerEntityStarted += OnPlayerEntityStarted;
		}
		
		void OnPlayerEntityStarted(PlayerEntity player)
		{
			// If we already have a player (perhaps more than one in scene), unsubscribe to first's experience gained event.
			if(playerEntity != null) playerEntity.experienceGained -= OnEntityExperienceGained;

			// Grab reference to player Entity and subscribe to experienceGained event.
			playerEntity = player;
			playerEntity.experienceGained += OnEntityExperienceGained;
		}
		
		void OnEntityExperienceGained(Entity source, Entity provider, int amount)
		{
			if(parentCanvas && experiencePrefab && provider)
			{
				// Where in the reference camera viewport did the experience come from (provider).
				Vector3 viewpoirtPoint = referenceCamera.WorldToViewportPoint(provider.transform.position);

				// Instantiate the xp gained prefab and set its initial position based on the viewport point of the provider of the xp.
				EntityXPText clone = Instantiate(experiencePrefab) as EntityXPText;
				clone.initialPosition = new Vector2(viewpoirtPoint.x * referenceCamera.pixelWidth, viewpoirtPoint.y * referenceCamera.pixelHeight);
				// Set parent to parentCanvas and set xp text to display.
				clone.transform.SetParent(parentCanvas.transform, false);
				clone.text.text = amount.ToString() + " xp";
			}
		}
	}
}
