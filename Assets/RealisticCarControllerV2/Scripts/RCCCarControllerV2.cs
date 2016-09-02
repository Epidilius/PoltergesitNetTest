//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2015 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

#pragma warning disable 0414

using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent (typeof(Rigidbody))]
public class RCCCarControllerV2 : MonoBehaviour {

	// Rigidbody.
	private Rigidbody rigid;

	// Mobile Controller.
	public bool mobileController = false;
	public MobileGUIType _mobileControllerType;
	public enum MobileGUIType{UIController, NGUIController}

	public bool AIController = false;
	
	// Dashboard Type.
	public bool dashBoard = false;
	public DashboardType _dashboardType;
	public enum DashboardType{UIDashboard, NGUIDashboard}
	
	public bool useAccelerometerForSteer = false, steeringWheelControl = false;
	public float gyroTiltMultiplier = 2.0f;

	// Display Information GUI
	public bool demoGUI = false;

	private Vector3 defbrakePedalPosition;

	// Finds all buttons when new vehicle spawned. Useful for spawning new vehicles on your scene at runtime.
	public bool autoFindButtons = true;
	
	// NGUI Controller Elements.
	// If boost UI is selected, will multiply default engine torque by 1.5.
	public RCCNGUIController gasPedal, brakePedal, leftArrow, rightArrow, handbrakeGui, boostGui;
	// UI Controller Elements.
	public RCCUIController gasPedalUI, brakePedalUI, leftArrowUI, rightArrowUI, handbrakeUI, boostUI;
	
	// Wheel Transforms Of The Vehicle.
	public Transform FrontLeftWheelTransform;
	public Transform FrontRightWheelTransform;
	public Transform RearLeftWheelTransform;
	public Transform RearRightWheelTransform;
	
	// Wheel Colliders Of The Vehicle.
	public WheelCollider FrontLeftWheelCollider;
	public WheelCollider FrontRightWheelCollider;
	public WheelCollider RearLeftWheelCollider;
	public WheelCollider RearRightWheelCollider;
	private WheelCollider[] allWheelColliders;

	// Wheel Friction Curves.
	WheelFrictionCurve sidewaysFriction;
	WheelFrictionCurve forwardFriction;
	
	// Extra Wheels. In case of if your vehicle has extra wheels.
	public Transform[] ExtraRearWheelsTransform;
	public WheelCollider[] ExtraRearWheelsCollider;

	//Applies Engine Torque To Extra Rear Wheels.
	public bool applyEngineTorqueToExtraRearWheelColliders = true;
	
	// Driver Steering Wheel. In case of if your vehicle has individual steering wheel model in interior.
	public Transform SteeringWheel;
	
	// Set wheel drive of the vehicle. If you are using rwd, you have to be careful with your rear wheel collider
	// settings and com of the vehicle. Otherwise, vehicle will behave like a toy. ***My advice is use fwd always***
	public WheelType _wheelTypeChoise = WheelType.RWD;
	public enum WheelType{FWD, RWD, AWD}
	
	//Center of mass.
	public Transform COM;

	//Enables/Disables controlling the vehicle.
	public bool canControl = true;
	public bool runEngineAtAwake = true;
	public bool canEngineStall = true;
	public bool engineRunning = false;
	//Enables/Disables auto reversing when player press brake button. Useful for if you are making parking style game.
	public bool autoReverse = true;
	//Enables/Disables automatic gear shifting of the vehicle.
	public bool automaticGear = true;
	private bool canGoReverseNow = false;
	
	public AnimationCurve[] engineTorqueCurve;
	public float[] gearSpeed;
	public float engineTorque = 2500.0f;
	public float maxEngineRPM = 8000.0f;
	public float minEngineRPM = 1000.0f;
	public float engineAcceleration = 1000.0f;

	// Maximum steer angle of your vehicle.
	public float steerAngle = 40.0f;
	// Maximum steer angle at highest speed.
	public float highspeedsteerAngle = 6.0f;
	// Maximum speed for steer angle.
	public float highspeedsteerAngleAtspeed = 80.0f;

	// Anti Roll Force for preventing flip overs.
	public float antiRoll = 10000.0f;

	public float speed;
	public float brake = 4000.0f;
	public float maxspeed = 220.0f;

	// Enables/Disables differential of the vehicle. For more information, google it.
	public bool useDifferential = true;
	private float differentialRatioRight;
	private float differentialRatioLeft;
	private float differentialDifference;

	private float resetTime = 0f;
	private float defSteerAngle = 0f;
	
	// Gears.
	public int currentGear;
	public int totalGears = 6;
	public bool changingGear = false;
	
	// Each Wheel Transform's Rotation Value.
	private float _rotationValueFL, _rotationValueFR, _rotationValueRL, _rotationValueRR;
	private float[] rotationValueExtra;
	
	// Private Bools.
	internal bool reversing = false;
	private bool headLightsOn = false;
	private float acceleration = 0f;
	private float lastVelocity = 0f;
	private bool engineStarting = false;
	
	//Audio.
	private AudioSource engineStartSound;
	public AudioClip engineStartClip;
	private AudioSource engineSoundOn;
	public AudioClip engineClipOn;
	private AudioSource engineSoundOff;
	public AudioClip engineClipOff;
	private AudioSource gearShiftingSound;
	public AudioClip[] gearShiftingClips;
	private AudioSource crashSound;
	public AudioClip[] crashClips;
	private AudioSource windSound;
	public AudioClip windClip;
	private AudioSource brakeSound;
	public AudioClip brakeClip;
	private AudioSource skidSound;

	public AudioClip asphaltSkidClip;
	public AudioClip grassSkidClip;
	public AudioClip sandSkidClip;

	public float minEngineSoundPitch = .75f;
	public float maxEngineSoundPitch = 1.75f;
	public float minEngineSoundVolume = .05f;
	public float maxEngineSoundVolume = .85f;

	public float maxGearShiftingSoundVolume = .3f;
	public float maxCrashSoundVolume = 1f;
	public float maxWindSoundVolume = .25f;
	public float maxBrakeSoundVolume = .35f;

	private GameObject allAudioSources;
	private GameObject allContactParticles;

	// Physics Materials.
	private PhysicMaterial grassPhysicsMaterial;
	private PhysicMaterial sandPhysicsMaterial;
	public bool UseTerrainSplatMapForGroundPhysic = false;
	
	// Inputs.
	[HideInInspector]public float gasInput = 0f;
	[HideInInspector]public float brakeInput = 0f;
	[HideInInspector]public float steerInput = 0f;
	
	[HideInInspector]public float clutchInput = 0f;
	[HideInInspector]public float handbrakeInput = 0f;
	[HideInInspector]public float fuelInput = 1f;
	[HideInInspector]public float boostInput = 1f;

	private float engineRPM = 0f;
	
	// UI DashBoard.
	public RCCDashboardInputs UIInputs;
	private RectTransform RPMNeedle;
	private RectTransform KMHNeedle;
	private float RPMNeedleRotation = 0.0f;
	private float KMHNeedleRotation = 0.0f;
	private float smoothedNeedleRotation = 0.0f;
	
	// NGUI Dashboard.
	public GameObject RPMNeedleNGUI;
	public GameObject KMHNeedleNGUI;
	public float minimumRPMNeedleAngle = 20.0f;
	public float maximumRPMNeedleAngle = 160.0f;
	public float minimumKMHNeedleAngle = -25.0f;
	public float maximumKMHNeedleAngle = 155.0f;
	
	//Smokes.
	private GroundMaterial _groundMaterial = GroundMaterial.Asphalt;
	public enum GroundMaterial{Asphalt, Grass, Sand} 

	public GameObject wheelSlipAsphalt;
	public GameObject wheelSlipSand;
	private List <ParticleSystem> _wheelSlipAsphalt = new List<ParticleSystem>();
	private List <ParticleSystem> _wheelSlipSand = new List<ParticleSystem>();

	public ParticleSystem[] exhaustGas;

	// Script will simulate chassis movement based on vehicle rigidbody situation.
	public GameObject chassis;
	// Chassis Vertical Lean Sensitivity
	public float chassisVerticalLean = 4.0f;
	// Chassis Horizontal Lean Sensitivity
	public float chassisHorizontalLean = 4.0f;

	private float horizontalLean = 0.0f;
	private float verticalLean = 0.0f;
	
	// Lights.
	public Light[] headLights;
	public Light[] brakeLights;
	public Light[] reverseLights;

	private float brakeLightInput;
	
	// Steering Wheel Controller.
	public float steeringWheelMaximumsteerAngle = 180.0f;
	public float steeringWheelGuiScale = 256f;
	private float _steeringWheelGuiScale = 0f;
	public float steeringWheelXOffset = 30.0f;
	public float steeringWheelYOffset = 30.0f;
	public Vector2 steeringWheelPivotPos = Vector2.zero;
	public float steeringWheelResetPosspeed = 200.0f;
	public Texture2D steeringWheelTexture;
	private float steeringWheelsteerAngle ;
	private bool  steeringWheelIsTouching;
	private Rect steeringWheelTextureRect;
	private Vector2 steeringWheelWheelCenter;
	private float steeringWheelOldAngle;
	private int touchId = -1;
	private Vector2 touchPos;

	// Damage
	struct originalMeshVerts{
		public Vector3[] meshVerts;
	}

	public bool useDamage = true;
	public MeshFilter[] deformableMeshFilters;
	
	public float randomizeVertices = 1f;
	public float damageRadius = .5f;

	// Comparing Original Vertex Position And Last Vertex Position To Decide Mesh Is Repaired Or Not.
	private float minimumVertDistanceForDamagedMesh = .002f;
	
	private Vector3[] colliderVerts;
	private originalMeshVerts[] originalMeshData;
	[HideInInspector]public bool sleep = true;

	// Maximum Vert Distance For Limiting Damage. Setting 0 Will Disable The Limit.
	public float maximumDamage = .5f;
	private float minimumCollisionForce = 5f;
	public float damageMultiplier = 1f;
	
	public GameObject contactSparkle;
	public int maximumContactSparkle = 5;
	private List<ParticleSystem> contactSparkeList = new List<ParticleSystem>();
	public bool repairNow = false;
	
	private Vector3 localVector;
	private Quaternion rot = Quaternion.identity;

	public bool ABS = true;
	public bool ASR = true;

	void Awake() {
		
		if(!GetComponent<RCCAICarController>())
			AIController = false;
		
	}

	void Start (){

		rigid = GetComponent<Rigidbody>();
		Time.fixedDeltaTime = .02f;
		rigid.maxAngularVelocity = 5f;

		allWheelColliders = GetComponentsInChildren<WheelCollider>();
		rotationValueExtra = new float[ExtraRearWheelsCollider.Length];
		defSteerAngle = steerAngle;

		grassPhysicsMaterial = Resources.Load("RCCGrassPhysics") as PhysicMaterial;
		sandPhysicsMaterial = Resources.Load("RCCSandPhysics") as PhysicMaterial;

		allAudioSources = new GameObject("All Audio Sources");
		allAudioSources.transform.SetParent(transform);
		allAudioSources.transform.localPosition = Vector3.zero;

		allContactParticles = new GameObject("All Contact Particles");
		allContactParticles.transform.SetParent(transform);
		allContactParticles.transform.localPosition = Vector3.zero;

		if(dashBoard && GameObject.FindObjectOfType<RCCDashboardInputs>()){

			UIInputs = GameObject.FindObjectOfType<RCCDashboardInputs>();
			UIInputs.GetNeedles();

			if(_dashboardType == DashboardType.NGUIDashboard){
				RPMNeedleNGUI = UIInputs.RPMNeedleNGUI;
				KMHNeedleNGUI = UIInputs.KMHNeedleNGUI;
			}else{
				RPMNeedle = UIInputs.RPMNeedleUI;
				KMHNeedle = UIInputs.KMHNeedleUI;
			}
			 
		}else if(dashBoard){

			Debug.LogError("RCCDashboardInputs not found! Ensure your scene has UI or NGUI canvas with RCCDashboardInputs script. You can use ready to use prefabs in Prefabs folder.");
			dashBoard = false;

		}

		if(autoFindButtons && mobileController){

			RCCMobileButtons UIButtons = GameObject.FindObjectOfType<RCCMobileButtons>();

			if(_mobileControllerType == MobileGUIType.NGUIController){
				gasPedal = UIButtons.gasButton.GetComponent<RCCNGUIController>();
				brakePedal = UIButtons.brakeButton.GetComponent<RCCNGUIController>();
				leftArrow = UIButtons.leftButton.GetComponent<RCCNGUIController>();
				rightArrow = UIButtons.rightButton.GetComponent<RCCNGUIController>();
				handbrakeGui = UIButtons.handbrakeButton.GetComponent<RCCNGUIController>();
				if(UIButtons.boostButton)
					boostGui = UIButtons.boostButton.GetComponent<RCCNGUIController>();
			}else{
				gasPedalUI = UIButtons.gasButton.GetComponent<RCCUIController>();
				brakePedalUI = UIButtons.brakeButton.GetComponent<RCCUIController>();
				leftArrowUI = UIButtons.leftButton.GetComponent<RCCUIController>();
				rightArrowUI = UIButtons.rightButton.GetComponent<RCCUIController>();
				handbrakeUI = UIButtons.handbrakeButton.GetComponent<RCCUIController>();
				if(UIButtons.boostButton)
					boostUI = UIButtons.boostButton.GetComponent<RCCUIController>();
			}

		}

		SoundsInitialize();
		MobileGUI();
		SteeringWheelInit();
		SmokeInit();

		if(useDamage)
			DamageInit();

		if(runEngineAtAwake)
			KillOrStartEngine();
		
	}
	
	public void CreateWheelColliders (){
		
		List <Transform> allWheelTransforms = new List<Transform>();
		allWheelTransforms.Add(FrontLeftWheelTransform); allWheelTransforms.Add(FrontRightWheelTransform); allWheelTransforms.Add(RearLeftWheelTransform); allWheelTransforms.Add(RearRightWheelTransform);
		
		if(allWheelTransforms[0] == null){
			Debug.LogError("You haven't choose your Wheel Transforms. Please select all of your Wheel Transforms before creating Wheel Colliders. Script needs to know their positions, aye?");
			return;
		}
		
		transform.rotation = Quaternion.identity;
		
		GameObject WheelColliders = new GameObject("Wheel Colliders");
		WheelColliders.transform.parent = transform;
		WheelColliders.transform.rotation = transform.rotation;
		WheelColliders.transform.localPosition = Vector3.zero;
		WheelColliders.transform.localScale = Vector3.one;
		
		foreach(Transform wheel in allWheelTransforms){
			
			GameObject wheelcollider = new GameObject(wheel.transform.name); 
			
			wheelcollider.transform.position = wheel.transform.position;
			wheelcollider.transform.rotation = transform.rotation;
			wheelcollider.transform.name = wheel.transform.name;
			wheelcollider.transform.parent = WheelColliders.transform;
			wheelcollider.transform.localScale = Vector3.one;
			wheelcollider.AddComponent<WheelCollider>();
			wheelcollider.GetComponent<WheelCollider>().radius = (wheel.GetComponent<MeshRenderer>().bounds.size.y / 2f) / transform.localScale.y;

			wheelcollider.AddComponent<RCCWheelCollider>();
			//wheelcollider.GetComponent<RCCWheelCollider>().hideFlags = HideFlags.HideInInspector;
			
			JointSpring spring = wheelcollider.GetComponent<WheelCollider>().suspensionSpring;

			spring.spring = 30000f;
			spring.damper = 2000f;

			wheelcollider.GetComponent<WheelCollider>().suspensionSpring = spring;
			wheelcollider.GetComponent<WheelCollider>().suspensionDistance = .25f;
			wheelcollider.GetComponent<WheelCollider>().forceAppPointDistance = .25f;
			wheelcollider.GetComponent<WheelCollider>().mass = 125f;
			wheelcollider.GetComponent<WheelCollider>().wheelDampingRate = 5f;

			wheelcollider.transform.localPosition = new Vector3(wheelcollider.transform.localPosition.x, wheelcollider.transform.localPosition.y + (wheelcollider.GetComponent<WheelCollider>().suspensionDistance / 2f), wheelcollider.transform.localPosition.z);
			
			sidewaysFriction = wheelcollider.GetComponent<WheelCollider>().sidewaysFriction;
			forwardFriction = wheelcollider.GetComponent<WheelCollider>().forwardFriction;

			forwardFriction.extremumSlip = .4f;
			forwardFriction.extremumValue = 1;
			forwardFriction.asymptoteSlip = .8f;
			forwardFriction.asymptoteValue = .75f;
			forwardFriction.stiffness = 1.75f;

			sidewaysFriction.extremumSlip = .25f;
			sidewaysFriction.extremumValue = 1;
			sidewaysFriction.asymptoteSlip = .5f;
			sidewaysFriction.asymptoteValue = 1f;
			sidewaysFriction.stiffness = 2f;

			wheelcollider.GetComponent<WheelCollider>().sidewaysFriction = sidewaysFriction;
			wheelcollider.GetComponent<WheelCollider>().forwardFriction = forwardFriction;

		}
		
		WheelCollider[] allWheelColliders = new WheelCollider[allWheelTransforms.Count];
		allWheelColliders = GetComponentsInChildren<WheelCollider>();
		
		FrontLeftWheelCollider = allWheelColliders[0];
		FrontRightWheelCollider = allWheelColliders[1];
		RearLeftWheelCollider = allWheelColliders[2];
		RearRightWheelCollider = allWheelColliders[3];
		
	}
	
	public void SoundsInitialize (){

		engineSoundOn = RCCCreateAudioSource.NewAudioSource(gameObject, "Engine Sound On AudioSource", 5, 0, engineClipOn, true, true, false);
		engineSoundOff = RCCCreateAudioSource.NewAudioSource(gameObject, "Engine Sound Off AudioSource", 5, 0, engineClipOff, true, true, false);
		skidSound = RCCCreateAudioSource.NewAudioSource(gameObject, "Skid Sound AudioSource", 5, 0, asphaltSkidClip, true, true, false);
		windSound = RCCCreateAudioSource.NewAudioSource(gameObject, "Wind Sound AudioSource", 5, 0, windClip, true, true, false);
		brakeSound = RCCCreateAudioSource.NewAudioSource(gameObject, "Brake Sound AudioSource", 5, 0, brakeClip, true, true, false);
		
	}
	
	public void KillOrStartEngine (){
		
		if(engineRunning && !engineStarting){
			engineRunning = false;
		}else if(!engineStarting){
			StartCoroutine("StartEngine");
		}
		
	}
	
	IEnumerator StartEngine (){

		engineRunning = false;
		engineStarting = true;
		if(!engineRunning)
			engineStartSound = RCCCreateAudioSource.NewAudioSource(gameObject, "Engine Start AudioSource", 5, 1, engineStartClip, false, true, true);
		yield return new WaitForSeconds(1f);
		engineRunning = true;
		yield return new WaitForSeconds(1f);
		engineStarting = false;

	}
	
	public void SteeringWheelInit (){
		
		_steeringWheelGuiScale = ((Screen.width * 1.0f) / 2.7f) * (steeringWheelGuiScale / 256f);
		steeringWheelIsTouching = false;
		steeringWheelTextureRect = new Rect( steeringWheelXOffset + (_steeringWheelGuiScale / Screen.width ), -steeringWheelYOffset + (Screen.height - (_steeringWheelGuiScale)), _steeringWheelGuiScale, _steeringWheelGuiScale );
		steeringWheelWheelCenter = new Vector2( steeringWheelTextureRect.x + steeringWheelTextureRect.width * 0.5f, Screen.height - steeringWheelTextureRect.y - steeringWheelTextureRect.height * 0.5f );
		steeringWheelsteerAngle  = 0f;
		
	}
	
	public void SmokeInit (){

		if(!wheelSlipAsphalt)
			return;
		
		for(int i = 0; i < allWheelColliders.Length; i++){
			GameObject ps = (GameObject)Instantiate(wheelSlipAsphalt, transform.position, transform.rotation) as GameObject;
			_wheelSlipAsphalt.Add(ps.GetComponent<ParticleSystem>());
			ps.GetComponent<ParticleSystem>().enableEmission = false;
			ps.transform.SetParent(allWheelColliders[i].transform);
			ps.transform.localPosition = Vector3.zero;
		}

		if(!wheelSlipSand)
			return;

		for(int i = 0; i < allWheelColliders.Length; i++){
			GameObject ps = (GameObject)Instantiate(wheelSlipSand, transform.position, transform.rotation) as GameObject;
			_wheelSlipSand.Add(ps.GetComponent<ParticleSystem>());
			ps.GetComponent<ParticleSystem>().enableEmission = false;
			ps.transform.SetParent(allWheelColliders[i].transform);
			ps.transform.localPosition = Vector3.zero;
		}

	}

	public void DamageInit (){

		if (deformableMeshFilters.Length == 0){
			MeshFilter[] allMeshFilters = GetComponentsInChildren<MeshFilter>();
			List <MeshFilter> properMeshFilters = new List<MeshFilter>();
			foreach(MeshFilter mf in allMeshFilters){
				if(mf.gameObject != FrontLeftWheelTransform.gameObject && mf.gameObject != FrontRightWheelTransform.gameObject && mf.gameObject != RearLeftWheelTransform.gameObject && mf.gameObject != RearRightWheelTransform.gameObject)
					properMeshFilters.Add(mf);
			}
			deformableMeshFilters = properMeshFilters.ToArray(); 
		}
		
		LoadOriginalMeshData();
		
		if(contactSparkle){
			
			for(int i = 0; i < maximumContactSparkle; i++){
				GameObject sparks = (GameObject)Instantiate(contactSparkle, transform.position, Quaternion.identity) as GameObject;
				sparks.transform.SetParent(allContactParticles.transform);
				contactSparkeList.Add(sparks.GetComponent<ParticleSystem>());
				sparks.GetComponent<ParticleSystem>().enableEmission = false;
			}
			
		}

	}

	void LoadOriginalMeshData(){

		originalMeshData = new originalMeshVerts[deformableMeshFilters.Length];

		for (int i = 0; i < deformableMeshFilters.Length; i++){
			originalMeshData[i].meshVerts = deformableMeshFilters[i].mesh.vertices;
		}

	}

	void Damage(){

		if (!sleep && repairNow){
			
			int k;
			sleep = true;
			for(k = 0; k < deformableMeshFilters.Length; k++){
				Vector3[] vertices = deformableMeshFilters[k].mesh.vertices;
				if(originalMeshData==null)
					LoadOriginalMeshData();
				for (int i = 0; i < vertices.Length; i++){
					vertices[i] += (originalMeshData[k].meshVerts[i] - vertices[i]) * (Time.deltaTime * 2f);
					if((originalMeshData[k].meshVerts[i] - vertices[i]).magnitude >= minimumVertDistanceForDamagedMesh)
						sleep = false;
				}
				deformableMeshFilters[k].mesh.vertices=vertices;
				deformableMeshFilters[k].mesh.RecalculateNormals();
				deformableMeshFilters[k].mesh.RecalculateBounds();
			}
			
			if(sleep)
				repairNow = false;
			
		}

	}

	void DeformMesh(Mesh mesh, Vector3[] originalMesh, Collision collision, float cos, Transform meshTransform, Quaternion rot, float MeshScale){
		
		Vector3[] vertices = mesh.vertices;
		
		foreach (ContactPoint contact in collision.contacts){
			
			Vector3 point =meshTransform.InverseTransformPoint(contact.point);
			 
			for (int i = 0; i < vertices.Length; i++){
				if ((point - vertices[i]).magnitude < damageRadius){
					vertices[i] += rot * ((localVector * (damageRadius - (point - vertices[i]).magnitude) / damageRadius) * cos + (UnityEngine.Random.onUnitSphere * (randomizeVertices / 500f)));
					if (maximumDamage > 0 && ((vertices[i] - originalMesh[i]).magnitude) > maximumDamage){
						vertices[i] = originalMesh[i] + (vertices[i] - originalMesh[i]).normalized * (maximumDamage);
					}
				}
			}
			
		}
		
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		
	}

	void CollisionParticles(Vector3 contactPoint){
		
		for(int i = 0; i < contactSparkeList.Count; i++){
			if(contactSparkeList[i].isPlaying)
				return;
			contactSparkeList[i].transform.position = contactPoint;
			contactSparkeList[i].enableEmission = true;
			contactSparkeList[i].Play();
		}
		
	}
	
	public void MobileGUI (){

		if(mobileController){
			if(_mobileControllerType == MobileGUIType.NGUIController){
				defbrakePedalPosition = brakePedal.transform.position;
			}else{
				defbrakePedalPosition = brakePedalUI.transform.position;
			}
		}
		
	}
	
	void Update (){

		//Enables inputs for controlling vehicle.
		if(canControl){
			if(mobileController && !AIController){
				if(_mobileControllerType == MobileGUIType.NGUIController)
					NGUIControlling();
				if(_mobileControllerType == MobileGUIType.UIController)
					UIControlling();
				MobileSteeringInputs();
				if(steeringWheelControl)
					SteeringWheelControlling();
			}else if(!AIController){
				KeyboardControlling();
			}
			ApplySteering(FrontLeftWheelCollider);
			ApplySteering(FrontRightWheelCollider);
			Lights();
			ResetCar();
		}else{
			gasInput = 0f;
			brakeInput = 0f;
			boostInput = 1f;
		}

		GearBox();
		WheelAlign();
		GroundPhysicsMaterial();
		foreach(WheelCollider wc in allWheelColliders){
			WheelCamber(wc);
		}
		Sounds();

		if(useDamage)
			Damage();

		if(chassis)
			Chassis();

		if(dashBoard && canControl)
			DashboardGUI();
		
	}
	
	void FixedUpdate (){

		Engine();
		Braking();
		Differential();
		AntiRollBars();
		SmokeWeedEveryday(_groundMaterial);
		
	}
	
	public void Engine (){
		
		//Speed.
		speed = rigid.velocity.magnitude * 3f;

		//Acceleration Calculation.
		acceleration = 0f;
		acceleration = (transform.InverseTransformDirection(rigid.velocity).z - lastVelocity) / Time.fixedDeltaTime;
		lastVelocity = transform.InverseTransformDirection(rigid.velocity).z;
		
		//Drag Limit Depends On Vehicle Acceleration.
		rigid.drag = Mathf.Clamp((acceleration / 50f), 0f, .1f);

		//Angular Drag Limit Depends On Vehicle Speed.
		rigid.angularDrag = Mathf.Clamp((speed / maxspeed), 0f, 1f);
		
		//Steer Limit.
		steerAngle = Mathf.Lerp(defSteerAngle, highspeedsteerAngle, (speed / highspeedsteerAngleAtspeed));

		float rpm = 0f;
		float wheelRPM = ((Mathf.Abs((FrontLeftWheelCollider.rpm * FrontLeftWheelCollider.radius) + (FrontRightWheelCollider.rpm * FrontRightWheelCollider.radius)) / 2f) / 3.25f);

		if(!reversing)
			rpm = Mathf.Clamp((Mathf.Lerp(0 - (minEngineRPM * (currentGear + 1)), maxEngineRPM,wheelRPM / (gearSpeed[currentGear] * 1.25f)) + minEngineRPM) * (1 - clutchInput) + ((clutchInput * gasInput) * maxEngineRPM), (minEngineRPM * (clutchInput)), maxEngineRPM);
		else
			rpm = Mathf.Clamp((Mathf.Lerp(0 - (minEngineRPM * (currentGear + 1)), maxEngineRPM,wheelRPM / (gearSpeed[currentGear] * 1.25f)) + minEngineRPM) * (1 - clutchInput) + ((clutchInput * brakeInput) * maxEngineRPM), (minEngineRPM * (clutchInput)), maxEngineRPM);

		engineRPM = Mathf.Lerp(engineRPM, (rpm + UnityEngine.Random.Range(-50f, 50f)) * fuelInput, Time.deltaTime * 2f);

		if(engineRPM < (minEngineRPM / 2f) && !engineStarting){
			if(canEngineStall)
				engineRunning = false;
		}

		if(!engineRunning)
			fuelInput = 0;
		else
			fuelInput = 1;

		//Reversing Bool.
		if(!AIController){
			if(brakeInput > .1f  && transform.InverseTransformDirection(rigid.velocity).z < 1f && canGoReverseNow)
				reversing = true;
			else
				reversing = false;
		}
		
		//Auto Reverse Bool.
		if(autoReverse){
			canGoReverseNow = true;
		}else{
			if(brakeInput < .1f && speed < 5)
				canGoReverseNow = true;
			else if(brakeInput > 0 && transform.InverseTransformDirection(rigid.velocity).z > 1f)
				canGoReverseNow = false;
		}

		foreach(WheelCollider wc in allWheelColliders){
			wc.wheelDampingRate = Mathf.Lerp(5f, 0f, gasInput);
		}
		
		#region Wheel Type Motor Torque.

		//Applying WheelCollider Motor Torques Depends On Wheel Type Choice.
		switch(_wheelTypeChoise){
			
		case WheelType.FWD:
			ApplyMotorTorque(FrontLeftWheelCollider, engineTorque, true);
			ApplyMotorTorque(FrontRightWheelCollider, engineTorque, false);
			break;
		case WheelType.RWD:
			ApplyMotorTorque(RearLeftWheelCollider, engineTorque, true);
			ApplyMotorTorque(RearRightWheelCollider, engineTorque, false);
			break;
		case WheelType.AWD:
			ApplyMotorTorque(FrontLeftWheelCollider, engineTorque / 2f, true);
			ApplyMotorTorque(FrontRightWheelCollider, engineTorque / 2f, false);
			ApplyMotorTorque(RearLeftWheelCollider, engineTorque / 2f, true);
			ApplyMotorTorque(RearRightWheelCollider, engineTorque / 2f, false);
			break;

		}

		if(ExtraRearWheelsCollider.Length > 0 && applyEngineTorqueToExtraRearWheelColliders){
			foreach(WheelCollider wc in ExtraRearWheelsCollider)
				ApplyMotorTorque(wc, engineTorque, true);
		}
		
		#endregion Wheel Type
		
	}

	public void Sounds(){

		//Engine Audio Volume.
		if(engineSoundOn){
			
			if(!reversing)
				engineSoundOn.volume = Mathf.Lerp (engineSoundOn.volume, Mathf.Clamp (gasInput, minEngineSoundVolume, maxEngineSoundVolume), Time.deltaTime * 50f);
			else
				engineSoundOn.volume = Mathf.Lerp (engineSoundOn.volume, Mathf.Clamp (brakeInput, minEngineSoundVolume, maxEngineSoundVolume), Time.deltaTime * 50f);
			
			if(engineRunning)
				engineSoundOn.pitch = Mathf.Lerp ( engineSoundOn.pitch, Mathf.Lerp (minEngineSoundPitch, maxEngineSoundPitch, (engineRPM) / (maxEngineRPM)), Time.deltaTime * 50f);
			else
				engineSoundOn.pitch = Mathf.Lerp ( engineSoundOn.pitch, 0, Time.deltaTime * 50f);
			
		}
		
		if(engineSoundOff){
			
			if(!reversing)
				engineSoundOff.volume = Mathf.Lerp (engineSoundOff.volume, Mathf.Clamp ((1 + (-gasInput)), minEngineSoundVolume, .5f), Time.deltaTime * 50f);
			else
				engineSoundOff.volume = Mathf.Lerp (engineSoundOff.volume, Mathf.Clamp ((1 + (-brakeInput)), minEngineSoundVolume, .5f), Time.deltaTime * 50f);
			
			if(engineRunning)
				engineSoundOff.pitch = Mathf.Lerp ( engineSoundOff.pitch, Mathf.Lerp (minEngineSoundPitch, maxEngineSoundPitch, (engineRPM) / (maxEngineRPM)), Time.deltaTime * 50f);
			else
				engineSoundOff.pitch = Mathf.Lerp ( engineSoundOff.pitch, 0, Time.deltaTime * 50f);
			
		}

		windSound.volume = Mathf.Lerp (0f, maxWindSoundVolume, speed / maxspeed);
		windSound.pitch = UnityEngine.Random.Range(.9f, 1.1f) * 1f;
		
		if(!reversing)
			brakeSound.volume = Mathf.Lerp (0f, maxBrakeSoundVolume, Mathf.Clamp01(brakeInput) * Mathf.Lerp(0f, 1f, FrontLeftWheelCollider.rpm / 50f));
		else
			brakeSound.volume = 0f;

	}

	public void ApplyMotorTorque(WheelCollider wc, float torque, bool leftSide){

		if(speed > maxspeed || Mathf.Abs(FrontLeftWheelCollider.rpm) > 3000 || Mathf.Abs(RearLeftWheelCollider.rpm) > 3000 || !engineRunning)
			torque = 0;

		if(reversing && speed > 55)
			torque = 0;

		if(!engineRunning)
			torque = 0;

		if(ASR){
			WheelHit hit;
			wc.GetGroundHit(out hit);
			if(Mathf.Abs(hit.forwardSlip) > .35f)
				torque = 0;
		}

		if(!reversing){
			if(leftSide)
				wc.motorTorque = (torque * (1 - clutchInput)) * (Mathf.Clamp((gasInput * fuelInput) * differentialRatioLeft, 0f, 1f) * boostInput) * engineTorqueCurve[currentGear].Evaluate(speed);
			else
				wc.motorTorque = (torque * (1 - clutchInput)) * (Mathf.Clamp((gasInput * fuelInput) * differentialRatioRight, 0f, 1f) * boostInput) * engineTorqueCurve[currentGear].Evaluate(speed);
		}else{
			wc.motorTorque =  (-torque * (1 - clutchInput)) * brakeInput;
		}
		
	}

	public void ApplyBrakeTorque(WheelCollider wc, float brake){

		if(ABS && handbrakeInput < .1f){
			WheelHit hit;
			wc.GetGroundHit(out hit);
			if(Mathf.Abs(hit.forwardSlip) > .35f)
				brake = 0;
		}

		wc.brakeTorque = brake;

	}

	public void ApplySteering(WheelCollider wc){

		wc.steerAngle = Mathf.Clamp((steerAngle * steerInput), -steerAngle, steerAngle);

	}
	
	public void Braking (){

		//Handbrake
		if(handbrakeInput > .1f){
			
			ApplyBrakeTorque(RearLeftWheelCollider, (brake * 1.25f) * handbrakeInput);
			ApplyBrakeTorque(RearRightWheelCollider, (brake * 1.25f) * handbrakeInput);
			
		}else{
			
			// Braking.
			if(brakeInput > .1f && !reversing){
				ApplyBrakeTorque(FrontLeftWheelCollider, brake * (brakeInput / 1f));
				ApplyBrakeTorque(FrontRightWheelCollider, brake * (brakeInput / 1f));
				ApplyBrakeTorque(RearLeftWheelCollider, brake * brakeInput);
				ApplyBrakeTorque(RearRightWheelCollider, brake * brakeInput);
			}else if(brakeInput > .1f && reversing){
				ApplyBrakeTorque(FrontLeftWheelCollider, 0f);
				ApplyBrakeTorque(FrontRightWheelCollider, 0f);
				ApplyBrakeTorque(RearLeftWheelCollider, 0f);
				ApplyBrakeTorque(RearRightWheelCollider, 0f);
			}else if(brakeInput < .1f){
				ApplyBrakeTorque(FrontLeftWheelCollider, 0f);
				ApplyBrakeTorque(FrontRightWheelCollider, 0f);
				ApplyBrakeTorque(RearLeftWheelCollider, 0f);
				ApplyBrakeTorque(RearRightWheelCollider, 0f);
			}
			
		}
		
	}
	
	public void Differential (){
		
		if(useDifferential){
			
			if(_wheelTypeChoise == WheelType.FWD){
				differentialDifference = Mathf.Clamp ( Mathf.Abs (FrontRightWheelCollider.rpm) - Mathf.Abs (FrontLeftWheelCollider.rpm), -50f, 50f );
				differentialRatioRight = Mathf.Lerp ( 0f, 1f, ( (((Mathf.Abs (FrontRightWheelCollider.rpm) + Mathf.Abs (FrontLeftWheelCollider.rpm)) + 10 / 2 ) + differentialDifference) /  (Mathf.Abs (FrontRightWheelCollider.rpm) + Mathf.Abs (FrontLeftWheelCollider.rpm))) );
				differentialRatioLeft = Mathf.Lerp ( 0f, 1f, ( (((Mathf.Abs (FrontRightWheelCollider.rpm) + Mathf.Abs (FrontLeftWheelCollider.rpm)) + 10 / 2 ) - differentialDifference) /  (Mathf.Abs (FrontRightWheelCollider.rpm) + Mathf.Abs (FrontLeftWheelCollider.rpm))) );
			}
			if(_wheelTypeChoise == WheelType.RWD){
				differentialDifference = Mathf.Clamp ( Mathf.Abs (RearRightWheelCollider.rpm) - Mathf.Abs (RearLeftWheelCollider.rpm), -50f, 50f );
				differentialRatioRight = Mathf.Lerp ( 0f, 1f, ( (((Mathf.Abs (RearRightWheelCollider.rpm) +  Mathf.Abs (RearLeftWheelCollider.rpm)) + 10 / 2 ) + differentialDifference) /  (Mathf.Abs (RearRightWheelCollider.rpm) + Mathf.Abs (RearLeftWheelCollider.rpm))) );
				differentialRatioLeft = Mathf.Lerp ( 0f, 1f, ( (((Mathf.Abs (RearRightWheelCollider.rpm) +  Mathf.Abs (RearLeftWheelCollider.rpm)) + 10 / 2 ) - differentialDifference) /  (Mathf.Abs (RearRightWheelCollider.rpm) + Mathf.Abs (RearLeftWheelCollider.rpm))) );
			}
			if(_wheelTypeChoise == WheelType.AWD){
				differentialDifference = Mathf.Clamp ( Mathf.Abs (RearRightWheelCollider.rpm) - Mathf.Abs (RearLeftWheelCollider.rpm), -50f, 50f );
				differentialRatioRight = Mathf.Lerp ( 0f, 1f, ( (((Mathf.Abs (RearRightWheelCollider.rpm) +  Mathf.Abs (RearLeftWheelCollider.rpm)) + 10 / 2 ) + differentialDifference) /  (Mathf.Abs (RearRightWheelCollider.rpm) + Mathf.Abs (RearLeftWheelCollider.rpm))) );
				differentialRatioLeft = Mathf.Lerp ( 0f, 1f, ( (((Mathf.Abs (RearRightWheelCollider.rpm) +  Mathf.Abs (RearLeftWheelCollider.rpm)) + 10 / 2 ) - differentialDifference) /  (Mathf.Abs (RearRightWheelCollider.rpm) + Mathf.Abs (RearLeftWheelCollider.rpm))) );
			}
			
		}else{
			
			differentialRatioRight = 1;
			differentialRatioLeft = 1;
			
		}
		
	}
	
	public void AntiRollBars (){

		WheelHit FrontWheelHit;
		
		float travelFL = 1.0f;
		float travelFR = 1.0f;
		
		bool groundedFL= FrontLeftWheelCollider.GetGroundHit(out FrontWheelHit);
		
		if (groundedFL)
			travelFL = (-FrontLeftWheelCollider.transform.InverseTransformPoint(FrontWheelHit.point).y - FrontLeftWheelCollider.radius) / FrontLeftWheelCollider.suspensionDistance;
		
		bool groundedFR= FrontRightWheelCollider.GetGroundHit(out FrontWheelHit);
		
		if (groundedFR)
			travelFR = (-FrontRightWheelCollider.transform.InverseTransformPoint(FrontWheelHit.point).y - FrontRightWheelCollider.radius) / FrontRightWheelCollider.suspensionDistance;
		
		float antiRollForceFront= (travelFL - travelFR) * antiRoll;
		
		if (groundedFL)
			rigid.AddForceAtPosition(FrontLeftWheelCollider.transform.up * -antiRollForceFront, FrontLeftWheelCollider.transform.position); 
		if (groundedFR)
			rigid.AddForceAtPosition(FrontRightWheelCollider.transform.up * antiRollForceFront, FrontRightWheelCollider.transform.position); 
		
		WheelHit RearWheelHit;
		
		float travelRL = 1.0f;
		float travelRR = 1.0f;
		
		bool groundedRL= RearLeftWheelCollider.GetGroundHit(out RearWheelHit);
		
		if (groundedRL)
			travelRL = (-RearLeftWheelCollider.transform.InverseTransformPoint(RearWheelHit.point).y - RearLeftWheelCollider.radius) / RearLeftWheelCollider.suspensionDistance;
		
		bool groundedRR= RearRightWheelCollider.GetGroundHit(out RearWheelHit);
		
		if (groundedRR)
			travelRR = (-RearRightWheelCollider.transform.InverseTransformPoint(RearWheelHit.point).y - RearRightWheelCollider.radius) / RearRightWheelCollider.suspensionDistance;
		
		float antiRollForceRear= (travelRL - travelRR) * antiRoll;
		
		if (groundedRL)
			rigid.AddForceAtPosition(RearLeftWheelCollider.transform.up * -antiRollForceRear, RearLeftWheelCollider.transform.position); 
		if (groundedRR)
			rigid.AddForceAtPosition(RearRightWheelCollider.transform.up * antiRollForceRear, RearRightWheelCollider.transform.position);

		if (groundedRR && groundedRL){
			rigid.AddRelativeTorque((Vector3.up * (steerInput * gasInput)) * 2000f);
		}

	}
	
	public void MobileSteeringInputs (){
		
		//Accelerometer Inputs.
		if(useAccelerometerForSteer){
			
			steerInput = Input.acceleration.x * gyroTiltMultiplier;

			ApplySteering(FrontLeftWheelCollider);
			ApplySteering(FrontRightWheelCollider);
			
		}else{
			
			//TouchScreen Inputs.
			if(!steeringWheelControl){

				ApplySteering(FrontLeftWheelCollider);
				ApplySteering(FrontRightWheelCollider);

			//SteeringWheel Inputs.
			}else{

				ApplySteering(FrontLeftWheelCollider);
				ApplySteering(FrontRightWheelCollider);
				
			}
			
		}
		
	}
	
	public void SteeringWheelControlling (){
		
		if(steeringWheelIsTouching){
			
			foreach(Touch touch in Input.touches )
			{
				if(touch.fingerId == touchId){
					touchPos = touch.position;
					
					if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled){
						steeringWheelIsTouching = false; 
					}
				}
			}
			
			float newsteerAngle = Vector2.Angle(Vector2.up, touchPos - steeringWheelWheelCenter);
			
			if(Vector2.Distance(touchPos, steeringWheelWheelCenter) > 20f){
				if(touchPos.x > steeringWheelWheelCenter.x)
					steeringWheelsteerAngle -= newsteerAngle - steeringWheelOldAngle;
				else
					steeringWheelsteerAngle += newsteerAngle - steeringWheelOldAngle;
			}
			
			if(steeringWheelsteerAngle > steeringWheelMaximumsteerAngle)
				steeringWheelsteerAngle = steeringWheelMaximumsteerAngle;
			else if(steeringWheelsteerAngle < -steeringWheelMaximumsteerAngle)
				steeringWheelsteerAngle = -steeringWheelMaximumsteerAngle;
			
			steeringWheelOldAngle = newsteerAngle;
			
		}else{
			
			foreach(Touch touch in Input.touches){
				if(touch.phase == TouchPhase.Began){
					if(steeringWheelTextureRect.Contains( new Vector2( touch.position.x, Screen.height - touch.position.y))){
						steeringWheelIsTouching = true;
						steeringWheelOldAngle = Vector2.Angle(Vector2.up, touch.position - steeringWheelWheelCenter);
						touchId = touch.fingerId;
					}
				}
			}
			
			if(!Mathf.Approximately(0f, steeringWheelsteerAngle)){
				float deltaAngle = steeringWheelResetPosspeed * Time.deltaTime;
				
				if(Mathf.Abs(deltaAngle) > Mathf.Abs(steeringWheelsteerAngle)){
					steeringWheelsteerAngle  = 0f;
					return;
				}
				
				if(steeringWheelsteerAngle > 0f)
					steeringWheelsteerAngle -= deltaAngle;
				else
					steeringWheelsteerAngle += deltaAngle;
			}
			
		}

		steerInput = -steeringWheelsteerAngle / steeringWheelMaximumsteerAngle;
		
	}
	#region PlayerInputs
	public void KeyboardControlling (){

		//Start Or Stop The Engine.
		if(Input.GetKeyDown(KeyCode.I))
			KillOrStartEngine();

		//Motor Input.
		if(!changingGear){
			gasInput = Mathf.Lerp(gasInput, Mathf.Clamp01(Input.GetAxis("Vertical")), Time.deltaTime * 10f);
		}else{
			gasInput = Mathf.Lerp(gasInput, 0, Time.deltaTime * 10f);
		}

		brakeInput = Mathf.Lerp(brakeInput, -Mathf.Clamp(Input.GetAxis("Vertical"), -1f, 0f), Time.deltaTime * 10f);
		handbrakeInput = Input.GetAxis("Jump");

		//Steering Input.
		if(Mathf.Abs(Input.GetAxis("Horizontal")) > .05f)
			steerInput = Mathf.Lerp (steerInput, Input.GetAxis("Horizontal"), Time.deltaTime * 20f);
		else
			steerInput = Mathf.Lerp (steerInput, Input.GetAxis("Horizontal"), Time.deltaTime * 20f);

		ApplySteering(FrontLeftWheelCollider);
		ApplySteering(FrontRightWheelCollider);
		
		//Boost Input.
		if(Input.GetButton("Fire2"))
			boostInput = 1.5f;
		else
			boostInput = 1f;

	}
	
	public void NGUIControlling (){
		
		//Motor Input.
		if(!changingGear)
			gasInput = gasPedal.input;
		else
			gasInput = 0;

		//Brake Input.
		brakeInput = brakePedal.input;
		
		//Steer Input.
		if(!useAccelerometerForSteer && !steeringWheelControl)
			steerInput = rightArrow.input + (-leftArrow.input);
		
		//Handbrake Input.
		if(handbrakeGui.input > .1f)
			handbrakeInput = 1;
		else
			handbrakeInput = 0;
		
		//Boost Input.
		if(boostGui)
			boostInput = Mathf.Clamp(boostGui.input * 2f, 1f, 1.5f);
		
	}

	public void UIControlling (){
		
		//Motor Input.
		if(!changingGear)
			gasInput = gasPedalUI.input;
		else
			gasInput = 0;

		//Brake Input.
		brakeInput = brakePedalUI.input;
		
		//Steer Input.
		if(!useAccelerometerForSteer && !steeringWheelControl)
			steerInput = rightArrowUI.input + (-leftArrowUI.input);
		
		//Handbrake Input.
		if(handbrakeUI.input > .1f)
			handbrakeInput = 1;
		else if(handbrakeUI.input < .9f)
			handbrakeInput = 0;
		
		//Boost Input.
		if(boostUI)
			boostInput = Mathf.Clamp(boostUI.input * 2f, 1f, 1.5f);
		
	}
	#endregion
	public void GearBox (){

		if(speed <= gearSpeed[0]){
			if(handbrakeInput < .1f){
				float wheelRPM = ((Mathf.Abs((FrontLeftWheelCollider.rpm * FrontLeftWheelCollider.radius) + (FrontRightWheelCollider.rpm * FrontRightWheelCollider.radius)) / 2f) / 3.25f);
				if(!reversing)
					clutchInput = Mathf.Lerp(clutchInput, (Mathf.Lerp(1f, (Mathf.Lerp(.25f, 0f, wheelRPM / gearSpeed[0])), gasInput)), Time.deltaTime * 50f);
				else
					clutchInput = Mathf.Lerp(clutchInput, (Mathf.Lerp(1f, (Mathf.Lerp(.25f, 0f, wheelRPM / gearSpeed[0])), brakeInput)), Time.deltaTime * 50f);
			}else{
				clutchInput = Mathf.Lerp(clutchInput, 1, Time.deltaTime * 10f);
			}
		}else{
			if(changingGear)
				clutchInput = Mathf.Lerp(clutchInput, 1, Time.deltaTime * 10f);
			else
				clutchInput = Mathf.Lerp(clutchInput, 0, Time.deltaTime * 10f);
		}

		if(clutchInput > 1)
			clutchInput = 1;
		if(clutchInput < 0)
			clutchInput = 0;
		
		if(automaticGear){

			if(currentGear < totalGears - 1 && !changingGear){
				if(speed >= (gearSpeed[currentGear] * 1.1f) && FrontLeftWheelCollider.rpm > 0){
					StartCoroutine("ChangingGear", currentGear + 1);
				}
			}
			
			if(currentGear > 0){

				if(!changingGear){

					if(speed < (gearSpeed[currentGear - 1] * .9f)){
						StartCoroutine("ChangingGear", currentGear - 1);
					}

				}
			}
			
		}else{
			
			if(currentGear < totalGears - 1 && !changingGear){
				if(Input.GetButtonDown("RCCShiftUp")){
					StartCoroutine("ChangingGear", currentGear + 1);
				}
			}
			
			if(currentGear > 0){
				if(Input.GetButtonDown("RCCShiftDown")){
					StartCoroutine("ChangingGear", currentGear - 1);
				}
			}
			
		}
		
	}
	
	IEnumerator ChangingGear(int gear){

		changingGear = true;

		if(gearShiftingClips.Length > 0)
			gearShiftingSound = RCCCreateAudioSource.NewAudioSource(gameObject, "Gear Shifting AudioSource", 5f, maxGearShiftingSoundVolume, gearShiftingClips[UnityEngine.Random.Range(0, gearShiftingClips.Length)], false, true, true);
		
		yield return new WaitForSeconds(.5f);
		changingGear = false;
		currentGear = gear;
		
	}
	
	public void WheelAlign (){
		
		RaycastHit hit;
		WheelHit CorrespondingGroundHit;
		
		
		//Front Left Wheel Transform.
		Vector3 ColliderCenterPointFL = FrontLeftWheelCollider.transform.TransformPoint( FrontLeftWheelCollider.center );
		FrontLeftWheelCollider.GetGroundHit( out CorrespondingGroundHit );
		
		if(Physics.Raycast( ColliderCenterPointFL, -FrontLeftWheelCollider.transform.up, out hit, (FrontLeftWheelCollider.suspensionDistance + FrontLeftWheelCollider.radius) * transform.localScale.y) && !hit.collider.isTrigger && hit.transform.root != transform){
			FrontLeftWheelTransform.transform.position = hit.point + (FrontLeftWheelCollider.transform.up * FrontLeftWheelCollider.radius) * transform.localScale.y;
			float extension = (-FrontLeftWheelCollider.transform.InverseTransformPoint(CorrespondingGroundHit.point).y - FrontLeftWheelCollider.radius) / FrontLeftWheelCollider.suspensionDistance;
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point + FrontLeftWheelCollider.transform.up * (CorrespondingGroundHit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - FrontLeftWheelCollider.transform.forward * CorrespondingGroundHit.forwardSlip, Color.green);
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - FrontLeftWheelCollider.transform.right * CorrespondingGroundHit.sidewaysSlip, Color.red);
		}else{
			FrontLeftWheelTransform.transform.position = ColliderCenterPointFL - (FrontLeftWheelCollider.transform.up * FrontLeftWheelCollider.suspensionDistance) * transform.localScale.y;
		}

		_rotationValueFL += FrontLeftWheelCollider.rpm * ( 6 ) * Time.deltaTime;
		FrontLeftWheelTransform.transform.rotation = FrontLeftWheelCollider.transform.rotation * Quaternion.Euler( _rotationValueFL, FrontLeftWheelCollider.steerAngle, FrontLeftWheelCollider.transform.rotation.z);
		
		
		//Front Right Wheel Transform.
		Vector3 ColliderCenterPointFR = FrontRightWheelCollider.transform.TransformPoint( FrontRightWheelCollider.center );
		FrontRightWheelCollider.GetGroundHit( out CorrespondingGroundHit );
		
		if(Physics.Raycast( ColliderCenterPointFR, -FrontRightWheelCollider.transform.up, out hit, (FrontRightWheelCollider.suspensionDistance + FrontRightWheelCollider.radius) * transform.localScale.y) && !hit.collider.isTrigger && hit.transform.root != transform){
				FrontRightWheelTransform.transform.position = hit.point + (FrontRightWheelCollider.transform.up * FrontRightWheelCollider.radius) * transform.localScale.y;
				float extension = (-FrontRightWheelCollider.transform.InverseTransformPoint(CorrespondingGroundHit.point).y - FrontRightWheelCollider.radius) / FrontRightWheelCollider.suspensionDistance;
				Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point + FrontRightWheelCollider.transform.up * (CorrespondingGroundHit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
				Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - FrontRightWheelCollider.transform.forward * CorrespondingGroundHit.forwardSlip, Color.green);
				Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - FrontRightWheelCollider.transform.right * CorrespondingGroundHit.sidewaysSlip, Color.red);
		}else{
			FrontRightWheelTransform.transform.position = ColliderCenterPointFR - (FrontRightWheelCollider.transform.up * FrontRightWheelCollider.suspensionDistance) * transform.localScale.y;
		}

		_rotationValueFR += FrontRightWheelCollider.rpm * ( 6 ) * Time.deltaTime;
		FrontRightWheelTransform.transform.rotation = FrontRightWheelCollider.transform.rotation * Quaternion.Euler( _rotationValueFR, FrontRightWheelCollider.steerAngle, FrontRightWheelCollider.transform.rotation.z);
		
		
		//Rear Left Wheel Transform.
		Vector3 ColliderCenterPointRL = RearLeftWheelCollider.transform.TransformPoint( RearLeftWheelCollider.center );
		RearLeftWheelCollider.GetGroundHit( out CorrespondingGroundHit );
		
		if(Physics.Raycast( ColliderCenterPointRL, -RearLeftWheelCollider.transform.up, out hit, (RearLeftWheelCollider.suspensionDistance + RearLeftWheelCollider.radius) * transform.localScale.y) && !hit.collider.isTrigger && hit.transform.root != transform){
			RearLeftWheelTransform.transform.position = hit.point + (RearLeftWheelCollider.transform.up * RearLeftWheelCollider.radius) * transform.localScale.y;
			float extension = (-RearLeftWheelCollider.transform.InverseTransformPoint(CorrespondingGroundHit.point).y - RearLeftWheelCollider.radius) / RearLeftWheelCollider.suspensionDistance;
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point + RearLeftWheelCollider.transform.up * (CorrespondingGroundHit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - RearLeftWheelCollider.transform.forward * CorrespondingGroundHit.forwardSlip, Color.green);
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - RearLeftWheelCollider.transform.right * CorrespondingGroundHit.sidewaysSlip, Color.red);
		}else{
			RearLeftWheelTransform.transform.position = ColliderCenterPointRL - (RearLeftWheelCollider.transform.up * RearLeftWheelCollider.suspensionDistance) * transform.localScale.y;
		}

		RearLeftWheelTransform.transform.rotation = RearLeftWheelCollider.transform.rotation * Quaternion.Euler( _rotationValueRL, 0, RearLeftWheelCollider.transform.rotation.z);
		_rotationValueRL += RearLeftWheelCollider.rpm * ( 6 ) * Time.deltaTime;
		
		
		//Rear Right Wheel Transform.
		Vector3 ColliderCenterPointRR = RearRightWheelCollider.transform.TransformPoint( RearRightWheelCollider.center );
		RearRightWheelCollider.GetGroundHit( out CorrespondingGroundHit );
		
		if(Physics.Raycast( ColliderCenterPointRR, -RearRightWheelCollider.transform.up, out hit, (RearRightWheelCollider.suspensionDistance + RearRightWheelCollider.radius) * transform.localScale.y) && !hit.collider.isTrigger && hit.transform.root != transform){
			RearRightWheelTransform.transform.position = hit.point + (RearRightWheelCollider.transform.up * RearRightWheelCollider.radius) * transform.localScale.y;
			float extension = (-RearRightWheelCollider.transform.InverseTransformPoint(CorrespondingGroundHit.point).y - RearRightWheelCollider.radius) / RearRightWheelCollider.suspensionDistance;
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point + RearRightWheelCollider.transform.up * (CorrespondingGroundHit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - RearRightWheelCollider.transform.forward * CorrespondingGroundHit.forwardSlip, Color.green);
			Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - RearRightWheelCollider.transform.right * CorrespondingGroundHit.sidewaysSlip, Color.red);
		}else{
			RearRightWheelTransform.transform.position = ColliderCenterPointRR - (RearRightWheelCollider.transform.up * RearRightWheelCollider.suspensionDistance) * transform.localScale.y;
		}

		RearRightWheelTransform.transform.rotation = RearRightWheelCollider.transform.rotation * Quaternion.Euler( _rotationValueRR, 0, RearRightWheelCollider.transform.rotation.z);
		_rotationValueRR += RearRightWheelCollider.rpm * ( 6 ) * Time.deltaTime;

		//Extra Wheels Transforms.
		if(ExtraRearWheelsCollider.Length > 0){
			
			for(int i = 0; i < ExtraRearWheelsCollider.Length; i++){
				
				Vector3 ColliderCenterPointExtra = ExtraRearWheelsCollider[i].transform.TransformPoint( ExtraRearWheelsCollider[i].center );
				
				if(Physics.Raycast( ColliderCenterPointExtra, -ExtraRearWheelsCollider[i].transform.up, out hit, (ExtraRearWheelsCollider[i].suspensionDistance + ExtraRearWheelsCollider[i].radius) * transform.localScale.y) && !hit.collider.isTrigger && hit.transform.root != transform){
					ExtraRearWheelsTransform[i].transform.position = hit.point + (ExtraRearWheelsCollider[i].transform.up * ExtraRearWheelsCollider[i].radius) * transform.localScale.y;
				}else{
					ExtraRearWheelsTransform[i].transform.position = ColliderCenterPointExtra - (ExtraRearWheelsCollider[i].transform.up * ExtraRearWheelsCollider[i].suspensionDistance) * transform.localScale.y;
				}
				ExtraRearWheelsTransform[i].transform.rotation = ExtraRearWheelsCollider[i].transform.rotation * Quaternion.Euler( rotationValueExtra[i], 0, ExtraRearWheelsCollider[i].transform.rotation.z);
				rotationValueExtra[i] += ExtraRearWheelsCollider[i].rpm * ( 6 ) * Time.deltaTime;
				
			}
			
		}
		
		//Driver SteeringWheel Transform.
		if(SteeringWheel)
			SteeringWheel.transform.rotation = transform.rotation * Quaternion.Euler(20, 0, (FrontLeftWheelCollider.steerAngle) * -6);
		
	}

	public void WheelCamber (WheelCollider wc){
		
		WheelHit CorrespondingGroundHit;
		Vector3 wheelLocalEuler;

		wc.GetGroundHit(out CorrespondingGroundHit); 
		float handling = Mathf.Lerp (-2f, 2f, CorrespondingGroundHit.force / 7500f);

		if(wc.transform.localPosition.x < 0)
			wheelLocalEuler = new Vector3(wc.transform.localEulerAngles.x, wc.transform.localEulerAngles.y, (-handling));
		else
			wheelLocalEuler = new Vector3(wc.transform.localEulerAngles.x, wc.transform.localEulerAngles.y, (handling));

		Quaternion wheelCamber = Quaternion.Euler(wheelLocalEuler);
		wc.transform.localRotation = wheelCamber;
		
	}
	
	public void DashboardGUI (){

		if(_dashboardType == DashboardType.NGUIDashboard){
		
			if(!UIInputs){
				Debug.LogError("If you gonna use NGUI Dashboard, your NGUI Root must have ''RCCNGUIDashboardInputs''. First be sure your NGUI Root has ''RCCNGUIDashboardInputs.cs''.");
				dashBoard = false;
				return;
			}
			
			UIInputs.RPM = engineRPM;
			UIInputs.KMH = speed;
			UIInputs.Gear = FrontLeftWheelCollider.rpm > -10 ? currentGear : -1f;
			
			RPMNeedleRotation = Mathf.Lerp (minimumRPMNeedleAngle, maximumRPMNeedleAngle, (engineRPM - minEngineRPM / 1.5f) / (maxEngineRPM + minEngineRPM));
			KMHNeedleRotation = Mathf.Lerp (minimumKMHNeedleAngle, maximumKMHNeedleAngle, speed / maxspeed);
			smoothedNeedleRotation = Mathf.Lerp (smoothedNeedleRotation, RPMNeedleRotation, Time.deltaTime * 5);
			
			RPMNeedleNGUI.transform.eulerAngles = new Vector3(RPMNeedleNGUI.transform.eulerAngles.x ,RPMNeedleNGUI.transform.eulerAngles.y, -smoothedNeedleRotation);
			KMHNeedleNGUI.transform.eulerAngles = new Vector3(KMHNeedleNGUI.transform.eulerAngles.x ,KMHNeedleNGUI.transform.eulerAngles.y, -KMHNeedleRotation);

		}

		if(_dashboardType == DashboardType.UIDashboard){
			
			if(!UIInputs){
				Debug.LogError("If you gonna use UI Dashboard, your Canvas Root must have ''RCCUIDashboardInputs''. First be sure your Canvas Root has ''RCCUIDashboardInputs.cs''.");
				dashBoard = false;
				return;
			}
			
			UIInputs.RPM = engineRPM;
			UIInputs.KMH = speed;
			UIInputs.Gear = FrontLeftWheelCollider.rpm > -10 ? currentGear : -1f;
			
			RPMNeedleRotation = Mathf.Lerp (minimumRPMNeedleAngle, maximumRPMNeedleAngle, (engineRPM - minEngineRPM / 1.5f) / (maxEngineRPM + minEngineRPM));
			KMHNeedleRotation = Mathf.Lerp (minimumKMHNeedleAngle, maximumKMHNeedleAngle, speed / maxspeed);
			smoothedNeedleRotation = Mathf.Lerp (smoothedNeedleRotation, RPMNeedleRotation, Time.deltaTime * 5);
			
			RPMNeedle.transform.eulerAngles = new Vector3(RPMNeedle.transform.eulerAngles.x ,RPMNeedle.transform.eulerAngles.y, -smoothedNeedleRotation);
			KMHNeedle.transform.eulerAngles = new Vector3(KMHNeedle.transform.eulerAngles.x ,KMHNeedle.transform.eulerAngles.y, -KMHNeedleRotation);
			
		}
		
	}
	
	public void SmokeWeedEveryday (GroundMaterial ground) {

		for(int i = 0; i < allWheelColliders.Length; i++){

			WheelHit CorrespondingGroundHit;
			allWheelColliders[i].GetGroundHit(out CorrespondingGroundHit);

			if(_wheelSlipAsphalt.Count > 0 && ground == GroundMaterial.Asphalt){

				if(Mathf.Abs(CorrespondingGroundHit.sidewaysSlip) > .25f || Mathf.Abs(CorrespondingGroundHit.forwardSlip) > .5f){
					if(!_wheelSlipAsphalt[i].enableEmission && speed > 1)
						_wheelSlipAsphalt[i].enableEmission = true;
				}else{
					if(_wheelSlipAsphalt[i].enableEmission)
						_wheelSlipAsphalt[i].enableEmission = false;
				}

			}else if(_wheelSlipAsphalt.Count > 0){
				if(_wheelSlipAsphalt[i].enableEmission)
					_wheelSlipAsphalt[i].enableEmission = false;
			}

			if(_wheelSlipSand.Count > 0 && ground == GroundMaterial.Sand){
				
				if(Mathf.Abs(CorrespondingGroundHit.sidewaysSlip) > .25f || Mathf.Abs(CorrespondingGroundHit.forwardSlip) > .5f){
					if(!_wheelSlipSand[i].enableEmission && speed > 1)
						_wheelSlipSand[i].enableEmission = true;
				}else{
					if(_wheelSlipSand[i].enableEmission)
						_wheelSlipSand[i].enableEmission = false;
				}
				
			}else if(_wheelSlipSand.Count > 0){
				if(_wheelSlipSand[i].enableEmission)
					_wheelSlipSand[i].enableEmission = false;
			}

		}
		
		if(exhaustGas.Length > 0 && engineRunning){
			foreach(ParticleSystem p in exhaustGas){
				if(speed < 20){
					if(!p.enableEmission)
						p.enableEmission = true;
					if(gasInput > .05f)
						p.emissionRate = 15;
					else
						p.emissionRate = 4;
				}else{
					if(p.enableEmission)
						p.enableEmission = false;
				}
			}
		}else if(exhaustGas.Length > 0){
			foreach(ParticleSystem p in exhaustGas){
				if(p.enableEmission)
					p.enableEmission = false;
			}
		}
		
	}
	
	public void GroundPhysicsMaterial (){

		if(!skidSound)
			return;
		
		WheelHit CorrespondingGroundHitF;
		FrontRightWheelCollider.GetGroundHit( out CorrespondingGroundHitF );
		
		WheelHit CorrespondingGroundHitR;
		RearRightWheelCollider.GetGroundHit( out CorrespondingGroundHitR );

		RaycastHit hit;
		float minimumSpeedForSkid = 0f;

		if(Physics.Raycast(FrontRightWheelCollider.transform.position, -FrontRightWheelCollider.transform.up, out hit)){

			if(UseTerrainSplatMapForGroundPhysic && hit.transform.gameObject.GetComponent<TerrainCollider>()){
				if(TerrainSurface.GetTextureMix(transform.position)[0] > .5f){
					_groundMaterial = GroundMaterial.Asphalt;
					SetSkidVolume(asphaltSkidClip, _groundMaterial, 0f);
				}else if(TerrainSurface.GetTextureMix(transform.position)[1] > .5f){
					minimumSpeedForSkid = Mathf.Lerp (0f, .35f, speed / 35f);
					_groundMaterial = GroundMaterial.Sand;
					SetSkidVolume(sandSkidClip, _groundMaterial, minimumSpeedForSkid);
				}else if(TerrainSurface.GetTextureMix(transform.position)[2] > .5f){
					minimumSpeedForSkid = Mathf.Lerp (0f, .35f, speed / 35f);
					_groundMaterial = GroundMaterial.Grass;
					SetSkidVolume(grassSkidClip, _groundMaterial, minimumSpeedForSkid);
				}
				return;
			}

			if(hit.collider.material.name == grassPhysicsMaterial.name + " (Instance)"){
				minimumSpeedForSkid = Mathf.Lerp (0f, .35f, speed / 35f);
				_groundMaterial = GroundMaterial.Grass;
				SetSkidVolume(grassSkidClip, _groundMaterial, minimumSpeedForSkid);
			}else if(hit.collider.material.name == sandPhysicsMaterial.name + " (Instance)"){
				minimumSpeedForSkid = Mathf.Lerp (0f, .35f, speed / 35f);
				_groundMaterial = GroundMaterial.Sand;
				SetSkidVolume(sandSkidClip, _groundMaterial, minimumSpeedForSkid);
			}else{
				_groundMaterial = GroundMaterial.Asphalt;
				SetSkidVolume(asphaltSkidClip, _groundMaterial, 0f);
			}

		}else{
			_groundMaterial = GroundMaterial.Asphalt;
			SetSkidVolume(asphaltSkidClip, _groundMaterial, 0f);
		}

	}

	public void SetSkidVolume(AudioClip clip, GroundMaterial ground, float minSpeed){

		WheelHit CorrespondingGroundHitF;
		FrontRightWheelCollider.GetGroundHit( out CorrespondingGroundHitF );
		
		WheelHit CorrespondingGroundHitR;
		RearRightWheelCollider.GetGroundHit( out CorrespondingGroundHitR );

		if(skidSound.clip != clip){
			skidSound.clip = clip;
			skidSound.Play();
		}

		if(ground == GroundMaterial.Asphalt){
			if(Mathf.Abs(CorrespondingGroundHitF.sidewaysSlip) > .25f || Mathf.Abs(CorrespondingGroundHitR.sidewaysSlip) > .25f || Mathf.Abs(CorrespondingGroundHitF.forwardSlip) > .7f || Mathf.Abs(CorrespondingGroundHitR.forwardSlip) > .7f){
				if(rigid.velocity.magnitude > 1f)
					skidSound.volume = Mathf.Clamp((Mathf.Abs(CorrespondingGroundHitF.sidewaysSlip) + Mathf.Abs(CorrespondingGroundHitR.sidewaysSlip) / 10f) + (Mathf.Abs(CorrespondingGroundHitF.forwardSlip) + Mathf.Abs(CorrespondingGroundHitR.forwardSlip) / 2f), 0f, 1f);
				else
					skidSound.volume -= Time.deltaTime * 1.5f;
			}else{
				skidSound.volume -= Time.deltaTime * 1.5f;
			}
		}

		if(ground == GroundMaterial.Sand){
			if(Mathf.Abs(CorrespondingGroundHitF.sidewaysSlip) > .2f || Mathf.Abs(CorrespondingGroundHitR.sidewaysSlip) > .2f || Mathf.Abs(CorrespondingGroundHitF.forwardSlip) > .7f || Mathf.Abs(CorrespondingGroundHitR.forwardSlip) > .7f || rigid.velocity.magnitude > 5f){
				if(rigid.velocity.magnitude > 1f)
					skidSound.volume = Mathf.Clamp(((Mathf.Abs(CorrespondingGroundHitF.sidewaysSlip) + Mathf.Abs(CorrespondingGroundHitR.sidewaysSlip)) / 10f) + (Mathf.Abs(CorrespondingGroundHitF.forwardSlip) + Mathf.Abs(CorrespondingGroundHitR.forwardSlip) / 2f), minSpeed, 1f);
				else
					skidSound.volume -= Time.deltaTime * 1.25f;
			}else{
				skidSound.volume -= Time.deltaTime * 1.25f;
			}
		}

		if(ground == GroundMaterial.Grass){
			if(Mathf.Abs(CorrespondingGroundHitF.sidewaysSlip) > .2f || Mathf.Abs(CorrespondingGroundHitR.sidewaysSlip) > .2f || Mathf.Abs(CorrespondingGroundHitF.forwardSlip) > .7f || Mathf.Abs(CorrespondingGroundHitR.forwardSlip) > .7f || rigid.velocity.magnitude > 5f){
				if(rigid.velocity.magnitude > 1f)
					skidSound.volume = Mathf.Clamp(((Mathf.Abs(CorrespondingGroundHitF.sidewaysSlip) + Mathf.Abs(CorrespondingGroundHitR.sidewaysSlip)) / 10f) + (Mathf.Abs(CorrespondingGroundHitF.forwardSlip) + Mathf.Abs(CorrespondingGroundHitR.forwardSlip) / 2f), minSpeed, 1f);
				else
					skidSound.volume -= Time.deltaTime * 1.25f;
			}else{
				skidSound.volume -= Time.deltaTime * 1.25f;
			}
		}

	}
	
	public void ResetCar (){
		
		if(speed < 5 && !rigid.isKinematic){
			
			if(transform.eulerAngles.z < 300 && transform.eulerAngles.z > 60){
				resetTime += Time.deltaTime;
				if(resetTime > 3){
					transform.rotation = Quaternion.identity;
					transform.position = new Vector3(transform.position.x, transform.position.y + 3, transform.position.z);
					resetTime = 0f;
				}
			}
			
			if(transform.eulerAngles.x < 300 && transform.eulerAngles.x > 60){
				resetTime += Time.deltaTime;
				if(resetTime > 3){
					transform.rotation = Quaternion.identity;
					transform.position = new Vector3(transform.position.x, transform.position.y + 3, transform.position.z);
					resetTime = 0f;
				}
			}
			
		}
		
	}
	
	void OnCollisionEnter (Collision collision){
		
		if (collision.contacts.Length < 1 || collision.relativeVelocity.magnitude < minimumCollisionForce)
			return;

			if(crashClips.Length > 0){
				if (collision.contacts[0].thisCollider.gameObject.transform != transform.parent){
					crashSound = RCCCreateAudioSource.NewAudioSource(gameObject, "Crash Sound AudioSource", 5, maxCrashSoundVolume, crashClips[UnityEngine.Random.Range(0, crashClips.Length)], false, true, true);
				}
			}

		if(useDamage){

			CollisionParticles(collision.contacts[0].point);
			
			Vector3 colRelVel = collision.relativeVelocity;
			colRelVel *= 1f - Mathf.Abs(Vector3.Dot(transform.up,collision.contacts[0].normal));
			
			float cos = Mathf.Abs(Vector3.Dot(collision.contacts[0].normal, colRelVel.normalized));
			
			if (colRelVel.magnitude * cos >= minimumCollisionForce){
				
				sleep = false;
				
				localVector = transform.InverseTransformDirection(colRelVel) * (damageMultiplier / 50f);
				
				if (originalMeshData == null)
					LoadOriginalMeshData();
				
				for (int i = 0; i < deformableMeshFilters.Length; i++){
					DeformMesh(deformableMeshFilters[i].mesh, originalMeshData[i].meshVerts, collision, cos / deformableMeshFilters[i].transform.lossyScale.x, deformableMeshFilters[i].transform, rot, deformableMeshFilters[i].transform.lossyScale.x);
				}
				
			}

		}

	}
	
	public void Chassis (){

		rigid.centerOfMass = new Vector3((COM.localPosition.x) * transform.localScale.x , (Mathf.Lerp(COM.localPosition.y, FrontLeftWheelCollider.transform.localPosition.y, speed / 120f)) * transform.localScale.y , (COM.localPosition.z) * transform.localScale.z);

		verticalLean = Mathf.Clamp(Mathf.Lerp (verticalLean, rigid.angularVelocity.x * chassisVerticalLean, Time.deltaTime * 3f), -3.0f, 3.0f);

		WheelHit CorrespondingGroundHit;
		RearRightWheelCollider.GetGroundHit(out CorrespondingGroundHit);

		float normalizedLeanAngle = Mathf.Clamp(CorrespondingGroundHit.sidewaysSlip, -1f, 1f);

		if(normalizedLeanAngle > 0f)
			normalizedLeanAngle = 1;
		else
			normalizedLeanAngle = -1;

		if(transform.InverseTransformDirection(rigid.velocity).z >= 0)
			horizontalLean = Mathf.Clamp(Mathf.Lerp (horizontalLean, (transform.InverseTransformDirection(rigid.angularVelocity).y) * chassisHorizontalLean, Time.deltaTime * 3f), -3f, 3f);
		else
			horizontalLean = Mathf.Clamp(Mathf.Lerp (horizontalLean, (Mathf.Abs (transform.InverseTransformDirection(rigid.angularVelocity).y) * -normalizedLeanAngle) * chassisHorizontalLean, Time.deltaTime * 3f), -3.0f, 3.0f);

		if(float.IsNaN(verticalLean) || float.IsNaN(horizontalLean) || float.IsInfinity(verticalLean) || float.IsInfinity(horizontalLean) || Mathf.Approximately(verticalLean, 0f) || Mathf.Approximately(horizontalLean, 0f))
			return;

		Quaternion target = Quaternion.Euler(verticalLean, chassis.transform.localRotation.y + (rigid.angularVelocity.z), horizontalLean);
		chassis.transform.localRotation = target;

	}
	
	public void Lights (){

		if(brakeInput > .1f)
			brakeLightInput = Mathf.Lerp(brakeLightInput, 1f, Time.deltaTime * 5f);
		else
			brakeLightInput = Mathf.Lerp(brakeLightInput, 0f, Time.deltaTime * 5f);

		if(Input.GetKeyDown(KeyCode.L) && !AIController){
			headLightsOn = !headLightsOn;
		}
		
		for(int i = 0; i < brakeLights.Length; i++){
			
			if(!reversing)
				brakeLights[i].intensity = brakeLightInput;
			else
				brakeLights[i].intensity = 0f;
			
		}
		
		for(int i = 0; i < headLights.Length; i++){
			
			if(headLightsOn)
				headLights[i].enabled = true;
			else
				headLights[i].enabled = false;
			
		}
		
		for(int i = 0; i < reverseLights.Length; i++){
			
			if(!reversing)
				reverseLights[i].intensity = Mathf.Lerp (reverseLights[i].intensity, 0f, Time.deltaTime * 5f);
			else
				reverseLights[i].intensity = brakeLightInput;
			
		}
		
	}
	
	void OnGUI (){

		GUI.skin.label.fontSize = 12;
		GUI.skin.box.fontSize = 12;
		Matrix4x4 orgRotation = GUI.matrix;
		
		if(canControl){
			
			if(useAccelerometerForSteer && mobileController){
				if(_mobileControllerType == MobileGUIType.NGUIController){
					leftArrow.gameObject.SetActive(false);
					rightArrow.gameObject.SetActive(false);
					handbrakeGui.gameObject.SetActive(true);
					brakePedal.transform.position = leftArrow.transform.position;
				}
				if(_mobileControllerType == MobileGUIType.UIController){
					leftArrowUI.gameObject.SetActive(false);
					rightArrowUI.gameObject.SetActive(false);
					handbrakeUI.gameObject.SetActive(true);
					brakePedalUI.transform.position = leftArrowUI.transform.position;
				}
				steeringWheelControl = false;
			}else if(mobileController){
				if(_mobileControllerType == MobileGUIType.NGUIController){
					gasPedal.gameObject.SetActive(true);
					leftArrow.gameObject.SetActive(true);
					rightArrow.gameObject.SetActive(true);
					handbrakeGui.gameObject.SetActive(true);
					brakePedal.transform.position = defbrakePedalPosition;
				}
				if(_mobileControllerType == MobileGUIType.UIController){
					gasPedalUI.gameObject.SetActive(true);
					leftArrowUI.gameObject.SetActive(true);
					rightArrowUI.gameObject.SetActive(true);
					handbrakeUI.gameObject.SetActive(true);
					brakePedalUI.gameObject.SetActive(true);
					brakePedalUI.transform.position = defbrakePedalPosition;
				}
			}
			
			if(steeringWheelControl && mobileController){

				if(_mobileControllerType == MobileGUIType.NGUIController){
					leftArrow.gameObject.SetActive(false);
					rightArrow.gameObject.SetActive(false);
				}
				if(_mobileControllerType == MobileGUIType.UIController){
					leftArrowUI.gameObject.SetActive(false);
					rightArrowUI.gameObject.SetActive(false);
				}

				GUIUtility.RotateAroundPivot( -steeringWheelsteerAngle , steeringWheelTextureRect.center + steeringWheelPivotPos );
				GUI.DrawTexture( steeringWheelTextureRect, steeringWheelTexture );
				GUI.matrix = orgRotation;

			}
			
			if( demoGUI ) {

				GUI.backgroundColor = Color.black;
				float guiWidth = Screen.width/2 - 200;
				
				GUI.Box(new Rect(Screen.width-410 - guiWidth, 10, 400, 220), "");
				GUI.Label(new Rect(Screen.width-400 - guiWidth, 10, 400, 150), "Engine RPM : " + Mathf.CeilToInt(engineRPM));
				GUI.Label(new Rect(Screen.width-400 - guiWidth, 30, 400, 150), "speed : " + Mathf.CeilToInt(speed));
				GUI.Label(new Rect(Screen.width-400 - guiWidth, 190, 400, 150), "Horizontal Tilt : " + Input.acceleration.x);
				GUI.Label(new Rect(Screen.width-400 - guiWidth, 210, 400, 150), "Vertical Tilt : " + Input.acceleration.y);
				if(_wheelTypeChoise == WheelType.FWD){
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 50, 400, 150), "Left Wheel RPM : " + Mathf.CeilToInt(FrontLeftWheelCollider.rpm));
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 70, 400, 150), "Right Wheel RPM : " + Mathf.CeilToInt(FrontRightWheelCollider.rpm));
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 90, 400, 150), "Left Wheel Torque : " + Mathf.CeilToInt(FrontLeftWheelCollider.motorTorque));
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 110, 400, 150), "Right Wheel Torque : " + Mathf.CeilToInt(FrontRightWheelCollider.motorTorque));
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 130, 400, 150), "Left Wheel brake : " + Mathf.CeilToInt(FrontLeftWheelCollider.brakeTorque));
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 150, 400, 150), "Right Wheel brake : " + Mathf.CeilToInt(FrontRightWheelCollider.brakeTorque));
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 170, 400, 150), "Steer Angle : " + Mathf.CeilToInt(FrontLeftWheelCollider.steerAngle));
				}
				if(_wheelTypeChoise == WheelType.RWD || _wheelTypeChoise == WheelType.AWD){
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 50, 400, 150), "Left Wheel RPM : " + Mathf.CeilToInt(RearLeftWheelCollider.rpm));
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 70, 400, 150), "Right Wheel RPM : " + Mathf.CeilToInt(RearRightWheelCollider.rpm));
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 90, 400, 150), "Left Wheel Torque : " + Mathf.CeilToInt(RearLeftWheelCollider.motorTorque));
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 110, 400, 150), "Right Wheel Torque : " + Mathf.CeilToInt(RearRightWheelCollider.motorTorque));
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 130, 400, 150), "Left Wheel brake : " + Mathf.CeilToInt(FrontLeftWheelCollider.brakeTorque));
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 150, 400, 150), "Right Wheel brake : " + Mathf.CeilToInt(FrontRightWheelCollider.brakeTorque));
					GUI.Label(new Rect(Screen.width-400 - guiWidth, 170, 400, 150), "Steer Angle : " + Mathf.CeilToInt(FrontLeftWheelCollider.steerAngle));
				}
				
				GUI.backgroundColor = Color.green;
				GUI.Button (new Rect(Screen.width-30 - guiWidth, 165, 10, Mathf.Clamp((-gasInput * 100), -100, 0)), "");

				GUI.backgroundColor = Color.red;
				GUI.Button (new Rect(Screen.width-45 - guiWidth, 165, 10, Mathf.Clamp((-brakeInput * 100), -100, 0)), "");

				GUI.backgroundColor = Color.blue;
				GUI.Button (new Rect(Screen.width-60 - guiWidth, 165, 10, Mathf.Clamp((-clutchInput * 100), -100, 0)), "");

				if(mobileController){

					if(GUI.Button(new Rect(Screen.width - 275, 200, 250, 50), "Use Accelerometer \n For Steer")){
						if(useAccelerometerForSteer)
							useAccelerometerForSteer = false;
						else useAccelerometerForSteer = true;
					}
					
					if(GUI.Button(new Rect(Screen.width - 275, 275, 250, 50), "Use Steering Wheel")){
						if(steeringWheelControl)
							steeringWheelControl = false;
						else steeringWheelControl = true;
					}

				}

			}
			
		}
		
	}
	
} 
