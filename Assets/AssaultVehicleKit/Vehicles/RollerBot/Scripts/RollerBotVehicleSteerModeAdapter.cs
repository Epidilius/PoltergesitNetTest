using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hebertsystems.AVK
{
	//  The RollerBotSteerModeAdapter modifies the steering characteristics of a
	//  RollerBotVehicle based on the current SteeringController being used to optimize handling.
	//
	//  The original values of the RollerBotVehicle will be saved as the default values
	//  and used if there is no matching SteeringController characteristics specified.
	//
	//  The SteeringController must be referenced by name since prefabs cannot save references
	//  to scene objects (and the RollerBotVehicle this is attached to will be a prefab).
	//
	[RequireComponent(typeof(RollerBotVehicle))]
	public class RollerBotVehicleSteerModeAdapter : MonoBehaviour
	{
		// Custom class used for specifying steering characteristics for corresponding SteeringControllers.
		[System.Serializable]
		public class SteerCharacteristics
		{
			public string steeringControllerName;						// The name of the SteeringController.
			
			public float turnSmoothTime = .2f;							// Turn dampening smooth time for changing direction.
		}
		
		public SteerCharacteristics[] steerCharacteristics;				// Array specifying steer characteristics for individual SteeringControllers.
		
		private RollerBotVehicle rollerBotVehicle;
		private SteerCharacteristics defaultCharacteristics = new SteerCharacteristics();
		private Dictionary<string, SteerCharacteristics> steerCharacteristicDictionary = new Dictionary<string, SteerCharacteristics>();
		
		void Awake() 
		{
			// Subscribe to setSteeringController event.
			Events.setSteeringController += OnSetSteeringController;
			
			// Obtain RollerBotVehicle reference.
			rollerBotVehicle = GetComponentInChildren<RollerBotVehicle>();
			if(!rollerBotVehicle) 
			{
				Debug.LogWarning("Unable to find required RollerBotVehicle for RollerBotVehicleSteerModeAdapter on " + name);
				return;
			}

			// Go through all SteerCharacteristics and store names in a dictionary for easy lookup.
			foreach(SteerCharacteristics sc in steerCharacteristics)
			{
				steerCharacteristicDictionary.Add(sc.steeringControllerName, sc);
			}
			
			// Save original values as defaults.
			defaultCharacteristics.turnSmoothTime = rollerBotVehicle.turnSmoothTime;

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
			
			// Set RollerBotVehicle steering
			if(rollerBotVehicle)
			{
				rollerBotVehicle.turnSmoothTime = characteristics.turnSmoothTime;
			}
		}
		
		void OnDestroy () 
		{
			// Unsubscribe from event.
			Events.setSteeringController -= OnSetSteeringController;
		}
	}
}
