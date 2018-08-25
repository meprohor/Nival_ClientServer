#region // include
/* MonoBehaviour */
using UnityEngine;
/* MemoryStream */
using System.IO;
/* BinaryFormatter */
using System.Runtime.Serialization.Formatters.Binary;
#endregion // include

public class InfoManager : MonoBehaviour {
	
	#region // Singleton
	public static InfoManager instance { private set; get; }
	void Awake() { if(null == instance) instance = this; }
	#endregion // Singleton
	
	#region // Variables
	/* Maximum number of strings in a log */
	public int maxLogEntries = 16;
	
	/* Actual string to show to ui */
	private static string uiLog = string.Empty;
	
	private static int entries = 0;
	
	/* String Helpers */
	public const string linestart = "> ";
	public const string newline = "\n";
	
	/* Rect Helpers */
	#if LOG
	private static Rect logRect = new Rect(10, 10, Screen.width - 20, Screen.height - 20);
	#endif // LOG
	
	/* Used for Deserealizing of a Message received in Bytes Array */
	private BinaryFormatter _formatter = new BinaryFormatter();
	private static BinaryFormatter formatter { get { return instance._formatter; } }
	#endregion // Variables
	
	private static void CreateInstance() {
		
		new GameObject("Info Manager", typeof(InfoManager));
	}
	
	public static void Log(string logMessage) {
		
		if(null == instance)
			CreateInstance();
		
		if(entries > instance.maxLogEntries) {
			
			ClearLog();
		}
		
		uiLog += linestart + logMessage + newline;
		++ entries;
	}
	
	public static void Log(byte[] logStream) {
		
		Stream stream = new MemoryStream(logStream);
		string deserialized = formatter.Deserialize(stream).ToString();
		
		Log(deserialized);
	}
	
	public static void ClearLog() {
		
		uiLog = string.Empty;
		entries = 0;
	}
	
	void OnGUI() {
		
		#if LOG
		GUI.Label(logRect, uiLog);
		#endif // LOG
	}
}
