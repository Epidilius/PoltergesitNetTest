using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace hebertsystems.AVK
{
	//  CCTV CameraController.
	//  This camera controller is a stationary camera located at pre-determined positions
	//  provided by children of the CCTVCameraPositions reference object and smoothly follows (looks at) the player vehicle.
	//  The controller will switch camera positions based on proximity.
	//
	public class CCTVCamera : CameraController 
	{
		public float cameraTrackSmoothTime = .5f;							// The smoothing time of the vehicle tracking motion.
		public float cameraSwitchFactor = .8f;								// To switch to a new camera, the new camera must be cameraSwitchFactor closer than the current.

		public float evaluateCameraSwitchInterval = .5f;					// Time interval to re-evaluate camera positions (avoid doing every frame).

		public GameObject CCTVCameraPositions;								// A GameObject containing children objects representing camera positions.


		private List<Vector3> cameraPositions = new List<Vector3>();
		private float nextEvaluateTime = 0;
		private CameraInput cameraInput;


		void Awake()
		{
			if(!CCTVCameraPositions)
			{
				Debug.LogWarning("No CCTV Camera Positions specified for CCTVCamera controller on " + name);
				return;
			}

			// Get position of all child GameObject's of CCTVCameraPositions, considered to be CCTV camera positions.
			cameraPositions = CCTVCameraPositions.transform.Cast<Transform>().Select(t=>t.position).ToList();
		}

		private bool GetClosestCCTVPosition(Vector3 vehiclePosition, Vector3 currentCameraPosition, out Vector3 newCameraPosition)
		{
			// If no camera positions, just return 0,100,0.
			if(cameraPositions.Count == 0) 
			{
				newCameraPosition = Vector3.up * 100;
				return currentCameraPosition != newCameraPosition;
			}

			// Squared distance between the vehicle and current camera position.
			float currentPositionSqrDistance = (vehiclePosition - currentCameraPosition).sqrMagnitude;

			// Initially set the switch flag to false - will get set if the position is actually changed.
			bool cameraSwitch = false;

			// Initially, just set next position to current.  The cameraSwitch flag won't get set unless it changes.
			newCameraPosition = currentCameraPosition;

			// The closest squared distance used to pick the closest one.
			float closestSqrDistance = currentPositionSqrDistance;

			// Iterate through all CCTV camera positions for evaluation.
			for(int i=0; i<cameraPositions.Count; i++)
			{
				Vector3 position = cameraPositions[i];

				// If the position is different than the current position, continue with evaluation.
				if(position != currentCameraPosition)
				{
					// Squared distance of current position and vehicle
					float sqrDistance = (vehiclePosition - position).sqrMagnitude;
					// If the distance is closer than closest so far and also closer than cameraSwitchFactor of current distance, set new position.
					if(sqrDistance < closestSqrDistance && sqrDistance < currentPositionSqrDistance * cameraSwitchFactor)
					{
						// Set new position, closestSqrDistance, and set flag to true.
						newCameraPosition = position;
						closestSqrDistance = sqrDistance;
						cameraSwitch = true;
					}
				}
			}

			return cameraSwitch;
		}
		
		public override void Initialize(ref ControlReferences references)
		{
			// Obtain reference to vehicle and if not currently set, just return.
			Vehicle vehicle = references.vehicle;
			if(!vehicle) return;

			// Get the closest CCTV camera position to the vehicle and set rotation to look at the vehicle.
			GetClosestCCTVPosition(vehicle.position, cameraInput.position, out cameraInput.position);
			cameraInput.rotation = Quaternion.LookRotation(vehicle.position - cameraInput.position);

			// Set camera type (for use elsewhere)
			references.currentCameraType = CameraType.Stationary;
		}
		
		public override CameraInput UpdateCamera(ref ControlReferences references)
		{
			// Obtain reference to vehicle and if not currently set, just return current input.
			Vehicle vehicle = references.vehicle;
			if(!vehicle) return cameraInput;

			bool cameraSwitch = false;
			// Is it time to re-evaluate the closest CCTV camera position?
			if(Time.time >= nextEvaluateTime)
			{
				// Get closest position.  cameraSwitch will be true if the position was actually switched.
				cameraSwitch = GetClosestCCTVPosition(vehicle.position, cameraInput.position, out cameraInput.position);
				// Set next evaluation time.
				nextEvaluateTime = Time.time + evaluateCameraSwitchInterval;
			}

			// Update camera rotation to look at the vehicle.
			// If we just switched the camera or there is no smooth time, update the rotation immediately.
			// If not, smoothly track the vehicle with cameraTrackSmoothTime.
			cameraInput.rotation = (cameraSwitch || cameraTrackSmoothTime <= 0) ?
				Quaternion.LookRotation(vehicle.position - cameraInput.position) :
				Quaternion.Slerp(cameraInput.rotation, Quaternion.LookRotation(vehicle.position - cameraInput.position), Time.deltaTime/cameraTrackSmoothTime);

			return cameraInput;
		}
		
		public override void Exit(ref ControlReferences references)
		{
		}
	}
}
