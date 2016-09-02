using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace hebertsystems.AVK
{
	//  Behavior providing simple player status readouts for health, shield and experience.
	//  Subscribes to the Events.playerEntityStarted event to find the player, then
	//  subscribes to the player's statusChanged event to update the status UI.
	//
	public class PlayerStatusHUD : MonoBehaviour 
	{
		[SerializeField] private Slider healthSlider;							// The slider used to indicate health
		[SerializeField] private Text healthText;								// The text used to indicate health
		[SerializeField] private Slider shieldSlider;							// The slider used to indicate shield
		[SerializeField] private Text shieldText;								// The text used to indicate shield
		[SerializeField] private Slider shieldRelativeSlider;					// The slider used to control the shield's relative size to health
		[SerializeField] private Text xpText;									// The text used to indicate Entity experience

		private Entity playerEntity;
		
		protected virtual void Awake()
		{
			// Subscribe to player started event.
			Events.playerEntityStarted += OnPlayerEntityStarted;
		}

		void OnPlayerEntityStarted(PlayerEntity player)
		{
			// If we already have a player (perhaps more than one in scene), unsubscribe to first's status changed event.
			if(playerEntity != null) playerEntity.statusChanged -= OnEntityStatusChanged;

			// Grab reference to player Entity and subscribe to status changed event.
			playerEntity = player;
			playerEntity.statusChanged += OnEntityStatusChanged;
		}
		
		void OnEntityStatusChanged(Entity source)
		{
			// Status has changed, update Entity status UI.

			// Update health slider and text.
			if(healthSlider) healthSlider.value = Mathf.InverseLerp(0, playerEntity.maxHealth, playerEntity.health);
			if(healthText) healthText.text = playerEntity.health.ToString();

			// Update relative size of shield to health, shield slider and text.
			if(shieldRelativeSlider) shieldRelativeSlider.value = Mathf.InverseLerp(0, playerEntity.maxHealth, playerEntity.maxShield);
			if(shieldSlider) shieldSlider.value = Mathf.InverseLerp(0, playerEntity.maxShield, playerEntity.shield);
			if(shieldText) shieldText.text = playerEntity.shield.ToString();

			// Update experience display.
			if(xpText) xpText.text = playerEntity.experience.ToString() + " xp";
		}
	}
}
