/** Summary **
 * 
 * Grid.cs - is a simple OnClick handler that informs ClientSide if a request for server should be formed
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

 #region // include
 /* MonoBehaviour */
using UnityEngine;
 #endregion // include

public class Grid : MonoBehaviour {
	
	public int index = -1;
	
	public Renderer lightsRenderer;
	public Material defaultLight;
	public Material selectedLight;
	
	/* On Selected with RMB */
	void OnMouseOver() {
		
		if(ClientSide.instance.locked) { return; }
		
		if(!Input.GetKeyUp(KeyCode.Mouse1))
			return;
		
		#region // Error handlers
		if(null == ClientSide.instance.selectedUnits || 0 >= ClientSide.instance.selectedUnits.Count) {
			
			return;
		}
		
		if(0 > index) {
			
			return;
		}
		#endregion // Error handlers
		
		if(null != lightsRenderer) {
			
			lightsRenderer.material = selectedLight;
		}
		
		ClientSide.instance.selectedDestination = this;
		ClientSide.instance.FormRequestAndSend();
	}
	
	public void Deselect() {
		
		lightsRenderer.material = defaultLight;
	}
}
