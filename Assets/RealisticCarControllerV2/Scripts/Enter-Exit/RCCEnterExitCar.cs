//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2015 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class RCCEnterExitCar : MonoBehaviour {

	private GameObject carCamera;
	private GameObject player;
	public Transform getOutPosition;

	private bool  opened = false;
	private float waitTime = 1f;
	private bool  temp = false;
	
	void Start (){

		carCamera = GameObject.FindObjectOfType<RCCCarCamera>().gameObject;
		carCamera.GetComponent<Camera>().enabled = false;
		carCamera.GetComponent<AudioListener>().enabled = false;

		GetComponent<RCCCarControllerV2>().canControl = false;

		if(!getOutPosition){
			GameObject getOutPos = new GameObject("Get Out Position");
			getOutPos.transform.SetParent(transform);
			getOutPos.transform.localPosition = new Vector3(-3f, 0f, 0f);
			getOutPos.transform.localRotation = Quaternion.identity;
			getOutPosition = getOutPos.transform;
		}

		if(GetComponent<RCCCarCameraConfig>())
			GetComponent<RCCCarCameraConfig>().enabled = false;

	}
	
	void Update (){

		if((Input.GetKeyDown(KeyCode.E)) && opened && !temp){
			GetOut();
			opened = false;
			temp = false;
		}

	}
	
	IEnumerator Act (GameObject fpsplayer){

		player = fpsplayer;

		if (!opened && !temp){
			GetIn();
			opened = true;
			temp = true;
			yield return new WaitForSeconds(waitTime);
			temp = false;
		}

	}
	
	void GetIn (){

		carCamera.transform.GetComponent<RCCCarCamera>().playerCar = transform;
		player.transform.SetParent(transform);
		player.transform.localPosition = Vector3.zero;
		player.transform.localRotation = Quaternion.identity;
		player.SetActive(false);
		carCamera.GetComponent<Camera>().enabled = true;
		if(GetComponent<RCCCarCameraConfig>())
			GetComponent<RCCCarCameraConfig>().enabled = true;
		GetComponent<RCCCarControllerV2>().canControl = true;
		carCamera.GetComponent<AudioListener>().enabled = true;
		SendMessage("KillOrStartEngine");

	}
	
	void GetOut (){

		player.transform.SetParent(null);
		player.transform.position = getOutPosition.position;
		player.transform.rotation = getOutPosition.rotation;
		player.SetActive(true);
		carCamera.GetComponent<Camera>().enabled = false;
		if(GetComponent<RCCCarCameraConfig>())
			GetComponent<RCCCarCameraConfig>().enabled = false;
		carCamera.GetComponent<AudioListener>().enabled = false;
		GetComponent<RCCCarControllerV2>().canControl = false;
		SendMessage("KillOrStartEngine");

	}
	
}