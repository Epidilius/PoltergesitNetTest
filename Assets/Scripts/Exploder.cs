using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Exploder : NetworkBehaviour
{
	public GameObject currentDetonator;
	private int _currentExpIdx = -1;
	public GameObject[] detonatorPrefabs;
	public float explosionLife = 10;
	public float timeScale = 1.0f;
	public float detailLevel = 1.0f;
	private float _spawnTime = -1000;

	private Rect _guiRect;
	
	private void Start()
	{
		if (!currentDetonator) NextExplosion();
		else _currentExpIdx = 0;
	}
	
	private void OnGUI()
	{
		if (!isLocalPlayer)
			return;

		_guiRect = new Rect(7, Screen.height - 180, 250, 200);
		GUILayout.BeginArea(_guiRect);
		
		GUILayout.BeginVertical();
		string expName = currentDetonator.name;
		if (GUILayout.Button(expName + " (Click For Next)"))
		{
			NextExplosion();
		}
		if (GUILayout.Button("Camera Far"))
		{
			Camera.main.transform.position = new Vector3(0, 0, -7);
			Camera.main.transform.eulerAngles = new Vector3(13.5f, 0, 0);
		}
		if (GUILayout.Button("Camera Near"))
		{
			Camera.main.transform.position = new Vector3(0, -8.664466f, 31.38269f);
			Camera.main.transform.eulerAngles = new Vector3(1.213462f, 0, 0);
		}
		
		GUILayout.Label("Time Scale");
		timeScale = GUILayout.HorizontalSlider(timeScale, 0.0f, 1.0f);
		
		GUILayout.Label("Detail Level (re-explode after change)");
		detailLevel = GUILayout.HorizontalSlider(detailLevel, 0.0f, 1.0f);
		
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
	
	private void NextExplosion()
	{
		if (_currentExpIdx >= detonatorPrefabs.Length - 1) _currentExpIdx = 0;
		else _currentExpIdx++;
		currentDetonator = detonatorPrefabs[_currentExpIdx];
	}

	//is this a bug? We can't use the same rect for placing the GUI as for checking if the mouse contains it...
	private Rect checkRect = new Rect(0, 0, 260, 180);
	
	private void Update()
	{
		if (!isLocalPlayer)
			return;
		//keeps the UI in the corner in case of resize... 
		_guiRect = new Rect(7, Screen.height - 150, 250, 200);
		
		//keeps the play button from making an explosion
		//if ((Time.time + _spawnTime) > 0.5f)
		//{
			//don't spawn an explosion if we're using the UI
			if (!checkRect.Contains(Input.mousePosition))
			{
				if (Input.GetMouseButtonDown(0))
				{
					Debug.Log ("Is Local Player: " + base.isLocalPlayer);

					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;
					if (Physics.Raycast(ray, out hit, 1000))
					{
						Detonator dTemp = (Detonator)currentDetonator.GetComponent("Detonator");					
						float offsetSize = dTemp.size/3;
						Vector3 hitPoint = hit.point +
							((Vector3.Scale(hit.normal, new Vector3(offsetSize, offsetSize, offsetSize))));

						CmdSpawnExplosion(hitPoint, _currentExpIdx);
					}
				}
			}
			Time.timeScale = timeScale;
		//}
	}

	[Command]
	public void CmdSpawnExplosion(Vector3 hitPoint, int explosionType)
	{
		currentDetonator = detonatorPrefabs[explosionType];
		GameObject exp = (GameObject) Instantiate(currentDetonator, hitPoint, Quaternion.identity);
		Detonator dTemp = (Detonator)exp.GetComponent("Detonator");
		dTemp.detail = detailLevel;

		Destroy(exp, explosionLife);

		NetworkServer.Spawn(exp);
	}		
}
