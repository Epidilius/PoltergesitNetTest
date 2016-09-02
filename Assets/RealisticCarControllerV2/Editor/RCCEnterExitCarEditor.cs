//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2015 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections;

public class RCCEnterExitCarEditor : Editor {
	
	[MenuItem("BoneCracker Games/Realistic Car Controller/Enter-Exit/Add Enter-Exit Script to Vehicle")]
	static void CreateEnterExitVehicleBehavior(){
		
		if(!Selection.activeGameObject.GetComponent<RCCEnterExitCar>() && Selection.activeGameObject.GetComponent<RCCCarControllerV2>()){
			Selection.activeGameObject.AddComponent<RCCEnterExitCar>();
		}else if(Selection.activeGameObject.GetComponent<RCCCarControllerV2>()){	
			EditorUtility.DisplayDialog("Your Vehicle Already Has Enter-Exit Script", "Your Vehicle Already Has Enter-Exit Script", "Ok");
		}else if(!Selection.activeGameObject.GetComponent<RCCCarControllerV2>()){
			EditorUtility.DisplayDialog("Your Vehicle Has Not RCCCarControllerV2", "Your Vehicle Has Not RCCCarControllerV2.", "Ok");
		}
		
	}

	[MenuItem("BoneCracker Games/Realistic Car Controller/Enter-Exit/Add Enter-Exit Script to FPS Player")]
	static void CreateEnterExitPlayerBehavior(){
		
		if(!Selection.activeGameObject.GetComponent<RCCEnterExitPlayer>()){
			Selection.activeGameObject.AddComponent<RCCEnterExitPlayer>();
		}else{	
			EditorUtility.DisplayDialog("Your Player Already Has Enter-Exit Script", "Your Player Already Has Enter-Exit Script", "Ok");
		}
		
	}
	
}
