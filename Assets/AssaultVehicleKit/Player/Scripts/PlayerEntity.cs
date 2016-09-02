using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace hebertsystems.AVK
{
	//  The PlayerEntity represents the player in the scene.
	//  There should only be one in the scene at a time.
	//  Other scripts can listen for the Events.playerEntityStarted event to latch
	//  on to the PlayerEntity and provide player control and status feedback.
	//
	public class PlayerEntity : Entity 
	{
		// Static player reference.
		public static Entity player		{get {return sPlayer;} }			// Reference to the Player Entity in the scene if one exists (read only).
		private static Entity sPlayer = null;							

		
		protected override void Start()
		{
			// Note:  This system will still work if there are multiple PlayerEntities in the scene,
			//        but the last one to register and send out the event will get the attention.
			//        There should only be one PlayerEntity in the scene at a time.

			// Set static player Entity reference and send playerEntityStarted event.
			sPlayer = this;
			Events.playerEntityStarted(this);
			
			base.Start();
		}

		protected override void OnDestroy()
		{
			// Check if we are the Player Entity (we should be) and if so, reset the static reference to null.
			if(sPlayer == this) sPlayer = null;
			
			base.OnDestroy();
		}
	}
}
