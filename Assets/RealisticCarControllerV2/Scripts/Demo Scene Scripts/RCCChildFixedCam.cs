//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2015 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class RCCChildFixedCam : MonoBehaviour {

	[HideInInspector]
	public Transform player;

	void Update () {
		transform.LookAt(player);
	}

}
