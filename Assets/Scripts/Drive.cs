using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class Drive : MonoBehaviour {
	
	public float mSpeed = 10.0f;
	public float mRotSpeed = 100.0f;
		
	// Update is called once per frame
	void Update () {
		float translation = CrossPlatformInputManager.GetAxis ("Vertical") * mSpeed;
		float rotation = CrossPlatformInputManager.GetAxis ("Horizontal") * mRotSpeed;

		translation *= Time.deltaTime;
		rotation *= Time.deltaTime;
		transform.Translate (0, 0, translation);
		transform.Rotate (0, rotation, 0);
	}
}
