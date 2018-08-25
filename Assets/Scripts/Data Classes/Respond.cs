/** Summary **
 * 
 * Respond.cs - is a class that is send from server to client. It contains info on units movement in order to reach
 * a position in a grid that is closest to the destination.
 * Request and WorldState data structures are essential for calculations, since it provides information of world size,
 * non-moving units positions and a list of units that must be moved.
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

 #region // include
#if SERVER
 /* Thread */
 using System.Threading;
 /* Array */
 using System;
 /* Stack */
 using System.Collections;
 /* List */
 using System.Collections.Generic;
#endif // SERVER
 #endregion // include
 
[System.Serializable]
public class Respond {
	
	#region // Variables
	
	/* list of orders assigned by current unit position */
	public int[][] commands;
	public bool success;
	
	[System.NonSerialized]
	public bool isReady = false;
	/* used to end a thread from outside */
	[System.NonSerialized]
	public bool killswitch = false;
	
	private Request request;
	private WorldState worldState;
	
	#if SERVER
	private static string identifier = "[Respond Thread] ";
	private static string threadEndedExternallyEntry = "Respond Computing Thread ended externally";
	private static string processingEntry = "processing Unit: ";
	#endif // SERVER
	
	#endregion // Variables
	
	#if SERVER
	public Respond(Request n_request, WorldState n_worldState) {
		
		request = n_request;
		worldState = n_worldState;
		
		Thread thread = new Thread(_findpath);
		thread.Start();
	}
	
	/* Threads */
	private void _findpath() {
		/* !!! no UnityEngine code must be used here !!! */
		
		success = false;
		isReady = false;
		
		#region // Local Variables
		int size = worldState.size;
		int selectedUnitsCount = request.unitsPositions.Length;
		int overallUnitsCount = worldState.positions.Length;
		int commandsCount;
		int i, j;
		
		/* per node data */
		int pos;
		int depth;
		int distance;
		
		int leftPos;
		int rightPos;
		int topPos;
		int downPos;
		TreeNode currentNode;
		TreeNode localDestinationNode;
		
		bool addedToTree;
		bool flagToContinue;
		
		/* used for distance calculation */
		int minDistance;
		
		int[] destinationCoordinates = new int[2];
		int[] nodeCoordinates = new int[2];
		int[] delta = new int[2];
		#endregion // Local Variables
		
		commands = new int[selectedUnitsCount][];
		
		/* movement graphs associated to each of moving units */
		Dictionary<int, List<TreeNode>> tree = new Dictionary<int, List<TreeNode>>();
		Queue<TreeNode> nodesToCheck = new Queue<TreeNode>();
		
		/* used for distance colculations */
		destinationCoordinates[0] = request.destination % size;
		destinationCoordinates[1] = request.destination / size;
		
		#region // sorting moving units by distance to destination
		/* This action saves in half of collision scenarios */
		
		for(i = 0; i < selectedUnitsCount; i ++) {
			
			for(j = 0; j < i; j ++) {
				
				delta[0] = destinationCoordinates[0] - request.unitsPositions[i] % size;
				delta[0] = (0 < delta[0])?(delta[0]):(- delta[0]);
				delta[1] = destinationCoordinates[1] - request.unitsPositions[i] / size;
				delta[1] = (0 < delta[1])?(delta[1]):(- delta[1]);
				
				distance = delta[0] + delta[1];
				
				delta[0] = destinationCoordinates[0] - request.unitsPositions[j] % size;
				delta[0] = (0 < delta[0])?(delta[0]):(- delta[0]);
				delta[1] = destinationCoordinates[1] - request.unitsPositions[j] / size;
				delta[1] = (0 < delta[1])?(delta[1]):(- delta[1]);
				
				if(distance < delta[0] + delta[1]) {
					
					int buf = request.unitsPositions[i];
					request.unitsPositions[i] = request.unitsPositions[j];
					request.unitsPositions[j] = buf;
				}
			}
		}
		#endregion // sorting moving units by distance to destination
		
		for(i = 0; i < selectedUnitsCount; i ++) {
			
			#region // movement graph building
			/* This is a BFS search of a destination node */
			/* One grapgh is created per moving unit since destination node depth would vary */
			
			InfoManager.Log(identifier + processingEntry + i);
			
			tree.Add(i, new List<TreeNode>());
			nodesToCheck.Clear();
			
			TreeNode startNode = new TreeNode(request.unitsPositions[i], 0, null);
			
			tree[i].Add(startNode);
			nodesToCheck.Enqueue(startNode);
			
			do {
				
				#region // prerequisites
				flagToContinue = false;
				addedToTree = false;
				currentNode = nodesToCheck.Dequeue();
				
				pos = currentNode.position;
				depth = currentNode.depth;
				#endregion // prerequisites
				
				if(-1 == pos) { continue; }
				
				if(killswitch) {
					
					InfoManager.Log(threadEndedExternallyEntry);
					isReady = true;
					return;
				}
				
				#region // check for collisions
				/* if non-moving unit occupies this position */
				for(j = 0; j < overallUnitsCount; j ++) {
					
					if(pos == worldState.positions[j])
						flagToContinue = true;
				}
				
				/* if moving units occupies this place at same depth */
				for(j = 0; j < i; j ++) {
					
					for(int k = 0; k < commands[j].Length; k ++) {
						
						if(pos == commands[j][k] && k == depth) { flagToContinue = true; }
					}
				}
				
				if(flagToContinue)
					continue;
				#endregion // check for collisions
				
				#region // add node to tree or replace existing node
				int treeDepth = tree[i].Count;
				
				for(j = 0; j < treeDepth; j ++) {
					
					if(tree[i][j].position == pos) {
						
						if(tree[i][j].depth >= depth) {
							
							/* replace other nodes back tracking links with current node */
							for(int k = 0; k < treeDepth; k ++) {
								
								if(tree[i][k].back == tree[i][j]) { tree[i][k].back = currentNode; }
							}
							
							tree[i][j] = currentNode;
							
							addedToTree = true;
							break;
						}
						else {
							
							/* tree already contains a better node */
							flagToContinue = true;
							break;
						}
					}
				}
				
				if(flagToContinue) { continue; }
				
				if(!addedToTree) {
					
					addedToTree = true;
					tree[i].Add(currentNode);
				}
				
				#endregion // add node to tree or replace existing node
				
				#if !FULLBFS
				/* if destination node was reached */
				if(addedToTree && request.destination == pos) { break; }
				#endif // !FULLBFS
				
				#region // stack adjacent positions
				if(addedToTree) {
					
					/* position of a node that leads back */
					int rejected = (null != currentNode.back)?(currentNode.back.position):(-1);
					
					topPos = (0 < pos / size)?(pos - size):(-1);
					if(rejected != topPos) nodesToCheck.Enqueue(new TreeNode(topPos, depth + 1, currentNode));
					
					rightPos = (size - 1 > pos % size)?(pos + 1):(-1);
					if(rejected != rightPos) nodesToCheck.Enqueue(new TreeNode(rightPos, depth + 1, currentNode));
					
					downPos = (size - 1 > pos / size)?(pos + size):(-1);
					if(rejected != downPos) nodesToCheck.Enqueue(new TreeNode(downPos, depth + 1, currentNode));
					
					leftPos = (0 < pos % size)?(pos - 1):(-1);
					if(rejected != leftPos) nodesToCheck.Enqueue(new TreeNode(leftPos, depth + 1, currentNode));
				}
				#endregion // stack adjacent positions
				
			}  while(0 != nodesToCheck.Count);
			#endregion // movement graph building
			
			#region // looking for local destination node and forming commands array
			
			localDestinationNode = tree[i][0];
			minDistance = -1;
			
			foreach(TreeNode node in tree[i]) {
				
				flagToContinue = false;
				
				/* if not occupied by other commands later values */
				for(j = 0; j < i; j ++) {
					
					commandsCount = commands[j].Length;
					
					if(0 < commandsCount && commands[j][commandsCount - 1] == node.position) {
						
						flagToContinue = true;
						break;
					}
				}
				
				if(flagToContinue) { continue; }
				
				#region // computing distance (no squares for performance)
				nodeCoordinates[0] = node.position % size;
				nodeCoordinates[1] = node.position / size;
				
				delta[0] = nodeCoordinates[0] - destinationCoordinates[0];
				delta[0] = (0 <= delta[0])?(delta[0]):(- delta[0]);
				
				delta[1] = nodeCoordinates[1] - destinationCoordinates[1];
				delta[1] = (0 <= delta[1])?(delta[1]):(- delta[1]);
				
				distance = delta[0] + delta[1];
				#endregion // computing distance (no squares for performance)
				
				if(distance < minDistance || 0 > minDistance) {
					
					minDistance = distance;
					localDestinationNode = node;
				}
			}
			
			/* Forming commands array */
			int outCount = 0;
			
			currentNode = localDestinationNode;
			while(null != currentNode) { outCount ++; currentNode = currentNode.back; }
			commands[i] = new int[outCount];
			
			j = outCount - 1;
			currentNode = localDestinationNode;
			while(null != currentNode) {
				
				commands[i][j --] = currentNode.position;
				currentNode = currentNode.back;
			}
			
			#endregion // looking for local destination node and forming commands array
		}
		
		success = true;
		isReady = true;
	}
	#endif // SERVER
}

#if SERVER
public class TreeNode {
	
	public int position;
	public int depth;
	
	public TreeNode back;
	
	public TreeNode(int n_position, int n_depth, TreeNode n_back) {
		
		position = n_position;
		depth = n_depth;
		
		back = n_back;
	}
}
#endif // SERVER
