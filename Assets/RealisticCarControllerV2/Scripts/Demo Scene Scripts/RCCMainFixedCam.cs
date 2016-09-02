//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2015 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RCCMainFixedCam : MonoBehaviour {
	
	private Camera[] cams;
	private AudioListener[] listeners;
	private RCCChildFixedCam[] childCams;
	private RCCCamManager switcher;
	private float[] distances;
	private int totalActiveCams;
	public Transform player;
	public Camera mainCamera;
	public AudioListener mainListener;
	public bool canTrackNow = false;
	public float distanceLimit = 100f;
	public float minimumFOV = 20;
	public float maximumFOV = 60;
	public bool drawGizmos = true;


	void Start () {

		cams = GetComponentsInChildren<Camera>();
		listeners = GetComponentsInChildren<AudioListener>();
		childCams = GetComponentsInChildren<RCCChildFixedCam>();
		switcher = mainCamera.GetComponent<RCCCamManager>();
		mainListener = mainCamera.GetComponent<AudioListener>();
		distances = new float[cams.Length];

		foreach(Camera c in cams){
			c.enabled = false;
		}
		foreach(AudioListener l in listeners){
			l.enabled = false;
		}
		foreach(RCCChildFixedCam l in childCams){
			l.enabled = false;
			l.player = player;
		}

	}

	void Act(){
		foreach(RCCChildFixedCam l in childCams){
			l.enabled = false;
			l.player = player;
		}
	}

	void Update(){

		if(canTrackNow){
			Tracking ();
		}else{

			mainCamera.enabled = true;
			mainListener.enabled = true;

			for(int i = 0; i < cams.Length; i ++){
				if(cams[i].enabled != false)
					cams[i].enabled = false;
				if(listeners[i].enabled != false)
					listeners[i].enabled = false;
				if(childCams[i].enabled != false)
				childCams[i].enabled = false;
			}

			totalActiveCams = 0;

		}

		foreach(RCCChildFixedCam l in childCams){
			if(l.player != player)
				l.player = player;
		}

	}

	void Tracking () {
	
		for(int i = 0; i < cams.Length; i ++){

			if(i == 0)
				totalActiveCams = 0;

			distances[i] = Vector3.Distance(cams[i].transform.position, player.transform.position);
			
			if(distances[i] <= distanceLimit && totalActiveCams < 1){

				if(cams[i].enabled != true)
					cams[i].enabled = true;
				if(listeners[i].enabled != true)
					listeners[i].enabled = true;
				if(childCams[i].enabled != true)
					childCams[i].enabled = true;

				cams[i].fieldOfView = Mathf.Lerp (cams[i].fieldOfView, Mathf.Lerp (maximumFOV, minimumFOV, ((distances[i] * 2) / distanceLimit)), Time.deltaTime * 2);
				if(mainListener.enabled != false)
					mainListener.enabled = false;
				if(mainCamera.enabled != false)
					mainCamera.enabled = false;
				totalActiveCams ++;

			}else{

				if(cams[i].enabled != false)
					cams[i].enabled = false;
				if(listeners[i].enabled != false)
					listeners[i].enabled = false;
				if(childCams[i].enabled != false)
					childCams[i].enabled = false;

				childCams[i].transform.rotation = Quaternion.identity;
				
			}

		}

		if(totalActiveCams < 1){
			if(!mainCamera.enabled)
				mainCamera.enabled = true;
			if(mainListener.enabled == false)
				mainListener.enabled = true;
			switcher.cameraChangeCount = 0;
		}

	}

	void OnDrawGizmos() {

		if(drawGizmos){
			cams = GetComponentsInChildren<Camera>();
			for(int i = 0; i < cams.Length; i ++){
				Gizmos.matrix = cams[i].transform.localToWorldMatrix;
				Gizmos.color = new Color(1, 0, 0, 0.5F);
				Gizmos.DrawCube(Vector3.zero,new Vector3(distanceLimit * 2, distanceLimit / 2, distanceLimit * 2));
			}
		}
	}
	
}
