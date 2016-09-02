//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2015 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class RCCDashboardInputs : MonoBehaviour {

	public UIType _UIType;
	public enum UIType {UI, NGUI}

	public GameObject RPMNeedle;
	public GameObject KMHNeedle;

	internal RectTransform RPMNeedleUI;
	internal RectTransform KMHNeedleUI;

	internal GameObject RPMNeedleNGUI;
	internal GameObject KMHNeedleNGUI;

	internal float RPM;
	internal float KMH;
	internal float Gear;

	public void GetNeedles(){
		
		if(_UIType == UIType.UI){
			RPMNeedleUI = RPMNeedle.GetComponent<RectTransform>();
			KMHNeedleUI = KMHNeedle.GetComponent<RectTransform>();
		}else{
			RPMNeedleNGUI = RPMNeedle;
			KMHNeedleNGUI = KMHNeedle;
		}
		
	}

}



