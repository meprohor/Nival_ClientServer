/** Summary **
 * 
 * Request.cs - is a class that contains request information for server,
 * that is a collection of units positions to be moved and destination.
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

[System.Serializable]
public class Request {
	
	public int[] unitsPositions;
	public int destination;
	
	public Request() {
		
		#if CLIENT
		if(null == ClientSide.instance.selectedUnits || 0 == ClientSide.instance.selectedUnits.Count) {
			
			return;
		}
		
		int selectedNumber = ClientSide.instance.selectedUnits.Count;
		
		unitsPositions = new int[selectedNumber];
		
		for(int i = 0; i < selectedNumber; i ++) {
			
			unitsPositions[i] = ClientSide.instance.selectedUnits[i].position;
		}
		
		destination = ClientSide.instance.selectedDestination.index;
		#endif // CLIENT
	}
}
