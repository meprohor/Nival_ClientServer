/** Summary **
 * 
 * WorldState.cs - is a class that represents world state that is shown to a client once one connects to server.
 * Size is a number of cells at one side of a field, count is a number of units and positions is an array that stores
 * position for every unit.
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

[System.Serializable]
public class WorldState {

	public int size;
	public int count;
	public int[] positions;
	
	public WorldState(int n_size, int n_count, ref int[] n_positions) {
		
		size = n_size;
		count = n_count;
		positions = (int[])n_positions.Clone();
	}
}
