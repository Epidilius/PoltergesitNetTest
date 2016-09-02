//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2015 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System;
using System.Collections;

public class RCCCarChange : MonoBehaviour {
	
	private GameObject[] objects;
	private GameObject activeObject;
	private int activeObjectIdx;
	private Camera mainCamera;
	private bool selectScreen = true;
	
	public Vector3 cameraOffset = new Vector3(0.0f, 1.0f, 0.0f);

	void Awake () {

		RCCCarControllerV2[] vehicles = GameObject.FindObjectsOfType<RCCCarControllerV2>();
		objects = new GameObject[vehicles.Length];

		for(int i = 0; i < vehicles.Length; i++){
			objects[i] = vehicles[i].gameObject;
		}

		foreach(GameObject controller in objects){
			controller.GetComponent<RCCCarControllerV2>().canControl = false;
			controller.GetComponent<RCCCarControllerV2>().runEngineAtAwake = false;
			controller.GetComponent<RCCCarControllerV2>().engineRunning = false;
		}

		mainCamera = Camera.main;

	}

	void Update () {

		if(selectScreen)
			mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, objects[activeObjectIdx].transform.position + (-mainCamera.transform.forward * 15f) + cameraOffset, Time.deltaTime * 5f);

	}
	
	void OnGUI()
	{

		if(selectScreen){

			GUIStyle centeredStyle = GUI.skin.GetStyle("Button");
			centeredStyle.alignment = TextAnchor.MiddleCenter;

			// Next
			if( GUI.Button(new Rect(Screen.width/2 + 65, Screen.height-100, 120, 50), "Next") )
			{
				activeObjectIdx++;
				if( activeObjectIdx >= objects.Length )
					activeObjectIdx = 0;
			}	
			
			// Previous
			if( GUI.Button(new Rect(Screen.width / 2 - 185, Screen.height-100, 120, 50), "Previous") )
			{
				activeObjectIdx--;
				if( activeObjectIdx < 0 )
					activeObjectIdx = objects.Length - 1;
			}

			// Select Car
			if( GUI.Button(new Rect(Screen.width / 2 - 60, Screen.height-100, 120, 50), "Select") )
			{
				selectScreen = false;
				objects[activeObjectIdx].GetComponent<RCCCarControllerV2>().canControl = true;
				objects[activeObjectIdx].GetComponent<RCCCarControllerV2>().KillOrStartEngine();
				GetComponent<RCCCamManager>().enabled = true;
				GetComponent<RCCCamManager>().target = objects[activeObjectIdx].transform;
				GetComponent<RCCCarCamera>().playerCar = objects[activeObjectIdx].transform;
				GetComponent<RCCCamManager>().cameraChangeCount = 5;
				GetComponent<RCCCamManager>().ChangeCamera();
			}

		}else{

			if( GUI.Button(new Rect(Screen.width - 270, 350, 240, 50), "Select Screen") )
			{
				selectScreen = true;
				objects[activeObjectIdx].GetComponent<RCCCarControllerV2>().canControl = false;
				objects[activeObjectIdx].GetComponent<RCCCarControllerV2>().engineRunning = false;
				GetComponent<RCCCamManager>().enabled = false;
				GetComponent<RCCCarCamera>().enabled = false;
				GetComponent<RCCCameraOrbit>().enabled = false;
				mainCamera.transform.rotation = Quaternion.Euler(mainCamera.transform.rotation.x, 140, mainCamera.transform.rotation.z);
			}

		}

	}

}
