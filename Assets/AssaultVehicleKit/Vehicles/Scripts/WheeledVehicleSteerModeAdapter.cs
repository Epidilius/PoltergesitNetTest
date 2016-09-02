using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hebertsystems.AVK
{
	//  The WheeledVehicleSteerModeAdapter modifies the steering characteristics of a
	//  WheeledVehicle based on the current SteeringController being used to optimize handling.
	//
	//  The original values of the WheeledVehicle will be saved as the default values
	//  and used if there is no matching SteeringController characteristics specified.
	//
	//  The SteeringController must be referenced by name since prefabs cannot save references
	//  to scene objects (and the WheeledVehicle this is attached to will be a prefab).
	//
	[RequireComponent(typeof(WheeledVehicle))]
	public class WheeledVehicleSteerModeAdapter : MonoBehaviour
	{
		// Custom class used for specifying steering characteristics for corresponding SteeringControllers.
		[System.Serializable]
		public class SteerCharacteristics
		{
			public string steeringControllerName;						// The name of the SteeringController.

			public float maxTurnSpeed = 90;								// The maximum turn speed (angles per sec) of the steering system.
			public float maxTurnSpeedAtMax = 90;						// The maximum turn speed at vehicle max speed.
			
			public float maxTurnAngle = 30;								// The maximum turn angle.
			public float maxTurnAngleAtMax = 30;						// The maximum turn angle at vehicle max speed.
		}

		public SteerCharacteristics[] steerCharacteristics;				// Array specifying steer characteristics for individual SteeringControllers.

		private WheeledVehicle wheeledVehicle;
		private SteerCharacteristics defaultCharacteristics = new SteerCharacteristics();
		private Dictionary<string, SteerCharacteristics> steerCharacteristicDictionary = new Dictionary<string, SteerCharacteristics>();

		void Awake() 
		{
			// Subscribe to setSteeringController event.
			Events.setSteeringController += OnSetSteeringController;

			// Obtain WheeledVehicle reference.
			wheeledVehicle = GetComponentInChildren<WheeledVehicle>();
			if(!wheeledVehicle) 
			{
				Debug.LogWarning("Unable to find required WheeledVehicle for WheeledVehicleSteeringModeAdapter on " + name);
				return;
			}

			// Go through all SteerCharacteristics and store names in a dictionary for easy lookup.
			foreach(SteerCharacteristics sc in steerCharacteristics)
			{
				steerCharacteristicDictionary.Add(sc.steeringControllerName, sc);
			}

			// Save original values as defaults.
			defaultCharacteristics.maxTurnSpeed = wheeledVehicle.maxTurnSpeed;
			defaultCharacteristics.maxTurnSpeedAtMax = wheeledVehicle.maxTurnSpeedAtMax;
			defaultCharacteristics.maxTurnAngle = wheeledVehicle.maxTurnAngle;
			defaultCharacteristics.maxTurnAngleAtMax = wheeledVehicle.maxTurnAngleAtMax;

			// Init current controller
			if(Events.currentSteeringController) OnSetSteeringController(Events.currentSteeringController);
		}

		void OnSetSteeringController(SteeringController steeringController)
		{
			// Assume default values initially.
			SteerCharacteristics characteristics = defaultCharacteristics;

			// Search for the new SteeringController in specified values.
			if(steerCharacteristicDictionary.ContainsKey(steeringController.name)) 
				characteristics = steerCharacteristicDictionary[steeringController.name];

			// Set WheeledVehicle steering
			if(wheeledVehicle)
			{
				wheeledVehicle.maxTurnSpeed = characteristics.maxTurnSpeed;
				wheeledVehicle.maxTurnSpeedAtMax = characteristics.maxTurnSpeedAtMax;
				wheeledVehicle.maxTurnAngle = characteristics.maxTurnAngle;
				wheeledVehicle.maxTurnAngleAtMax = characteristics.maxTurnAngleAtMax;
			}
		}

		void OnDestroy () 
		{
			// Unsubscribe from event.
			Events.setSteeringController -= OnSetSteeringController;
		}
	}
}
