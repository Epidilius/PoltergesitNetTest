using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  The CameraInput structure containing camera parameters set by CameraControllers.
	//
	public struct CameraInput
	{
		public Vector3 position;				// The desired position of the camera.
		public Quaternion rotation;				// The desired rotation of the camera.
		public float? fov;						// The desired FOV of the camera (optional).

		// Lerp between two CameraInput values.
		public static CameraInput Lerp(CameraInput from, CameraInput to, float t)
		{
			CameraInput result = new CameraInput();

			result.position = Vector3.Lerp(from.position, to.position, t);
			result.rotation = Quaternion.Slerp(from.rotation, to.rotation, t);

			if(from.fov.HasValue && to.fov.HasValue) result.fov = Mathf.Lerp(from.fov.Value, to.fov.Value, t);

			return result;
		}
	}
}