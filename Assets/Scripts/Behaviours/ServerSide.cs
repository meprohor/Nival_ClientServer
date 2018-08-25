/** Summary **
 * 
 * ServerSide.cs - collection of methods for processing client requests and forming a WorldState for Client once one connects
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

#region // include
/* MonoBehaviour */
using UnityEngine;
/* IEnumerator */
using System.Collections;
/* Dictionary */
using System.Collections.Generic;
#endregion // include

[System.Serializable]
public class ServerSide : MonoBehaviour {
	
	#region // Singleton
	public static ServerSide instance { private set; get; }
	void Awake() { if(null == instance) instance = this; }
	#endregion // Singleton
	
	#region // Variables
	[Header("Task Related Values")]
	public int minimumSize = 7;
	public int maximumSize = 12;
	public int minimumUnitCount = 1;
	public int maximumUnitCount = 5;
	
	[Header("References")]
	public NetServer netServer;
	
	[System.NonSerialized]
	public Dictionary<int, WorldState> stateByConnection = new Dictionary<int, WorldState>();
	
	#if SERVER
	private Dictionary<int, Respond> calculationsInProgress = new Dictionary<int, Respond>();
	#endif // SERVER
	
	private const string impossibleToUseEntry = "Impossible to use ServerSide methods in this Build";
	#endregion // Variables
	
	void Start() {
		
		if(null == netServer) {
			
			netServer = GetComponent<NetServer>();
		}
	}
	
	public void OnDisconnected(int connection) {
		
		#if SERVER
		if(calculationsInProgress.ContainsKey(connection)) {
			
			calculationsInProgress[connection].killswitch = true;
		}
		
		stateByConnection.Remove(connection);
		#endif // SERVER
	}
	
	public static WorldState GenerateNewWorldState() {
		
		#if SERVER
		int size;
		int count;
		int[] positions;
		
		size = Random.Range(instance.minimumSize, instance.maximumSize + 1);
		count = Random.Range(instance.minimumUnitCount, instance.maximumUnitCount + 1);
		
		positions = new int[count];
		
		/* area - площадь */
		int area = size * size;
		int i, j;
		
		for(i = 0; i < count; i ++) {
			
			positions[i] = Random.Range(0, area);
			
			for(j = 0; j < i; j ++) {
				
				if(positions[i] == positions[j]) {
					
					--i;
					break;
				}
			}
		}
		
		WorldState result = new WorldState(size, count, ref positions);
		return result;
		
		#else
		InfoManager.Log(impossibleToUseEntry);
		return null;
		#endif // SERVER
	}
	
	public static void ProcessRequest(object receivedRequest, int connection) {
		
		#if SERVER
		if(receivedRequest is Request) {
			
			Request request = (Request)receivedRequest;
			instance.StartCoroutine(instance.MakeARespond(request, connection));
			
			return;
		}
		#endif // SERVER
	}
	
	private IEnumerator MakeARespond(Request request, int connection) {
		
		#if SERVER
		
		if(calculationsInProgress.ContainsKey(connection)) { yield break; }
		
		WorldState associatedState = stateByConnection[connection];
		
		#region // Clear worldState of dynamic units
		foreach(int selectedUnitPosition in request.unitsPositions) {
			
			for(int i = 0; i < associatedState.positions.Length; i ++) {
				
				if(associatedState.positions[i] == selectedUnitPosition)
					associatedState.positions[i] = -1;
			}
		}
		#endregion // Clear worldState of dynamic units
		
		Respond respond = new Respond(request, associatedState);
		calculationsInProgress.Add(connection, respond);
		
		while(false == respond.isReady) { yield return null; }
		
		calculationsInProgress.Remove(connection);
		if(!respond.success) { yield break; }
		
		instance.netServer.SendRespond(respond, connection, true);
		
		#region // Update worldState
		int j = 0;
		// foreach(List<int> command in respond.commands) {
		foreach(int[] command in respond.commands) {
			
			for(int i = j; i < associatedState.positions.Length; i ++) {
				
				if(-1 == associatedState.positions[i]) {
					
					j = i + 1;
					// associatedState.positions[i] = command[command.Count - 1];
					associatedState.positions[i] = command[command.Length - 1];
					break;
				}
			}
		}
		#endregion // Update worldState
		
		#else
		yield break;
		#endif // SERVER
	}
}
