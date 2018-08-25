/** Summary **
 * 
 * ClientSide.cs - collection of variables and methods to form requests for server and
 * processing server responds
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

#region // include
/* MonoBehaviour */
using UnityEngine;
/* SceneManager */
using UnityEngine.SceneManagement;
/* IEnumerator */
using System.Collections;
/* List */
using System.Collections.Generic;
#endregion // include

public class ClientSide : MonoBehaviour {
	
	#region // Singleton
	public static ClientSide instance { private set; get; }
	void Awake() { if(null == instance) instance = this; }
	#endregion // Singleton
	
	#region // Variables
	#if CLIENT
	
	/* Exposed Variables */
	[Header("Connection")]
	public float timeout = 16.0f;
	public string timeoutSceneName = "Timeout";
	
	[Header("Prefabs")]
	public GameObject unitPrefab;
	public GameObject gridPrefab;
	public GameObject wallPrefab;
	public GameObject cornerPrefab;
	
	[Header("References")]
	public Transform instantiatedPrefabsCatalog;
	public NetClient clientScript;
	
	#endif // CLIENT
	
	/* Values determined at Runtime */
	[System.NonSerialized]
	public bool locked = false;
	
	[System.NonSerialized]
	public List<Unit> selectedUnits = new List<Unit>();
	
	[System.NonSerialized]
	public Grid selectedDestination;
	
	#if CLIENT
	
	private WaitForSeconds waitForTimeout;
	
	private Vector3 startPos;
	
	[System.NonSerialized]
	public WorldState state = null;
	
	/* Constant Values */
	private static Vector3 _90dRigth = new Vector3(.0f, 90.0f, .0f);
	
	/* std error strings */
	private const string failedToBuildEntry = "Failed to Build the Scene";
	private const string invalidSizeErrorEntry = "Width or Height Cannot be Equal or Lower Than 0";
	private const string invalidPrefabsErrorEntry = "Wall Prefab and Corner Prefab Must be Both Set";
	private const string noWorldStateEntry = "No World State had been assigned to this Client";
	private const string missingUnitPrefabEntry = "Unit Prefab must be set in order to Spawn Units";
	private const string missingGridPrefabEntry = "Grid Prefab must be set in order to Spawn Grid Floor";
	private const string noNetClientSetEntry = "Net Client instance is not set to send a message";
	
	#endif // CLIENT
	#endregion // Variables
	
	void Start() {
		
		#if CLIENT
		if(null == clientScript) { clientScript = GetComponent<NetClient>(); }
		
		waitForTimeout = new WaitForSeconds(timeout);
		
		StartCoroutine(ExpectTimeout());
		#endif // CLIENT
	}
	
	public void ProcessRespond(object receivedRespond) {
		
		#if CLIENT
		if(null == state && receivedRespond is WorldState) {
			
			StopAllCoroutines();
			
			state = (WorldState)receivedRespond;
			startPos = new Vector3(- state.size / 2.0f + .5f, .0f, - state.size / 2.0f + .5f);
			
			bool built = Build();
			
			/* Building walls failed */
			if(null != instantiatedPrefabsCatalog && !built) {
				
				foreach(Transform instantiated in instantiatedPrefabsCatalog)
					Destroy(instantiated.gameObject);
				
				InfoManager.Log(failedToBuildEntry);
			}
			else if(built) {
				
				SpawnUnits();
				SpawnGridFloor();
			}
			
			return;
		}
		
		if(receivedRespond is Respond) {
			
			StopAllCoroutines();
			
			Respond respond = (Respond)receivedRespond;
			
			if(!respond.success) { return; }
			
			int j = 0;
			foreach(int[] command in respond.commands) {
				
				Unit.GetInstance(command[0]).Receive(command);
				
				for(int i = j; i < state.positions.Length; i ++) {
					
					if(command[0] == state.positions[i]) {
						
						j = i;
						state.positions[i] = command[command.Length - 1];
						break;
					}
				}
			}
			
			return;
		}
		#endif // CLIENT
	}
	
	public Vector3 GetPositionFromIndex(int index) {
		
		#if CLIENT
		return startPos + Vector3.right * (index % state.size) + Vector3.forward * (index / state.size);
		
		#else
		return - Vector3.one;
		#endif // CLIENT
	}
	
	public bool Build() {
		
		#if CLIENT
		int size = state.size;
		
		#region // Error Handling
		if(0 >= size) {
			
			InfoManager.Log(invalidSizeErrorEntry);
			return false;
		}
		
		if(null == wallPrefab || null == cornerPrefab) {
			
			InfoManager.Log(invalidPrefabsErrorEntry);
			return false;
		}
		#endregion // Error Handling
		
		int wallsNum = size - 1;
		float halfSize = (float)size / 2.0f;
		
		Vector3 wallStartPos = new Vector3(- halfSize, .0f, - halfSize);
		
		Vector3 scalerPPN = new Vector3(1.0f, 1.0f, -1.0f);
		Vector3 scalerNPP = new Vector3(-1.0f, 1.0f, 1.0f);
		
		/* Corners */
		Instantiate(cornerPrefab, wallStartPos,
			Quaternion.Euler(_90dRigth * 2.0f),
			instantiatedPrefabsCatalog);
		
		Instantiate(cornerPrefab, wallStartPos + Vector3.right * size,
			Quaternion.Euler(_90dRigth),
			instantiatedPrefabsCatalog);
		
		Instantiate(cornerPrefab, wallStartPos + Vector3.forward * size,
			Quaternion.Euler(_90dRigth * 3.0f),
			instantiatedPrefabsCatalog);
		
		Instantiate(cornerPrefab, wallStartPos + (Vector3.right + Vector3.forward) * size,
			Quaternion.identity,
			instantiatedPrefabsCatalog);
		
		/* Walls */
		for(int i = 0; i < wallsNum; i ++) {
			
			Instantiate(wallPrefab, Vector3.Scale(wallStartPos + Vector3.forward * (1.0f + i), scalerNPP), Quaternion.identity, instantiatedPrefabsCatalog);
			Instantiate(wallPrefab, wallStartPos + Vector3.forward * (1.0f + i), Quaternion.Euler(_90dRigth * 2.0f), instantiatedPrefabsCatalog);
			
			Instantiate(wallPrefab, wallStartPos + Vector3.right * (1.0f + i), Quaternion.Euler(_90dRigth), instantiatedPrefabsCatalog);
			Instantiate(wallPrefab, Vector3.Scale(wallStartPos + Vector3.right * (1.0f + i), scalerPPN), Quaternion.Euler(_90dRigth * 3.0f), instantiatedPrefabsCatalog);
		}
		
		return true;
		
		#else
		return false;
		#endif // CLIENT
	}
	
	public void SpawnUnits() {
		
		#if CLIENT
		if(null == state) {
			
			InfoManager.Log(noWorldStateEntry);
			return;
		}
		
		if(null == unitPrefab) {
			
			InfoManager.Log(missingUnitPrefabEntry);
			return;
		}
		
		Vector3 spawnPos;
		
		GameObject instantiated;
		Unit unit;
		
		foreach(int unitPosition in state.positions) {
			
			spawnPos = GetPositionFromIndex(unitPosition);
			
			instantiated = (GameObject)Instantiate(unitPrefab, spawnPos, Quaternion.identity, instantiatedPrefabsCatalog);
			unit = instantiated.GetComponent<Unit>();
			
			unit.position = unitPosition;
		}
		#endif // CLIENT
	}
	
	public void SpawnGridFloor() {
		
		#if CLIENT
		if(null == state) {
			
			InfoManager.Log(noWorldStateEntry);
			return;
		}
		
		if(null == gridPrefab) {
			
			InfoManager.Log(missingGridPrefabEntry);
			return;
		}
		
		int size = state.size;
		int area = size * size;
		
		float halfSize = (float)size / 2.0f;
		Vector3 startPos = new Vector3(- halfSize + .5f, .0f, - halfSize + .5f);
		Vector3 spawnPos;
		
		GameObject instantiated;
		Grid gridBehaviour;
		
		for(int i = 0; i < area; i ++) {
			
			spawnPos = startPos + Vector3.right * (i % size) + Vector3.forward * (i / size);
			
			instantiated = (GameObject)Instantiate(gridPrefab, spawnPos, Quaternion.identity, instantiatedPrefabsCatalog);
			gridBehaviour = instantiated.GetComponent<Grid>();
			
			gridBehaviour.index = i;
		}
		#endif // CLIENT
	}
	
	public void FormRequestAndSend() {
		
		#if CLIENT
		locked = true;
		
		if(null == clientScript) {
			
			InfoManager.Log(noNetClientSetEntry);
			return;
		}
		
		Request request = new Request();
		clientScript.DoSend((object)request, true);
		
		StartCoroutine(ExpectTimeout());
		#endif // CLIENT
	}
	
	IEnumerator ExpectTimeout() {
		
		#if CLIENT
		yield return waitForTimeout;
		
		SceneManager.LoadSceneAsync(timeoutSceneName, LoadSceneMode.Single);
		
		#else
		yield break;
		#endif // CLIENT
	}
}
