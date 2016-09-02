using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;

namespace hebertsystems.AVK
{
	//  Will display PlayerController mode text based on the name of the currently selected controller.
	//  The type of PlayerController can be selected and that type's mode text will be displayed when selected.
	//  The mode text uses the gameobject name of the selected controller, with special text surrounded with parenthesis.
	//
	public class PlayerControllerDisplay : MonoBehaviour
	{
		public PlayerControllerType type;			// The type of controller (camera/steering/aiming)

		public Text modeLabel;						// The controller mode label, obtained from the name of the controller object before any parenthesis.
		public Text padding;						// Padding between the controller mode label and any subtype label.
		public Text modeSubtypeLabel;				// The controller mode subtype label, if needed, to further specify the controller mode.  Obtained within parenthesis of the controller name.
		
		protected virtual void Awake()
		{
			// Subscribe to controller set event depending on the type to display.
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
		
		void OnSetController(PlayerController controller)
		{
			// Obtain the mode and subtype (if specified) of the controller.
			// The mode is just the name of the object before any parenthesis, with the subtype given within parenthesis.
			// Example:  "Orbit Camera (World Up)" would give a mode of "Orbit Camera" and subtype of "World Up".
			string[] names = controller.name.Split('(',')');
			string modeText = names[0].Trim();
			string modeSubtypeText = names.Count() > 1 ? names[1].Trim() : null;

			// Set the label text with the mode and subtype text.
			if(modeLabel) modeLabel.text = modeText;
			if(padding) padding.gameObject.SetActive(modeSubtypeText != null);
			if(modeSubtypeLabel)
			{
				modeSubtypeLabel.gameObject.SetActive(modeSubtypeText != null);
				if(modeSubtypeText != null) modeSubtypeLabel.text = modeSubtypeText;
			}
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
