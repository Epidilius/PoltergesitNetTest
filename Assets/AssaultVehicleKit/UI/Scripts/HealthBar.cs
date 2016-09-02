using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

namespace hebertsystems.AVK
{
	//  Behavior to display Entity health as a simple health bar.
	//  This component is expected to be placed as a child of an Entity
	//  with the UI elements (Canvas, Image, etc.) as children.
	//  This behavior would typically be used along with the Billboard to always
	//  face the reference camera.
	//
	public class HealthBar : MonoBehaviour
	{
		[SerializeField]
		private Slider slider;									// The slider used to indicate health.
		public Gradient healthGradient;							// The color gradient to use as a function of health between 0 and Max.

		public Rect visibleViewportRect = new Rect(0,0,1,1);	// The normalized viewport Rect that the Entity must be within (of referenceCamera) for the health bar to be visible.
		public float transparencyFadeTime = 1;					// The fade time of turning health bar on and off between visible states.

		public float maxDistanceFromCamera = 100;				// The maximum distance the Entity can be from the camera for the health bar to be visible.
		public Camera referenceCamera;							// The reference camera to use to obtain Entity viewport coordinates.
																// If no camera specified, Camera.main will be used if available.

		private Entity entity;
		private CanvasRenderer[] canvasRenderers = new CanvasRenderer[0];
		private float mTransparency = 0;
		private Vector3 relativeWorldPosition = Vector3.zero;
		private Image sliderFill;

		protected virtual void Awake()
		{
			// Obtain reference to Entity this health bar is attached to.
			entity = GetComponentInParent<Entity>();
			if(!entity) Debug.LogWarning("No Entity found for HealthBar on " + name);

			// Obtain reference to all CanvasRenderers in children, used later for setting alpha (transparency) of any UI elements.
			canvasRenderers = GetComponentsInChildren<CanvasRenderer>();

			// Save relative world position between the health bar and Entity.
			if(entity) relativeWorldPosition = transform.position - entity.transform.position;

			// If referenceCamera was not specified, attempt to use Camera.main.
			if(!referenceCamera) referenceCamera = Camera.main;
			if(!referenceCamera) Debug.LogWarning("No main camera found for HealthBar on " + name);

			// Obtain slider (if not manually specified) and slider "Fill" Image.
			if(!slider) slider = GetComponentInChildren<Slider>();
			if(slider) sliderFill = slider.GetComponentsInChildren<Image>().First(i=>i.name.Equals("Fill"));
			else Debug.Log("No Slider specified for HealthBar on " + name);
			if(!sliderFill) Debug.Log("No Slider 'Fill' Image found for HealthBar on " + name);
		}

		protected virtual void Update()
		{
			if(entity) 
			{
				// Set the position based on Entity and relativeWorldPosition (stay at same relative distance).
				transform.position = entity.transform.position + relativeWorldPosition;
			
				// Obtain the normalized viewport point of the Entity within the referenceCamera viewport.
				Vector3 entityViewportPoint = referenceCamera.WorldToViewportPoint(entity.transform.position);
				
				// Determine visiblity of the HealthBar.
				// Entity's normalized viewport point must be within visibleViewPortRect
				// Entity must be within maxVisibleDistance
				bool visible = visibleViewportRect.Contains(entityViewportPoint) && entityViewportPoint.z <= maxDistanceFromCamera;

				// Set transparency level (fade in or out, or keep at same level, depending on state).
				mTransparency = Mathf.Clamp01( mTransparency + Time.deltaTime/transparencyFadeTime * (visible ? 1 : -1));	

				if(slider)
				{
					// Set the slider value and sliderFill color based on ratio of Entity's health to maxHealth.
					float t = Mathf.InverseLerp(0, entity.maxHealth, entity.health);
					slider.value = t;

					if(sliderFill) sliderFill.color = healthGradient.Evaluate(t);
				}

				// Iterate through all CanvasRenderers and set alpha to current mTransparency
				foreach(CanvasRenderer canvasRenderer in canvasRenderers)
				{
					canvasRenderer.SetAlpha(mTransparency);
				}
			}
		}
	}
}
