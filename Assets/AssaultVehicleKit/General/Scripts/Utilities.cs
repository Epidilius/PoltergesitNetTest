using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Utilities class to contain any handy methods for the project.
	//
	public static class Utilities
	{
		public static bool CursorLocked()
		{
			#if UNITY_5_0
			return !(Cursor.lockState == CursorLockMode.None);
			#else
			return Screen.lockCursor;
			#endif
		}
		
		public static void LockCursor(bool locked)
		{
			#if UNITY_5_0
			Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = !locked;
			#else
			Screen.lockCursor = locked;
			#endif
		}
	}
}
