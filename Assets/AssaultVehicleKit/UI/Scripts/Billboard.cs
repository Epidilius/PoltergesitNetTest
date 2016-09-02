using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Will provide billboard behavior to any gameobject with respect
	//  to a reference camera by modifying rotation.  There are 3 billboard
	//  modes that are supported.
	//
	public class Billboard : MonoBehaviour 
	{
		// Internal billboard mode
		public enum BillboardMode
		{
			Billboard,												// Standard billboard mode - the object stays flat within camera space
			VerticalBillboard,										// Vertical mode will act as a tree, keeping the vertical axis up
			HorizontalBillboard										// Horizontal mode will act as a decal on the ground, keeping the top away from the camera.
		}

		public BillboardMode mode = BillboardMode.Billboard;		// The mode for this billboard.
		public Camera referenceCamera;								// The reference camera to provide billboard behavior for.  If no camera specified, the main camera is used.
		public bool flipFace = false;								// Whether to flip the forward face of the billboard.

		private Transform cameraTransform;
		private Vector3 forward;
		private Vector3 up;

		void Awake () 
		{
			// Obtain camera references if none specified.
			if(!referenceCamera) referenceCamera = Camera.main;

			if(referenceCamera) cameraTransform = referenceCamera.transform;
			else Debug.Log("Unable to obtain camera reference for Billboard on " + name);

			DoBillboard();
		}

		void Update () 
		{
			DoBillboard();
		}

		void DoBillboard()
		{
			// If no camera reference available, do nothing.
			if(!cameraTransform) return;

			// Determine the forward and up vectors for this object based on the current mode.
			switch(mode)
			{
			case BillboardMode.VerticalBillboard:
				forward = cameraTransform.forward;
				forward.y = 0;
				if(forward == Vector3.zero)
				{
					forward = cameraTransform.up * (cameraTransform.forward.y > 0 ? -1 : 1);
					forward.y = 0;
				}
				up = Vector3.up;
				break;

			case BillboardMode.HorizontalBillboard:
				forward = Vector3.down;
				up = cameraTransform.forward;
				up.y = 0;
				if(up == Vector3.zero)
				{
					up = referenceCamera.transform.up * (cameraTransform.forward.y > 0 ? -1 : 1);
					up.y = 0;
				}
				break;

			case BillboardMode.Billboard:
			default:
				forward = cameraTransform.forward;
				up = cameraTransform.up;
				break;
			}

			// Flip forward if flipFace is set
			if(flipFace) forward *= -1;

			// Calculate rotation based on forward and up
			transform.rotation = Quaternion.LookRotation(forward, up);
		}
	}
}
