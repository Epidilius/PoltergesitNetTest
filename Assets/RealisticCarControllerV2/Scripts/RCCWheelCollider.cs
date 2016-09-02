//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2015 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class RCCWheelCollider : MonoBehaviour {
	
	private RCCCarControllerV2 carController;
	private Rigidbody carRigid;
	private float startSlipValue = .25f;
	private RCCSkidmarks skidmarks = null;
	private int lastSkidmark = -1;
	private WheelCollider wheelCollider;
	
	private float wheelSlipAmountSideways;
	private float wheelSlipAmountForward;

	//Physics Materials.
	private PhysicMaterial grassPhysicsMaterial;
	private PhysicMaterial sandPhysicsMaterial;

	//WheelFriction Curves and Stiffness.
	private WheelFrictionCurve forwardFrictionCurve;
	private WheelFrictionCurve sidewaysFrictionCurve;

	private float defForwardStiffness = 0f;
	private float defSidewaysStiffness = 0f;
	
	void  Start (){

		wheelCollider = GetComponent<WheelCollider>();
		carController = transform.root.GetComponent<RCCCarControllerV2>();
		carRigid = carController.GetComponent<Rigidbody>();

		if(FindObjectOfType(typeof(RCCSkidmarks)))
			skidmarks = FindObjectOfType(typeof(RCCSkidmarks)) as RCCSkidmarks;
		else
			Debug.Log("No skidmarks object found. Skidmarks will not be drawn. Drag ''RCCSkidmarksManager'' from Prefabs folder, and drop on to your existing scene...");

		grassPhysicsMaterial = Resources.Load("RCCGrassPhysics") as PhysicMaterial;
		sandPhysicsMaterial = Resources.Load("RCCSandPhysics") as PhysicMaterial;

		forwardFrictionCurve = GetComponent<WheelCollider>().forwardFriction;
		sidewaysFrictionCurve = GetComponent<WheelCollider>().sidewaysFriction;

		defForwardStiffness = forwardFrictionCurve.stiffness;
		defSidewaysStiffness = sidewaysFrictionCurve.stiffness;

	}
	
	void  FixedUpdate (){

		if(skidmarks){

			WheelHit GroundHit;
			wheelCollider.GetGroundHit(out GroundHit);
			
			wheelSlipAmountSideways = Mathf.Abs(GroundHit.sidewaysSlip);
			wheelSlipAmountForward = Mathf.Abs(GroundHit.forwardSlip);
			
			if (wheelSlipAmountSideways > startSlipValue || wheelSlipAmountForward > .5f){
				
				Vector3 skidPoint = GroundHit.point + 2f * (carRigid.velocity) * Time.deltaTime;

				if(carRigid.velocity.magnitude > 1f)
					lastSkidmark = skidmarks.AddSkidMark(skidPoint, GroundHit.normal, (wheelSlipAmountSideways / 2f) + (wheelSlipAmountForward / 2.5f), lastSkidmark);
				else
					lastSkidmark = -1;

			}

			else{

				lastSkidmark = -1;

			}
			
		}

		RaycastHit hit;

		if(Physics.Raycast(transform.position, -transform.up, out hit)){

			if(carController.UseTerrainSplatMapForGroundPhysic && hit.transform.gameObject.GetComponent<TerrainCollider>()){
				if(TerrainSurface.GetTextureMix(transform.position)[0] > .5f)
					SetWheelStiffnessByGroundPhysic(1f);
				else if(TerrainSurface.GetTextureMix(transform.position)[1] > .5f)
					SetWheelStiffnessByGroundPhysic(2f);
				else if(TerrainSurface.GetTextureMix(transform.position)[2] > .5f)
					SetWheelStiffnessByGroundPhysic(3f);
				return;
			}

			if(hit.collider.material.name == grassPhysicsMaterial.name + " (Instance)"){
				SetWheelStiffnessByGroundPhysic(3f);
			}else	if(hit.collider.material.name == sandPhysicsMaterial.name + " (Instance)"){
				SetWheelStiffnessByGroundPhysic(2f);
			}else{
				SetWheelStiffnessByGroundPhysic(1f);
			}

		}

	}

	public void SetWheelStiffnessByGroundPhysic(float stiffnessDivider){
		
		forwardFrictionCurve.stiffness = Mathf.Lerp(forwardFrictionCurve.stiffness, defForwardStiffness / stiffnessDivider, Time.deltaTime * 5f);
		sidewaysFrictionCurve.stiffness = Mathf.Lerp(sidewaysFrictionCurve.stiffness, defSidewaysStiffness / stiffnessDivider, Time.deltaTime * 5f);

		wheelCollider.forwardFriction = forwardFrictionCurve;
		wheelCollider.sidewaysFriction = sidewaysFrictionCurve;
		
	}

}