/** Summary **
 * 
 * NetBase.cs - is a collection of methods, that use Unity Net Low Level API (Unet LLAPI)
 * functionality as either server or client defined by a build definition.
 * 
 * Handler methods can (and should) be overriden in order to process
 * connection information. Those handlers are:
 *  - protected void OnConnected(int _socket, int _connectionID)
 *  - protected void OnDisconnected(int _socket, int _connectionID)
 *  - protected void OnMessageReceived(int _socket, int _connectionID, int _channelID, ref byte[] _buffer, int _bufferSize)
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

#region // include
/* MonoBehaviour */
using UnityEngine;
/* NetworkTransport */
using UnityEngine.Networking;
/* SceneManager */
using UnityEngine.SceneManagement;
/* MemoryStream */
using System.IO;
/* BinaryFormatter */
using System.Runtime.Serialization.Formatters.Binary;
#endregion // include

public class NetBase : MonoBehaviour {
	
	#if (SERVER && CLIENT) || (!SERVER && !CLIENT) // These lines prevent invalid build from being built
	}} APPLICATION CANNOT BE BOTH SERVER AND CLIENT AT THE SAME TIME {{
	}} NOR BE NEITHER OF THOSE {{
	#endif
	
	#region // Variables
	[Header("Initial Preferences")]
	public string ip = "127.0.0.1";
	public int port = 1337;
	
	public int avarageBufferSize = 32;
	public int maxConnections = 10;
	
	public int maximumSendRetries = 8;
	
	[Header("Initial Classes Values")]
	public GlobalConfig globalConfig;
	public ConnectionConfig connectionConfig;
	
	/* Values Determined At Initialization */
	#if CLIENT
	public int connection { protected set; get; }
	#endif
	
	protected static bool initialized = false;
	
	protected static int socket;
	[System.NonSerialized]
	public HostTopology topology;
	
	public static byte channelUnreliable;
	public static byte channelReliable;
	
	/* Values Determined at Runtime */
	protected BinaryFormatter formatter = new BinaryFormatter();
	
	protected byte error;
	
	protected byte[] buffer;
	protected int bufferSize;
	
	/* Event */
	protected static NetworkEventType networkEvent;
	
	protected static int senderSocket;
	protected static int connectionID;
	
	protected static int channelID;
	protected static int receivedDataSize;
	
	/* String Helpers */
	protected const string sbName = "Client ";
	protected const string sbHasConnected = " has Connected";
	protected const string sbHasDisconnected = " has Disconnected";
	protected const string sbSentAMessage = " has Sent a Message";
	protected const string reallocatingBufferToReceiveEntry = "Reallocating buffer in order to receive message to a size of ";
	protected const string reallocatingBufferToSendEntry = "Reallocating buffer in order to send message to a size of ";
	
	#if SERVER
	protected const string identifier = "[Server] ";
	protected const string impossibleToStartThisEntry = "Server cannot be Started in this Build";
	protected const string failedToSendDataEntry = "Server Failed to Send data to Client";
	#elif CLIENT
	protected const string identifier = "[Client] ";
	protected const string impossibleToStartThisEntry = "Client cannot be Initialized in this Build";
	protected const string failedToSendDataEntry = "Client Failed to Send data to Server";
	#endif // SERVER
	
	#endregion // Variables
	
	#region // Default MonoBehaviour methods
	void Awake() {
		
		bufferSize = avarageBufferSize;
		buffer = new byte[bufferSize];
	}
	
	void Start() {
		
		if(initialized)
			return;
		
		if(null != CustomNetworkData.instance) {
			
			ip = CustomNetworkData.instance.ip;
			
			#if SERVER
			port = CustomNetworkData.instance.port;
			#endif // SERVER
		}
		
		channelReliable = connectionConfig.AddChannel(QosType.ReliableSequenced);
		channelUnreliable = connectionConfig.AddChannel(QosType.UnreliableSequenced);
		
		topology = new HostTopology(connectionConfig, maxConnections);
		NetworkTransport.Init(globalConfig);
		
		#if SERVER
		socket = NetworkTransport.AddHost(topology, port);
		
		#elif CLIENT
		socket = NetworkTransport.AddHost(topology);
		#endif // SERVER
		
		if(0 > socket) {
			
			CustomNetworkData.instance.invalidData = true;
			SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
		}
		else { CustomNetworkData.instance.invalidData = false; }
		
		#if CLIENT
		connection = NetworkTransport.Connect(socket, ip, port, 0, out error);
		#endif // Client
		
		if(socket < 0) {
			
			return;
		}
		
		initialized = true;
	}
	
	/* Incoming Messages lookup */
	void Update() {
		
		if(!initialized)
			return;
		
		if(buffer.Length != avarageBufferSize) {
			
			bufferSize = avarageBufferSize;
			buffer = new byte[bufferSize];
		}
		
		do {
			
			networkEvent = NetworkTransport.Receive(out senderSocket, out connectionID, out channelID, buffer, bufferSize, out receivedDataSize, out error);
			
			/* if buffer is not big enough */
			if(NetworkError.MessageToLong == (NetworkError)error) {
				
				InfoManager.Log(identifier + reallocatingBufferToReceiveEntry + receivedDataSize.ToString());
				
				bufferSize = receivedDataSize;
				buffer = new byte[bufferSize];
				
				continue;
			}
			
			switch(networkEvent) {
				
				case NetworkEventType.Nothing:
					break;
				
				case NetworkEventType.ConnectEvent:
					OnConnected(senderSocket, connectionID);
					break;
				
				case NetworkEventType.DataEvent:
					OnMessageReceived(senderSocket, connectionID, channelID, ref buffer, bufferSize);
					break;
				
				case NetworkEventType.DisconnectEvent:
					OnDisconnected(senderSocket, connectionID);
					break;
				
				default: break;
			}
		} while (networkEvent != NetworkEventType.Nothing);
	}
	
	void OnDestroy() {
		
		if(!initialized)
			return;
		
		#if SERVER
		NetworkTransport.RemoveHost(socket);
		#elif CLIENT
		NetworkTransport.Disconnect(socket, connection, out error);
		#endif // SERVER
		
		NetworkTransport.Shutdown();
		initialized = false;
	}
	#endregion // Default MonoBehaviour methods
	
	#region // Messaging
	protected void Send(object messageToSend, int connection, bool reliable = false) {
		
		int counter = maximumSendRetries;
		
		if(bufferSize != avarageBufferSize) {
			
			bufferSize = avarageBufferSize;
			buffer = new byte[bufferSize];
		}
		
		
		while(true) {
			
			try {
				
				-- counter;
				
				MemoryStream stream = new MemoryStream(buffer);
				formatter.Serialize(stream, messageToSend);
				
				NetworkTransport.Send(socket, connection,
						(reliable?channelReliable:channelUnreliable),
						buffer, bufferSize, out error);
			}
			/* if buffer is not big enough */
			catch(System.NotSupportedException) {
				
				if(0 > counter) {
					
					InfoManager.Log(failedToSendDataEntry);
					return;
				}
				
				bufferSize *= 2;
				buffer = new byte[bufferSize];
				
				InfoManager.Log(identifier + reallocatingBufferToSendEntry + bufferSize);
				
				continue;
			}
			
			break;
		}
	}
	
	/* Returns object received by connection */
	public object ProcessMessage(byte[] messageToProcess) {
		
		Stream stream = new MemoryStream(messageToProcess);
		return formatter.Deserialize(stream);
	}
	#endregion // Messaging
	
	#region // Handlers
	protected virtual void OnConnected(int _socket, int _connectionID) {  }
	protected virtual void OnDisconnected(int _socket, int _connectionID) {  }
	protected virtual void OnMessageReceived(int _socket, int _connectionID, int _channelID, ref byte[] _buffer, int _bufferSize) {  }
	#endregion // Handlers
}
