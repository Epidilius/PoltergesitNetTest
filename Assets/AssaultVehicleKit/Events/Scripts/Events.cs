using UnityEngine;
using System;
using System.Collections;

namespace hebertsystems.AVK
{
	//  The Events class contains all the system level events
	//  for the AVK system.
	//
	public static class Events
	{
		public static Action<PlayerEntity> playerEntityStarted;				// Player Entity started event.
		public static Action<Vehicle, bool> setPlayerVehicle;				// Player vehicle set event.
		public static Action<CameraController> setCameraController;			// Camera controller set event.
		public static Action<SteeringController> setSteeringController;		// Steering controller set event.
		public static Action<AimingController> setAimingController;			// Aiming controller set event.

		public static CameraController currentCameraController;				// Current CameraController.
		public static SteeringController currentSteeringController;			// Current SteeringController.
		public static AimingController currentAimingController;				// Current AimingController.

		public static void PlayerEntityStarted(PlayerEntity playerEntity)
		{
			if(playerEntityStarted != null) playerEntityStarted(playerEntity);
		}

		public static void SetPlayerVehicle(Vehicle vehicle, bool resetControls = true)
		{
			if(setPlayerVehicle != null) setPlayerVehicle(vehicle, resetControls);
		}

		public static void SetCameraController(CameraController controller)
		{
			currentCameraController = controller;
			if(setCameraController != null) setCameraController(controller);
		}

		public static void SetSteeringController(SteeringController controller)
		{
			currentSteeringController = controller;
			if(setSteeringController != null) setSteeringController(controller);
		}

		public static void SetAimingController(AimingController controller)
		{
			currentAimingController = controller;
			if(setAimingController != null) setAimingController(controller);
		}
	}
}
