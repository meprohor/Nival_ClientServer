/** Summary **
 * 
 * StartupManager.cs - provides GUI and scene management for startup scene
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

#region // include
/* MonoBehaviour */
using UnityEngine;
/* SceneManager */
using UnityEngine.SceneManagement;
#endregion // include

public class StartupManager : MonoBehaviour {
	
	#region // Variables
	private Rect guiFieldRect = new Rect(20, 20, Screen.width / 2.0f, Screen.height - 40);
	private int newPort;
	
	public string serverSceneName = "Server";
	public string clientSceneName = "Client";
	
	private static string startStr = "START";
	private static string ipEntry = "IP: ";
	private static string portEntry = "Port: ";
	
	#if SERVER
	private static string failedEntry = "Couldn't Start a Server. Probably specified Port is occupied";
	
	#else
	private static string failedEntry = "Couldn't Connect to a Server";
	#endif // SERVER
	
	#endregion // Variables
	
	void Start() {
		
		if(null == CustomNetworkData.instance) {
			
			new GameObject("Custom Network Data", typeof(CustomNetworkData));
		}
	}
	
	void OnGUI() {
		
		if(null == CustomNetworkData.instance)
			return;
		
		GUILayout.BeginArea(guiFieldRect);
		
		GUILayout.BeginHorizontal();
		
		GUILayout.Label(ipEntry, GUILayout.MaxWidth(80));
		CustomNetworkData.instance.ip = GUILayout.TextField(CustomNetworkData.instance.ip, 16);
		
		GUILayout.EndHorizontal();
		
		#if SERVER
		GUILayout.BeginHorizontal();
		
		GUILayout.Label(portEntry, GUILayout.MaxWidth(80));
		
		try { newPort = int.Parse(GUILayout.TextField(CustomNetworkData.instance.port.ToString())); }
		catch(System.Exception) { newPort = CustomNetworkData.instance.port; }
		
		CustomNetworkData.instance.port = newPort;
		
		GUILayout.EndHorizontal();
		#endif // SERVER
		
		if(GUILayout.Button(startStr)) {
			
			#if SERVER
			SceneManager.LoadSceneAsync(serverSceneName, LoadSceneMode.Single);
			
			#elif CLIENT
			SceneManager.LoadSceneAsync(clientSceneName, LoadSceneMode.Single);
			
			#endif
		}
		
		if(CustomNetworkData.instance.invalidData) {
			
			GUILayout.Label(failedEntry);
		}
		
		GUILayout.EndArea();
	}
}
