/** Summary **
 * 
 * CustomNetworkData.cs - is a collection of data that overloads standard NetBase values if present.
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

#region // include
/* MonoBehaviour */
using UnityEngine;
#endregion // include

public class CustomNetworkData : MonoBehaviour {
	
	public static CustomNetworkData instance { private set; get; }
	
	public string ip = "127.0.0.1";
	public bool invalidData = false;
	
	#if SERVER
	public int port = 1337;
	#endif // SERVER
	
	void Awake() {
		
		if(null != instance)
			return;
		
		instance = this;
		DontDestroyOnLoad(gameObject);
	}
}
