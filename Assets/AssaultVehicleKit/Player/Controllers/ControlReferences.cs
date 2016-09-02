using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Structure containing references for use by camera/steering/aiming controllers.
	//
	public struct ControlReferences
	{
		public CameraType currentCameraType;				// The current camera type.
		public Transform cameraRigTransform;				// The transform of the camera rig, usually a parent of the camera.
		public Camera driverCamera;							// Reference to the driver camera rendering the scene.
		public Vehicle vehicle;								// Reference to the current driven vehicle.
	}
}
