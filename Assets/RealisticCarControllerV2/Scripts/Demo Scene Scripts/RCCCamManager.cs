//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2015 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class RCCCamManager: MonoBehaviour {

	private RCCCarCamera carCamera;
	private RCCCameraOrbit orbitScript;
	private RCCCockpitCamera cockpitCamera;
	private RCCWheelCamera wheelCamera;

	public bool useOrbitCamera = false;
	public bool useFixedCamera = false;

	internal float dist = 10f;
	internal float height = 5f;
	internal int cameraChangeCount = 0;
	internal Transform target;

	void Start () {

		cameraChangeCount = 0;

		carCamera = GetComponent<RCCCarCamera>();
		target = carCamera.playerCar;

		if(GetComponent<RCCCameraOrbit>())
			orbitScript = GetComponent<RCCCameraOrbit>();
		else
			orbitScript = gameObject.AddComponent<RCCCameraOrbit>();

	}

	void Update () {

		if(Input.GetKeyDown(KeyCode.C))
			ChangeCamera();

	}

	public void ChangeCamera(){

		if(!target)
			return;

		cameraChangeCount++;
		if(cameraChangeCount >= 5)
			cameraChangeCount = 0;

		if(target.GetComponent<RCCCarCameraConfig>()){
			dist = target.GetComponent<RCCCarCameraConfig>().distance;
			height = target.GetComponent<RCCCarCameraConfig>().height;
			carCamera.distance = dist;
			carCamera.height = height;
		}

		if(useOrbitCamera){
			orbitScript.target = target.transform;
			orbitScript.distance = dist;
		}

		if(target.GetComponentInChildren<RCCCockpitCamera>())
			cockpitCamera = target.GetComponentInChildren<RCCCockpitCamera>();
		if(target.GetComponentInChildren<RCCWheelCamera>())
			wheelCamera = target.GetComponentInChildren<RCCWheelCamera>();

		switch(cameraChangeCount){

		case 0:
			orbitScript.enabled = false;
			carCamera.enabled = true;
			carCamera.transform.parent = null;
			break;
		case 1:
			if(!useOrbitCamera){
				ChangeCamera();
				break;
			}
			orbitScript.enabled = true;
			carCamera.enabled = false;
			carCamera.transform.parent = null;
			break;
		case 2:
			if(!useFixedCamera){
				ChangeCamera();
				break;
			}
			orbitScript.enabled = false;
			carCamera.enabled = false;
			carCamera.transform.parent = null;
			break;
		case 3:
			orbitScript.enabled = false;
			carCamera.enabled = false;
			carCamera.transform.parent = target;
			if(!cockpitCamera){
				ChangeCamera();
				break;
			}
			carCamera.transform.localPosition = cockpitCamera.transform.localPosition;
			carCamera.transform.localRotation = cockpitCamera.transform.localRotation;
			carCamera.GetComponent<Camera>().fieldOfView = 60;
			break;
		case 4:
			orbitScript.enabled = false;
			carCamera.enabled = false;
			carCamera.transform.parent = target;
			if(!wheelCamera){
				ChangeCamera();
				break;
			}
			carCamera.transform.localPosition = wheelCamera.transform.localPosition;
			carCamera.transform.localRotation = wheelCamera.transform.localRotation;
			carCamera.GetComponent<Camera>().fieldOfView = 60; 
			break;
		case 5:
			orbitScript.enabled = false;
			carCamera.enabled = false;
			carCamera.transform.parent = null;
			break;
		
		}

	}

}
