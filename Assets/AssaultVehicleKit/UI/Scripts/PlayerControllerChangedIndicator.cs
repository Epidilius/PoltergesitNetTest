using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;

namespace hebertsystems.AVK
{
	//  Visual indication that a PlayerController has changed.
	//  Will flash the transparency of any UI elements that are a part of this object
	//  to briefly indicate a controller has changed.
	//
	public class PlayerControllerChangedIndicator : MonoBehaviour
	{
		public PlayerControllerType type;					// The type of controller (camera/steering/aiming)
					
		public float fadeOutTime = .5f;						// The time to fade out the UI elements during controller change.

		public Image[] images = new Image[0];				// Any UI images to turn on and fade out on a controller change.  Can be found as part of the same game object.

		private float transparency = 0;
		
		protected virtual void Awake()
		{
			// Grab all child Images
			if(images.Length == 0) images = GetComponentsInChildren<Image>();
			if(images.Length == 0) Debug.LogWarning("No Indicator Images set for PlayerControllerChangedIndicator on " + name);
		}

		IEnumerator Start()
		{
			yield return new WaitForEndOfFrame();

			// Subscribe to controller set event
			// Note:  Doing this a little later to avoid the initial controllers setup event.
			switch(type)
			{
			case PlayerControllerType.CameraController:
				Events.setCameraController += OnSetController;
				break;
				
			case PlayerControllerType.SteeringController:
				Events.setSteeringController += OnSetController;
				break;
				
			case PlayerControllerType.AimingController:
				Events.setAimingController += OnSetController;
				break;
			}
		}

		void OnEnable()
		{
			// When enabled, set transparency to 0 for changed indicator images.
			transparency = 0;
			SetTransparency(transparency);
		}

		void Update()
		{
			// If transparency above 0, decrease based on fadeOutTime and set transparency for images.
			if(transparency > 0)
			{
				transparency = Mathf.Clamp01(transparency - Time.deltaTime / fadeOutTime);
				SetTransparency(transparency);
			}
		}
		
		void SetTransparency(float t)
		{
			// Set transparency for each Image.
			for(int i=0; i<images.Length; i++)
				images[i].canvasRenderer.SetAlpha(t);
		}

		// Set transparency to fully opaque when controller has changed.
		void OnSetController(PlayerController playerController)
		{
			transparency = 1;
		}

		void OnDestroy()
		{
			// UnSubscribe to controller set events
			Events.setCameraController -= OnSetController;
			Events.setSteeringController -= OnSetController;
			Events.setAimingController -= OnSetController;
		}
	}
}
