using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;

namespace hebertsystems.AVK
{
	//  Simple behavior to toggle the Player Controller Display on/off.
	//
	public class ToggleControllerDisplay : MonoBehaviour
	{
		private bool visibility = true;

		void Awake()
		{
			// Subscribe to the keyDown event to check for the 'M' key.
			PlayerInput.keyDown += OnKeyDown;
		}

		void OnDestroy()
		{
			PlayerInput.keyDown -= OnKeyDown;
		}

		void OnKeyDown(char c, KeyCode keyCode)
		{
			// Toggle active state on 'M' key down.
			if(keyCode == KeyCode.M)
			{
				visibility = !visibility;
				gameObject.SetActive(visibility);
			}
		}
	}
}
