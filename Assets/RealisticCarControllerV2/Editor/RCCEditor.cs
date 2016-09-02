using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RCCCarControllerV2)), CanEditMultipleObjects]
public class RCCEditor : Editor {

	RCCCarControllerV2 carScript;
	
	bool nguiMobileController = false;
	static bool firstInit = false;
	
	Texture2D wheelIcon;
	Texture2D steerIcon;
	Texture2D configIcon;
	Texture2D lightIcon;
	Texture2D mobileIcon;
	Texture2D soundIcon;
	Texture2D gaugeIcon;
	Texture2D smokeIcon;
	Texture2D damageIcon;
	
	bool WheelSettings;
	bool SteerSettings;
	bool Configurations;
	bool LightSettings;
	bool SoundSettings;
	bool MobileSettings;
	bool DashBoardSettings;
	bool SmokeSettings;
	bool DamageSettings;

	Color defBackgroundColor;

	[MenuItem("BoneCracker Games/Realistic Car Controller/Add Main Controller To Vehicle")]
	static void CreateBehavior(){

		if(!Selection.activeGameObject.GetComponent<RCCCarControllerV2>()){

			Selection.activeGameObject.AddComponent<RCCCarControllerV2>();

			Selection.activeGameObject.GetComponent<Rigidbody>().mass = 1500f;
			Selection.activeGameObject.GetComponent<Rigidbody>().angularDrag = .5f;
			Selection.activeGameObject.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

			if(EditorUtility.DisplayDialog("Build Your Car With Default Prefabs and Settings", "Would You Like To Build Your Car With Default Prefabs and Settings?", "Yes", "No"))
				firstInit = true;

		}else{

			EditorUtility.DisplayDialog("Your Gameobject Already Has Realistic Car ControllerV2", "Your Gameobject Already Has Realistic Car ControllerV2", "Ok");

		}
		
	}
	
	void Awake(){

		wheelIcon = Resources.Load("WheelIcon", typeof(Texture2D)) as Texture2D;
		steerIcon = Resources.Load("SteerIcon", typeof(Texture2D)) as Texture2D;
		configIcon = Resources.Load("ConfigIcon", typeof(Texture2D)) as Texture2D;
		lightIcon = Resources.Load("LightIcon", typeof(Texture2D)) as Texture2D;
		mobileIcon = Resources.Load("MobileIcon", typeof(Texture2D)) as Texture2D;
		soundIcon = Resources.Load("SoundIcon", typeof(Texture2D)) as Texture2D;
		gaugeIcon = Resources.Load("GaugeIcon", typeof(Texture2D)) as Texture2D;
		smokeIcon = Resources.Load("SmokeIcon", typeof(Texture2D)) as Texture2D;
		damageIcon = Resources.Load("DamageIcon", typeof(Texture2D)) as Texture2D;
		
	}
	
	public override void OnInspectorGUI () {

		serializedObject.Update();

		carScript = (RCCCarControllerV2)target;
		defBackgroundColor = GUI.backgroundColor;

		if(!carScript.GetComponent<RCCAICarController>())
			carScript.AIController = false;

		if(firstInit){
			SetDefaultSettings();
			SetDefaultPrefabs();
		}
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		EditorGUILayout.BeginHorizontal();
		
		if(WheelSettings)
			GUI.backgroundColor = Color.gray;
		else GUI.backgroundColor = defBackgroundColor;
		
		if(GUILayout.Button(wheelIcon))
			if(!WheelSettings)	WheelSettings = true;
		else WheelSettings = false;
		
		if(SteerSettings)
			GUI.backgroundColor = Color.gray;
		else GUI.backgroundColor = defBackgroundColor;
		
		if(GUILayout.Button(steerIcon))
			if(!SteerSettings)	SteerSettings = true;
		else SteerSettings = false;
		
		if(Configurations)
			GUI.backgroundColor = Color.gray;
		else GUI.backgroundColor = defBackgroundColor;
		
		if(GUILayout.Button(configIcon))
			if(!Configurations)	Configurations = true;
		else Configurations = false;
		
		if(LightSettings)
			GUI.backgroundColor = Color.gray;
		else GUI.backgroundColor = defBackgroundColor;
		
		if(GUILayout.Button(lightIcon))
			if(!LightSettings)	LightSettings = true;
		else LightSettings = false;
		
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		
		if(SoundSettings)
			GUI.backgroundColor = Color.gray;
		else GUI.backgroundColor = defBackgroundColor;
		
		if(GUILayout.Button(soundIcon))
			if(!SoundSettings)	SoundSettings = true;
		else SoundSettings = false;

		if(!carScript.GetComponent<RCCAICarController>()){
			if(MobileSettings)
				GUI.backgroundColor = Color.gray;
			else GUI.backgroundColor = defBackgroundColor;
		}else{
			GUI.backgroundColor = Color.red;
		}
		
		if(GUILayout.Button(mobileIcon))
			if(!MobileSettings)	MobileSettings = true;
		else MobileSettings = false;
		
		if(DashBoardSettings)
			GUI.backgroundColor = Color.gray;
		else GUI.backgroundColor = defBackgroundColor;
		
		if(GUILayout.Button(gaugeIcon))
			if(!DashBoardSettings)	DashBoardSettings = true;
		else DashBoardSettings = false;
		
		if(SmokeSettings)
			GUI.backgroundColor = Color.gray;
		else GUI.backgroundColor = defBackgroundColor;
		
		if(GUILayout.Button(smokeIcon))
			if(!SmokeSettings)	SmokeSettings = true;
		else SmokeSettings = false;

		if(DamageSettings)
			GUI.backgroundColor = Color.gray;
		else GUI.backgroundColor = defBackgroundColor;

		if(GUILayout.Button(damageIcon))
			if(!DamageSettings)	DamageSettings = true;
		else DamageSettings = false;

		GUI.backgroundColor = defBackgroundColor;
		EditorGUILayout.EndHorizontal();
		
		if(MobileSettings){

			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Mobile Settings", MessageType.None);
			GUI.color = defBackgroundColor;
			EditorGUILayout.Space();

			if(!carScript.GetComponent<RCCAICarController>()){

				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(serializedObject.FindProperty("mobileController"), new GUIContent("Mobile Controller", "Enables mobile controller."), false);

				if(carScript.mobileController){

					EditorGUILayout.PropertyField(serializedObject.FindProperty("_mobileControllerType"), new GUIContent("Mobile Controller Type", "Unity UI controller, or NGUI controller?"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("useAccelerometerForSteer"), new GUIContent("Use Accelerometer For Steering", "Uses device accelerometer for steering."), false);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringWheelControl"), new GUIContent("Steering Wheel Control", "Enables touchable steering wheel GUI."), false);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gyroTiltMultiplier"), new GUIContent("Gyro Sensitivity", "Gyro Sensitivity."), false);
					
					if(carScript._mobileControllerType == RCCCarControllerV2.MobileGUIType.NGUIController)
						nguiMobileController = true;
					else
						nguiMobileController = false;
					
					if(nguiMobileController){
						
						EditorGUILayout.Space();
						GUI.color = Color.cyan;
						EditorGUILayout.HelpBox("NGUI Elements", MessageType.None);
						GUI.color = defBackgroundColor;
						EditorGUILayout.Space();

						EditorGUILayout.PropertyField(serializedObject.FindProperty("autoFindButtons"), new GUIContent("Auto Find Mobile Buttons", "Finds all buttons when new vehicle spawned. Useful for spawning new vehicles on your scene at runtime."));

						if(!carScript.autoFindButtons){

							EditorGUILayout.PropertyField(serializedObject.FindProperty("gasPedal"),false);
							EditorGUILayout.PropertyField(serializedObject.FindProperty("brakePedal"),false);
							EditorGUILayout.PropertyField(serializedObject.FindProperty("leftArrow"),false);
							EditorGUILayout.PropertyField(serializedObject.FindProperty("rightArrow"),false);
							EditorGUILayout.PropertyField(serializedObject.FindProperty("handbrakeGui"),false);
							EditorGUILayout.PropertyField(serializedObject.FindProperty("boostGui"), new GUIContent("Boost UI", "This UI is optional. If boost UI is selected, will multiply default engine torque by 1.5"), false);

						}else{

							if(!GameObject.FindObjectOfType<RCCMobileButtons>())
								EditorGUILayout.HelpBox("Ensure that your UI Canvas has ''RCCMobileButtons'' script.", MessageType.Error);

						}

						EditorGUILayout.Space();
						
					}else{
						
						EditorGUILayout.Space();
						GUI.color = Color.cyan;
						EditorGUILayout.HelpBox("Unity UI Elements", MessageType.None);
						GUI.color = defBackgroundColor;
						EditorGUILayout.Space();

						EditorGUILayout.PropertyField(serializedObject.FindProperty("autoFindButtons"), new GUIContent("Auto Find Mobile Buttons", "Finds all buttons when new vehicle spawned. Useful for spawning new vehicles on your scene at runtime."));

						if(!carScript.autoFindButtons){

							EditorGUILayout.PropertyField(serializedObject.FindProperty("gasPedalUI"),false);
							EditorGUILayout.PropertyField(serializedObject.FindProperty("brakePedalUI"),false);
							EditorGUILayout.PropertyField(serializedObject.FindProperty("leftArrowUI"),false);
							EditorGUILayout.PropertyField(serializedObject.FindProperty("rightArrowUI"),false);
							EditorGUILayout.PropertyField(serializedObject.FindProperty("handbrakeUI"),false);
							EditorGUILayout.PropertyField(serializedObject.FindProperty("boostUI"), new GUIContent("Boost UI", "This UI is optional. If boost UI is selected, will multiply default engine torque by 1.5"), false);

						}else{

							if(!GameObject.FindObjectOfType<RCCMobileButtons>())
								EditorGUILayout.HelpBox("Ensure that your UI Canvas has ''RCCMobileButtons'' script.", MessageType.Error);

						}

						EditorGUILayout.Space();
						
					}
					
					if(carScript.steeringWheelControl){
						
						EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringWheelMaximumsteerAngle"),false);
						EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringWheelXOffset"),false);
						EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringWheelYOffset"),false);
						EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringWheelGuiScale"),false);
						EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringWheelPivotPos"),false);
						EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringWheelResetPosspeed"),false);
						EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringWheelTexture"),false);
						
					}
					
				}

			}else{

				EditorGUILayout.HelpBox("AI Controller Enabled! Mobile Controllers and Keyboard Controllers are invalid.", MessageType.Error);

				if(carScript.mobileController)
					carScript.mobileController = false;

			}

			EditorGUILayout.Space();
			
		}
		
		if(WheelSettings){
			
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Wheel Settings", MessageType.None);
			GUI.color = defBackgroundColor;
			EditorGUILayout.Space();

			EditorGUILayout.Space();
			if(GUILayout.Button("Create Necessary Gameobject Groups")){
				
				Transform[] objects = carScript.gameObject.GetComponentsInChildren<Transform>();
				bool didWeHaveThisObject = false;
				
				foreach(Transform g in objects){
					if(g.name == "Chassis")
						didWeHaveThisObject = true;
				}
				
				if(!didWeHaveThisObject){
					
					GameObject chassis = new GameObject("Chassis");
					chassis.transform.parent = carScript.transform;
					chassis.transform.localPosition = Vector3.zero;
					chassis.transform.localScale = Vector3.one;
					chassis.transform.rotation = carScript.transform.rotation;
					carScript.chassis = chassis;
					GameObject wheelTransforms = new GameObject("Wheel Transforms");
					wheelTransforms.transform.parent = chassis.transform;
					wheelTransforms.transform.localPosition = Vector3.zero;
					wheelTransforms.transform.localScale = Vector3.one;
					wheelTransforms.transform.rotation = carScript.transform.rotation;
					GameObject COM = new GameObject("COM");
					COM.transform.parent = carScript.transform;
					COM.transform.localPosition = Vector3.zero;
					COM.transform.localScale = Vector3.one;
					COM.transform.rotation = carScript.transform.rotation;
					carScript.COM = COM.transform;
					
				}else{
					
					Debug.LogError("Your Vehicle has these groups already!");
					
				}
				
			}
			
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("FrontLeftWheelTransform"), new GUIContent("Front Left Wheel", "Select front left wheel of your car."), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("FrontRightWheelTransform"), new GUIContent("Front Right Wheel", "Select front right wheel of your car."), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("RearLeftWheelTransform"), new GUIContent("Rear Left Wheel", "Select rear left wheel of your car."), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("RearRightWheelTransform"), new GUIContent("Rear Right Wheel", "Select rear right wheel of your car."), false);
			EditorGUILayout.Space();
			
			if(GUILayout.Button("Create Wheel Colliders")){
				
				WheelCollider[] wheelColliders = carScript.gameObject.GetComponentsInChildren<WheelCollider>();
				
				if(wheelColliders.Length >= 1)
					Debug.LogError("Your Vehicle has Wheel Colliders already!");
				else
					carScript.CreateWheelColliders();
				
			}
			
			if(carScript.FrontLeftWheelTransform == null || carScript.FrontRightWheelTransform == null || carScript.RearLeftWheelTransform == null || carScript.RearRightWheelTransform == null  )
				EditorGUILayout.HelpBox("Select all of your Wheel Transforms before creating Wheel Colliders", MessageType.Warning);
			
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("FrontLeftWheelCollider"), new GUIContent("Front Left WheelCollider", "WheelColliders are generated when you click ''Create WheelColliders'' button. But if you want to create your WheelCollider yourself, select corresponding WheelCollider for each wheel."), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("FrontRightWheelCollider"), new GUIContent("Front Right WheelCollider", "WheelColliders are generated when you click ''Create WheelColliders'' button. But if you want to create your WheelCollider yourself, select corresponding WheelCollider for each wheel."), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("RearLeftWheelCollider"), new GUIContent("Rear Left WheelCollider", "WheelColliders are generated when you click ''Create WheelColliders'' button. But if you want to create your WheelCollider yourself, select corresponding WheelCollider for each wheel."), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("RearRightWheelCollider"), new GUIContent("Rear Right WheelCollider", "WheelColliders are generated when you click ''Create WheelColliders'' button. But if you want to create your WheelCollider yourself, select corresponding WheelCollider for each wheel."), false);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ExtraRearWheelsCollider"), new GUIContent("Extra Rear Wheel Collider", "In case of if your vehicle has extra wheels."), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ExtraRearWheelsTransform"), new GUIContent("Extra Rear Wheel Transform", "In case of if your vehicle has extra wheels."), true);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("applyEngineTorqueToExtraRearWheelColliders"), new GUIContent("Apply Engine Torque To Extra Rear Wheels", "Applies Engine Torque To Extra Rear Wheels."), false);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("SteeringWheel"), new GUIContent("Interior Steering Wheel Model", "In case of if your vehicle has individual steering wheel model in interior."), false);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("UseTerrainSplatMapForGroundPhysic"), new GUIContent("Use Terrain SplatMap For Ground Physic", "Based on your terrain splatmap. Your terrain needs 3 textures at least. First texture will represent ''Asphalt'', second texture ''Sand'', third texture ''Grass''."), false);
			if(carScript.UseTerrainSplatMapForGroundPhysic)
				EditorGUILayout.HelpBox("Based on your terrain splatmap. Your terrain needs 3 textures at least. First texture will represent ''Asphalt'', second texture ''Sand'', third texture ''Grass''.", MessageType.Info);

		}
		
		if(SteerSettings){
			
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Steer Settings", MessageType.None);
			GUI.color = defBackgroundColor;
			EditorGUILayout.Space();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("steerAngle"), new GUIContent("Maximum Steer Angle", "Maximum steer angle for your vehicle."), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("highspeedsteerAngle"), new GUIContent("Maximum Steer Angle At Highest Speed", "Maximum steer angle at highest speed."), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("highspeedsteerAngleAtspeed"), new GUIContent("Steer Angle At Highest Speed", "Steer Angle At Highest Speed."), false);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("antiRoll"), new GUIContent("Anti Roll Force", "Anti Roll Force for preventing flip overs."));
			EditorGUILayout.Space();
			
		}
		
		if(Configurations){
			
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Configurations", MessageType.None);
			GUI.color = defBackgroundColor;
			EditorGUILayout.Space();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("canControl"), new GUIContent("Can Be Controllable Now", "Enables/Disables controlling the vehicle."));
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("runEngineAtAwake"), new GUIContent("Engine Is Running At Awake", "Engine is running defaultly at start of the game."));
			EditorGUILayout.LabelField("Engine Is Running Now", carScript.engineRunning.ToString());
			//EditorGUILayout.PropertyField(serializedObject.FindProperty("engineRunning"), new GUIContent("Engine Running Now", "Engine is running currently."));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("canEngineStall"), new GUIContent("Engine Can Be Stalled?", "Stalled Engine due to low RPM."));
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("autoReverse"), new GUIContent("Auto Reverse", "Enables/Disables auto reversing when player press brake button. Useful for if you are making parking style game."));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("useDifferential"), new GUIContent("Use Differential", "Enables/Disables differential of the vehicle. For more information, google it."), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("automaticGear"), new GUIContent("Automatic Gear Shifting", "Enables/Disables automatic gear shifting of the vehicle."));

			if(!carScript.automaticGear)
				EditorGUILayout.HelpBox("If you want to use Manual Gear, you need to create 2 Inputs.''RCCShiftUp'' and ''RCCShiftDown''. And set which key you want to. You can create new Inputs by Edit --> Project Settings --> Input.", MessageType.Info);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_wheelTypeChoise"));
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("COM"), new GUIContent("Center Of Mass (''COM'')", "Center of Mass of the vehicle. Usually, COM is below around front driver seat."));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("totalGears"), new GUIContent("Total Gears Count", "How Many Gears Your Vehicle Has?"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("currentGear"), new GUIContent("Current Gear"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("engineTorque"), new GUIContent("Maximum Engine Torque"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maxspeed"), new GUIContent("Maximum Speed"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("minEngineRPM"), new GUIContent("Lowest Engine RPM (While Running Engine)"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maxEngineRPM"), new GUIContent("Highest Engine RPM"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("brake"), new GUIContent("Maximum Brake Torque"), false);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ABS"), new GUIContent("ABS"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ASR"), new GUIContent("ASR"), false);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("chassis"), new GUIContent("Chassis", "Script will simulate chassis movement based on vehicle rigidbody situation."), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("chassisVerticalLean"), new GUIContent("Chassis Vertical Lean Sensitivity"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("chassisHorizontalLean"), new GUIContent("Chassis Horizontal Lean Sensitivity"), false);
//			EditorGUILayout.PropertyField(serializedObject.FindProperty("engineTorqueCurve"), true);
//			EditorGUILayout.PropertyField(serializedObject.FindProperty("gearSpeed"), true);
			EditorGUILayout.Space();

		}
		
		if(SoundSettings){
			
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Sound Settings", MessageType.None);
			GUI.color = defBackgroundColor;

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipOn"), new GUIContent("Engine Sound On"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipOff"), new GUIContent("Engine Sound Off"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("engineStartClip"), new GUIContent("Engine Starting Sound", "Optional"), false);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("asphaltSkidClip"), new GUIContent("Tire Skid Sound On Asphalt Surfaces"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("grassSkidClip"), new GUIContent("Tire Skid Sound On Grass Surfaces"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("sandSkidClip"), new GUIContent("Tire Skid Sound On Sand Surfaces"), false);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("crashClips"), new GUIContent("Crashing Sounds"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("gearShiftingClips"), new GUIContent("Gear Shifting Sounds"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("windClip"), new GUIContent("Wind Sound"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("brakeClip"), new GUIContent("Brake Sound"), true);
			EditorGUILayout.Space();
			EditorGUILayout.Slider(serializedObject.FindProperty("minEngineSoundPitch"), .25f, 1f);
			EditorGUILayout.Slider(serializedObject.FindProperty("maxEngineSoundPitch"), 1.25f, 2f);
			EditorGUILayout.Slider(serializedObject.FindProperty("minEngineSoundVolume"), 0f, 1f);
			EditorGUILayout.Slider(serializedObject.FindProperty("maxEngineSoundVolume"), 0f, 1f);
			EditorGUILayout.Space();
			EditorGUILayout.Slider(serializedObject.FindProperty("maxGearShiftingSoundVolume"), 0f, 1f);
			EditorGUILayout.Slider(serializedObject.FindProperty("maxCrashSoundVolume"), 0f, 1f);
			EditorGUILayout.Slider(serializedObject.FindProperty("maxWindSoundVolume"), 0f, 1f);
			EditorGUILayout.Slider(serializedObject.FindProperty("maxBrakeSoundVolume"), 0f, 1f);
			EditorGUILayout.Space();

		}
		
		if(DashBoardSettings){
			
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("DashBoard Settings", MessageType.None);
			GUI.color = defBackgroundColor;
			EditorGUILayout.Space();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("demoGUI"), new GUIContent("Display Information GUI"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("dashBoard"), new GUIContent("Use Dashboard"), false);
			
			if(carScript.dashBoard){
				
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_dashboardType"), false);

				if(!GameObject.FindObjectOfType<RCCDashboardInputs>())
					EditorGUILayout.HelpBox("UI Dashboard Input not found. Ensure that your UI Canvas has ''RCCDashboardInputs'' script.", MessageType.Error);

				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumRPMNeedleAngle"), false);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRPMNeedleAngle"), false);
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumKMHNeedleAngle"), false);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumKMHNeedleAngle"), false);
				
			}

			EditorGUILayout.Space();
		
		}
		
		if(SmokeSettings){
			
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Smoke Settings", MessageType.None);
			GUI.color = defBackgroundColor;
			EditorGUILayout.Space();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("wheelSlipAsphalt"), new GUIContent("Asphalt Wheel Smoke"), false);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("wheelSlipSand"), new GUIContent("Sand Wheel Smoke"), false);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("exhaustGas"), new GUIContent("Exhaust Gas"), true);
			EditorGUILayout.Space();
			
		}
		
		if(LightSettings){
			
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Light Settings", MessageType.None);
			GUI.color = defBackgroundColor;
			EditorGUILayout.Space();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("headLights"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("brakeLights"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("reverseLights"), true);
			EditorGUILayout.Space();
			
		}

		if(DamageSettings){
			
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Damage Settings", MessageType.None);
			GUI.color = defBackgroundColor;
			EditorGUILayout.Space();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("useDamage"), new GUIContent("Use Damage"), false);
			EditorGUILayout.Space();

			if(carScript.useDamage){

				EditorGUILayout.PropertyField(serializedObject.FindProperty("deformableMeshFilters"), new GUIContent("Deformable Mesh Filters", "If no mesh filters selected, will collect all mesh filters in children expect wheel transforms."), true);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("randomizeVertices"), new GUIContent("Randomize Vertices", "Randomizes vertices movement angle."), false);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("damageRadius"), new GUIContent("Damage Radius on Contact", "Damage radius on contact."), false);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumDamage"), new GUIContent("Maximum Damage", "Maximum deformable damage."), false);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("damageMultiplier"), new GUIContent("Damage Multiplier", "Damage mutliplier."), false);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("contactSparkle"), new GUIContent("Contact Sparkle", "Contact sparkle particles"), false);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumContactSparkle"), new GUIContent("Maximum Contact Sparkle Count", "Script is re-using existing collision particles. 3 is good for mobile, 5 and above is good for PC."), false);

				if(carScript.sleep){
					GUI.color = Color.gray;
					GUILayout.Button("Repaired");
				}else{
					GUI.color = Color.green;
					if(GUILayout.Button("Repair Now"))
						carScript.repairNow = true;
				}

			}

			EditorGUILayout.Space();
			
		}

		if(carScript.GetComponent<RCCAICarController>()){

			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("This Vehicle Is Controlling By AI. Therefore, All Player Controllers Are Disabled For This Vehicle.", MessageType.Info);
			EditorGUILayout.Space();

			if(GUILayout.Button("Remove AI Controller From Vehicle")){
				carScript.AIController = false;
				DestroyImmediate(carScript.GetComponent<RCCAICarController>());
			}

		}

		EditorGUILayout.Space();
		GUI.color = Color.cyan;
		EditorGUILayout.HelpBox("System Overall Check", MessageType.None);
		GUI.color = defBackgroundColor;
		EditorGUILayout.Space();
		
		EditorGUILayout.BeginHorizontal();
		
		if(carScript.FrontLeftWheelCollider == null || carScript.FrontRightWheelCollider == null || carScript.RearLeftWheelCollider == null || carScript.RearRightWheelCollider == null)
			EditorGUILayout.HelpBox("Wheel Colliders = NOT OK", MessageType.Error);
		else
			EditorGUILayout.HelpBox("Wheel Colliders = OK", MessageType.None);
		
		if(carScript.FrontLeftWheelTransform == null || carScript.FrontRightWheelTransform == null || carScript.RearLeftWheelTransform == null || carScript.RearRightWheelTransform == null)
			EditorGUILayout.HelpBox("Wheel Transforms = NOT OK", MessageType.Error);
		else
			EditorGUILayout.HelpBox("Wheel Transforms = OK", MessageType.None);
		
		if(carScript.COM){
			
			if(carScript.COM == null)
				EditorGUILayout.HelpBox("COM = NOT OK", MessageType.Error);
			else
				EditorGUILayout.HelpBox("COM = OK", MessageType.None);
			
		}

		Collider[] cols = carScript.gameObject.GetComponentsInChildren<Collider>();

		if(cols.Length <= 4)
			EditorGUILayout.HelpBox("Your vehicle MUST have Box Collider, or Mesh Collider.", MessageType.Error);
		else
			EditorGUILayout.HelpBox("Colliders = OK", MessageType.None);
		
		EditorGUILayout.EndHorizontal();

		if(carScript.COM){
			
			if(Mathf.Approximately(carScript.COM.transform.localPosition.y, 0f))
				EditorGUILayout.HelpBox("You haven't changed COM position of the vehicle yet. Keep in that your mind, COM is most extremely important for realistic behavior.", MessageType.Warning);
			else
				EditorGUILayout.HelpBox("COM position = OK", MessageType.None);
			
		}else{
			
			EditorGUILayout.HelpBox("You haven't created COM of the vehicle yet. Just hit ''Create Necessary Gameobject Groups'' under ''Wheel'' tab for creating this too.", MessageType.Error);
			
		}
		
		if(carScript.mobileController){
			if(carScript._mobileControllerType == RCCCarControllerV2.MobileGUIType.NGUIController){
				if(!carScript.autoFindButtons){
					if(carScript.gasPedal == null || carScript.brakePedal == null || carScript.handbrakeGui == null || carScript.leftArrow == null || carScript.rightArrow == null)
						EditorGUILayout.HelpBox("Select all of your NGUI Controller Elements in script.", MessageType.Error);
				}else{
					EditorGUILayout.HelpBox("NGUI Elements = OK", MessageType.None);
				}
			}
			if(carScript._mobileControllerType == RCCCarControllerV2.MobileGUIType.UIController){
				if(!carScript.autoFindButtons){
					if(carScript.gasPedalUI == null || carScript.brakePedalUI == null || carScript.handbrakeUI == null || carScript.leftArrowUI == null || carScript.rightArrowUI == null)
						EditorGUILayout.HelpBox("Select all of your GUI Controller Elements in script.", MessageType.Error);
				}else{
					EditorGUILayout.HelpBox("GUI Elements = OK", MessageType.None);
				}
			}
		}
		
		serializedObject.ApplyModifiedProperties();

		if(GUI.changed && !EditorApplication.isPlaying){
			EngineCurveInit();
		}
		
	}
	
	void EngineCurveInit (){
		
		if(carScript.totalGears <= 0){
			Debug.LogError("You are trying to set your vehicle gear to 0 or below! Why you trying to do this???");
			return;
		}
		
		carScript.gearSpeed = new float[carScript.totalGears];
		carScript.engineTorqueCurve = new AnimationCurve[carScript.totalGears];
		carScript.currentGear = 0;
		
		for(int i = 0; i < carScript.engineTorqueCurve.Length; i ++){
			carScript.engineTorqueCurve[i] = new AnimationCurve(new Keyframe(0, 1));
		}
		
		for(int i = 0; i < carScript.totalGears; i ++){
			
			carScript.gearSpeed[i] = Mathf.Lerp(0, carScript.maxspeed / 1.5f, ((float)(i+1)/(float)(carScript.totalGears))) * Mathf.Lerp(1f, 1f, (float)i / carScript.totalGears);

			if(i != 0){
				carScript.engineTorqueCurve[i].MoveKey(0, new Keyframe(0, Mathf.Lerp (.5f, 0, (float)(i+1) / (float)carScript.totalGears)));
				carScript.engineTorqueCurve[i].AddKey(Mathf.Lerp(0, carScript.maxspeed / 1.5f, ((float)(i)/(float)(carScript.totalGears))), Mathf.Lerp(1f, .75f, ((float)(i)/(float)(carScript.totalGears))));
				carScript.engineTorqueCurve[i].AddKey(Mathf.Lerp (0, carScript.maxspeed, (float)(i+1) / (float)carScript.totalGears), .15f);
				carScript.engineTorqueCurve[i].postWrapMode = WrapMode.Clamp;
			}else{
				carScript.engineTorqueCurve[i].MoveKey(0, new Keyframe(0, 1));
				carScript.engineTorqueCurve[i].AddKey(Mathf.Lerp (0, carScript.maxspeed / 1.5f, (float)(i+1) / (float)carScript.totalGears), .75f);
				carScript.engineTorqueCurve[i].AddKey(Mathf.Lerp(25, carScript.maxspeed / 1f, ((float)(i+1) / (float)(carScript.totalGears))), 0f);
				carScript.engineTorqueCurve[i].postWrapMode = WrapMode.Clamp;
			}
			
		}
		
	}

	void SetDefaultSettings(){
		
		Transform[] objects = carScript.gameObject.GetComponentsInChildren<Transform>();
		bool didWeHaveThisObject = false;
		
		foreach(Transform g in objects){
			if(g.name == "Chassis")
				didWeHaveThisObject = true;
		}
		
		if(!didWeHaveThisObject){
			
			GameObject chassis = new GameObject("Chassis");
			chassis.transform.parent = carScript.transform;
			chassis.transform.localPosition = Vector3.zero;
			chassis.transform.localScale = Vector3.one;
			chassis.transform.rotation = carScript.transform.rotation;
			carScript.chassis = chassis;

			foreach(Transform g in objects){
				if(g.parent == carScript.transform){
					g.transform.parent = carScript.chassis.transform;
				}
			}

			GameObject wheelTransforms = new GameObject("Wheel Transforms");
			wheelTransforms.transform.parent = carScript.transform;
			wheelTransforms.transform.localPosition = Vector3.zero;
			wheelTransforms.transform.localScale = Vector3.one;
			wheelTransforms.transform.rotation = carScript.transform.rotation;
			GameObject COM = new GameObject("COM");
			COM.transform.parent = carScript.transform;
			COM.transform.localPosition = Vector3.zero;
			COM.transform.localScale = Vector3.one;
			COM.transform.rotation = carScript.transform.rotation;
			carScript.COM = COM.transform;
			
		}

	}

	void SetDefaultPrefabs(){

		if(!firstInit)
			return;

		AudioClip engineOn = Resources.Load<AudioClip>("DefaultPrefabsForFirstInit/DefaultEngineSounds/DefaultEngineOn") as AudioClip;
		AudioClip engineOff = Resources.Load<AudioClip>("DefaultPrefabsForFirstInit/DefaultEngineSounds/DefaultEngineOff") as AudioClip;
		AudioClip engineStart = Resources.Load<AudioClip>("DefaultPrefabsForFirstInit/DefaultEngineSounds/DefaultEngineStart") as AudioClip;
		AudioClip asphaltSkid = Resources.Load<AudioClip>("DefaultPrefabsForFirstInit/DefaultMiscSounds/DefaultSkidAsphalt") as AudioClip;
		AudioClip grassSkid = Resources.Load<AudioClip>("DefaultPrefabsForFirstInit/DefaultMiscSounds/DefaultSkidGrass") as AudioClip;
		AudioClip sandSkid = Resources.Load<AudioClip>("DefaultPrefabsForFirstInit/DefaultMiscSounds/DefaultSkidSand") as AudioClip;
		AudioClip wind = Resources.Load<AudioClip>("DefaultPrefabsForFirstInit/DefaultMiscSounds/DefaultWind") as AudioClip;
		AudioClip brakes = Resources.Load<AudioClip>("DefaultPrefabsForFirstInit/DefaultMiscSounds/DefaultBrake") as AudioClip;
		AudioClip[] crashes = Resources.LoadAll<AudioClip>("DefaultPrefabsForFirstInit/DefaultCrashSounds") as AudioClip[];
		AudioClip[] gears = Resources.LoadAll<AudioClip>("DefaultPrefabsForFirstInit/DefaultGearSounds") as AudioClip[];
		GameObject asphaltSmoke = Resources.Load<GameObject>("DefaultPrefabsForFirstInit/DefaultWheelSlipAsphalt") as GameObject;
		GameObject sandSmoke = Resources.Load<GameObject>("DefaultPrefabsForFirstInit/DefaultWheelSlipSand") as GameObject;

		carScript.engineClipOn = engineOn;
		carScript.engineClipOff = engineOff;
		carScript.engineStartClip = engineStart;
		carScript.asphaltSkidClip = asphaltSkid;
		carScript.grassSkidClip = grassSkid;
		carScript.sandSkidClip = sandSkid; 
		carScript.windClip = wind;
		carScript.brakeClip = brakes;
		carScript.crashClips = new AudioClip[crashes.Length];
		carScript.crashClips = crashes;
		carScript.gearShiftingClips = new AudioClip[gears.Length];
		carScript.gearShiftingClips = gears;
		carScript.wheelSlipAsphalt = asphaltSmoke;
		carScript.wheelSlipSand = sandSmoke;

		firstInit = false;
		
	}
	
}
