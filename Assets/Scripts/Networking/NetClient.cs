/** Summary **
 * 
 * NetClient.cs - is a NetBase class deriative that receives WorldState from server once one
 * connects to it and channels server replies to a ClientSide.cs
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

public class NetClient : NetBase {
	
	#if CLIENT
	private object received;
	
	protected override void OnMessageReceived(int _socket, int _connectionID, int _channelID, ref byte[] _buffer, int _bufferSize) {
		
		InfoManager.Log(identifier + sbName + _connectionID + sbSentAMessage);
		
		received = ProcessMessage(_buffer);
		ClientSide.instance.ProcessRespond(received);
	}
	
	public void DoSend(object info, bool reliable = false) {
		
		Send(info, connection, reliable);
	}
	#endif // CLIENT
}
