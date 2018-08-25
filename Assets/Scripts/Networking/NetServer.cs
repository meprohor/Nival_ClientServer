/** Summary **
 * 
 * NetServer.cs - is a NetBase class deriative that sends WorldState to client once one
 * connects to a server and channels client requests to a ServerSide.cs
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

public class NetServer : NetBase {
	
	#if SERVER
	private object received;
	
	private const string byeStr = "bye";
	
	protected override void OnConnected(int _socket, int _connectionID) {
		
		WorldState newWorldState = ServerSide.GenerateNewWorldState();
		
		ServerSide.instance.stateByConnection.Add(_connectionID, newWorldState);
		Send((object)newWorldState, connectionID, true);
	}
	
	protected override void OnDisconnected(int _socket, int _connectionID) {
		
		ServerSide.instance.OnDisconnected(_connectionID);
		InfoManager.Log(identifier + sbName + _connectionID + sbHasDisconnected);
	}
	
	protected override void OnMessageReceived(int _socket, int _connectionID, int _channelID, ref byte[] _buffer, int _bufferSize) {
		
		InfoManager.Log(identifier + sbName + _connectionID + sbSentAMessage);
		
		received = ProcessMessage(_buffer);
		ServerSide.ProcessRequest(received, _connectionID);
	}
	
	public void SendRespond(Respond respond, int address, bool reliable = false) {
		
		Send((object)respond, address, reliable);
	}
	#endif // SERVER
}
