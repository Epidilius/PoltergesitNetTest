using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  The main class providing the driving mechanic for the player.
	//  The vehicle can either be set manually, or the PlayerDriver will listen
	//  for the "Events.playerEntityStarted" and "Events.setPlayerVehicle" events to attach to the Player Entity's vehicle.
	//  The player inputs are then fed generically to the vehicle and any turrets on the vehicle,
	//  allowing for a layer of abstraction between the driving/shooting inputs and the vehicles and turrets
	//  that respond to them.
	//
	public class PlayerDriver : MonoBehaviour 
	{
		public Vehicle vehicle;									// The vehicle for the player to drive.  Does not have to be set manually since
																// the system can listen for the "Events.playerEntityStarted" event to attach
																// to a vehicle marked as the players.

		public float cameraTransitionTime = .5f;				// The time to transition between camera controller position/rotation.

		private Turret[] turrets = new Turret[0];

		private CameraInput cameraInput;
		private VehicleInput vehicleInput;
		private TurretInput turretInput;

		private float cameraTransitionEndTime = 0;

		private CameraController cameraController;
		private CameraController previousCameraController;
		private SteeringController steeringController;
		private AimingController aimingController;
		private ControlReferences controlReferences;
		private Camera driverCamera;
		private float defaultCameraFOV = 60;

		
		void Awake()
		{
			// Subscribe to the playerEntityStarted event so that we can sync to the player entity's vehicle if one in the scene.
			Events.playerEntityStarted += OnPlayerEntityStarted;

			// Subscribe to the setPlayerVehicle event so that other behaviors can set the player vehicle.
			Events.setPlayerVehicle += OnSetPlayerVehicle;

			// Subscribe to the controller events to swap between the various controllers.
			Events.setCameraController += OnSetCameraController;
			Events.setSteeringController += OnSetSteeringController;
			Events.setAimingController += OnSetAimingController;		

			// If a vehicle manually set, set it up.
			if(vehicle) SetVehicle(vehicle, true);

			controlReferences.cameraRigTransform = transform;
			driverCamera = GetComponentInChildren<Camera>();
			controlReferences.driverCamera = driverCamera;
			if(driverCamera) defaultCameraFOV = driverCamera.fieldOfView;
		}
		
		void OnPlayerEntityStarted(PlayerEntity player)
		{
			// Get the vehicle component of the player entity gameobject.
			Vehicle playerVehicle = player.GetComponent<Vehicle>();

			// If the player vehicle is different than the pre-existing vehicle reference, set it up.
			if(playerVehicle != vehicle) SetVehicle(playerVehicle, true);
		}

		void OnSetPlayerVehicle(Vehicle playerVehicle, bool resetControls)
		{
			// Set up the vehicle and controls.
			SetVehicle(playerVehicle, resetControls);
		}

		void SetVehicle(Vehicle playerVehicle, bool resetControls)
		{
			controlReferences.vehicle = playerVehicle;

			// Verify vehicle reference to use (null can still be sent through events, so just making sure).
			if(!playerVehicle) return;

			// Set vehicle and child turret references.
			vehicle = playerVehicle;
			turrets = vehicle.gameObject.GetComponentsInChildren<Turret>();

			// Initialize camera controller if indicated.
			if(resetControls && cameraController)
			{
				controlReferences.currentCameraType = CameraType.None;
				cameraController.Initialize(ref controlReferences);
			}
		}

		void OnSetCameraController(CameraController controller)
		{
			// If there is a current camera controller, set it as the previous controller.
			if(cameraController) 
			{
				// Set as previous.
				previousCameraController = cameraController;

				// Set the camera transition end time to lerp between the previous and current camera controllers.
				if(cameraTransitionTime > 0) 
				{
					cameraTransitionEndTime = Time.time + cameraTransitionTime;
				}
			}

			// Set new camera controller and initialize it.
			cameraController = controller;
			cameraController.Initialize(ref controlReferences);
		}

		void OnSetSteeringController(SteeringController controller)
		{
			// Exit the current steering controller.
			if(steeringController) steeringController.Exit(ref controlReferences);

			// Set new steering controller and initialize it.
			steeringController = controller;
			steeringController.Initialize(ref controlReferences);
		}

		void OnSetAimingController(AimingController controller)
		{
			// Exit the current aiming controller.
			if(aimingController) aimingController.Exit(ref controlReferences);

			// Set new aiming controller and initialize it.
			aimingController = controller;
			aimingController.Initialize(ref controlReferences);
		}

		void LateUpdate()
		{
			// Call on each controller (camera, steering, and aiming) to do their thing.

			// Camera
			// Transition between the previous and current camera controllers if within transition transition time.
			if(Time.time <= cameraTransitionEndTime && previousCameraController && cameraController)
			{
				// Get previous and new CameraController inputs
				CameraInput previousCameraInput = previousCameraController.UpdateCamera(ref controlReferences);
				CameraInput newCameraInput = cameraController.UpdateCamera(ref controlReferences);

				// Set FOV to default if not set by controller to prepare for lerp.
				if(!previousCameraInput.fov.HasValue) previousCameraInput.fov = defaultCameraFOV;
				if(!newCameraInput.fov.HasValue) newCameraInput.fov = defaultCameraFOV;

				// Calculate ratio from start to end transition time
				float t = Mathf.InverseLerp(cameraTransitionEndTime - cameraTransitionTime, cameraTransitionEndTime, Time.time);
				// Smooth out ratio value
				t = Mathf.SmoothStep(0, 1, Mathf.SmoothStep(0, 1, t));
				// Lerp between them
				cameraInput = CameraInput.Lerp(previousCameraInput, newCameraInput, t);
			}
			else
			{
				// Exit the previous camera.
				if(previousCameraController)
				{
					previousCameraController.Exit(ref controlReferences);
					previousCameraController = null;
				}

				// Update the camera with the current controller.
				if(cameraController) cameraInput = cameraController.UpdateCamera(ref controlReferences);
			}

			// Update rig position/rotation with camera controller input.
			transform.position = cameraInput.position;
			transform.rotation = cameraInput.rotation;
			// Update FOV
			if(driverCamera) driverCamera.fieldOfView = cameraInput.fov.HasValue ? cameraInput.fov.Value : defaultCameraFOV;

			// Steering
			if(steeringController) vehicleInput = steeringController.UpdateSteering(ref controlReferences);

			if(vehicle)
			{
				// Provide input to vehicle and turrets.
				vehicle.input = vehicleInput;

				// Aiming
				if(aimingController) turretInput = aimingController.UpdateAiming(ref controlReferences);

				foreach(Turret turret in turrets)
				{
					turret.input = turretInput;
				}
			}
		}
	}
}
