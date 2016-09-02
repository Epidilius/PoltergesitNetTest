using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class LocalPlayerSetup : NetworkBehaviour {
	[SyncVar] public string mName = "Player";
	//SyncVar is Server to Client change. When the var is updated on the Server, it gets pushed to all clients. 

	[SyncVar]
	public Color mPlayerColour = Color.white;

	// Use this for initialization
	void Start () {
		if (isLocalPlayer) {
			GetComponent<RCCCarControllerV2>().enabled = true;
			//GetComponent<Drive>().enabled = true;
			//Camera.main.transform.position = this.transform.position - this.transform.forward * 10 + this.transform.up * 5;
			//Camera.main.transform.LookAt(this.transform.position);
			//Camera.main.transform.parent = this.transform;
			SmoothCameraFollow.target = this.transform;
		}

		Renderer[] renderer = GetComponentsInChildren<Renderer>();
		foreach(Renderer rend in renderer) {
			rend.material.color = mPlayerColour;
		}

		//TODO: Replace with ACTUAL spawn points 
		this.transform.position = new Vector3 (Random.Range (-20, 20), 0, Random.Range (-20, 20));
	}

	void OnGUI()
	{
		if (isLocalPlayer) {
			mName = GUI.TextField (new Rect (25, Screen.height - 40, 100, 30), mName);
			if(GUI.Button(new Rect(130, Screen.height - 40, 80, 30), "Change")) {
				CmdChangeName(mName);
			}

			//string ip = MasterServer.ipAddress;
			//GUI.TextField (new Rect (250, Screen.height - 40, 100, 30), ip);
		}
	}

	//Command is the opposite of SyncVar. When a Command is called, the variable will get changed on the server 
	[Command]
	public void CmdChangeName (string aName) {
		mName = aName;
	}
	void Update() {
		//TODO: More efficient way of doing this. Find out if I can try a "On GUI box change" thig. Or, just wait until I have a lobby setup, and do it there. 
		//if (isLocalPlayer) {
			this.GetComponentInChildren<TextMesh> ().text = mName;
		//}
	}
}
