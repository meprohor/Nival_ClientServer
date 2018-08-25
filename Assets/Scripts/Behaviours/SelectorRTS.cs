/** Summary **
 * 
 * SelectorRTS.cs - this class handles LMB press and drag, forms a Rect that checks for units in it and draws
 * a rect to GUI.
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

#region // include
/* MonoBehaviour */
using UnityEngine;
#endregion // include

public class SelectorRTS : MonoBehaviour {
	
	#region // Variables
	[Range(.0f, 512.0f)]
	public float minimalArea = .05f;
	
	[Range(.0f, 1.0f)]
	public float transparency = .5f;
	
	#region // private Color color
	#if !UNITY_EDITOR
	private Color _color = Color.black;
	private Color color {
		
		get {
			
			if(Color.black == _color) {
				
				_color = Color.white;
				_color.a = 1.0f - transparency;
			}
			return _color;
		}
	}
	
	#else
	private Color _color;
	private Color color {
		
		get {
			
			_color = Color.white;
			_color.a = 1.0f - transparency;
			
			return _color;
		}
	}
	#endif // !UNITY_EDITOR
	#endregion // private Color color
	
	[System.NonSerialized]
	public Vector2 startSelectionPos,
		currentSelectionPos;
	private Rect selectedViewPortRect = new Rect(.0f, .0f, .0f, .0f);
	private Rect displayedViewPortRect = new Rect(.0f, .0f, .0f, .0f);
	
	#region // private Camera cam
	private Camera _cam = null;
	private Camera cam {
		
		get {
			
			if(null == _cam)
				_cam = Camera.main;
			return _cam;
		}
	}
	#endregion // private Camera cam
	
	#endregion // Variables
	
	void LateUpdate() {
		
		if(ClientSide.instance.locked) { return; }
		
		if(Input.GetKey(KeyCode.Mouse0)) {
			
			if(Input.GetKeyDown(KeyCode.Mouse0)) {
				
				selectedViewPortRect.width = selectedViewPortRect.height = .0f;
				startSelectionPos = Input.mousePosition;
			}
			else {
				
				currentSelectionPos = Input.mousePosition;
				
				selectedViewPortRect.x = Mathf.Min(startSelectionPos.x, currentSelectionPos.x);
				selectedViewPortRect.width = Mathf.Abs(startSelectionPos.x - currentSelectionPos.x);
				
				selectedViewPortRect.y = Mathf.Min(startSelectionPos.y, currentSelectionPos.y);
				selectedViewPortRect.height = Mathf.Abs(startSelectionPos.y - currentSelectionPos.y);
			}
			
			if(selectedViewPortRect.width * selectedViewPortRect.height < minimalArea) { return; }
			
			foreach(Unit unit in Unit.instances) {
				
				if(selectedViewPortRect.Contains(cam.WorldToScreenPoint(unit.transform.position))) {
					
					if(!ClientSide.instance.selectedUnits.Contains(unit)) {
						
						ClientSide.instance.selectedUnits.Add(unit);
						unit.OnSelected();
					}
				}
				else {
					
					if(ClientSide.instance.selectedUnits.Contains(unit)) {
						
						ClientSide.instance.selectedUnits.Remove(unit);
						unit.OnDeselected();
					}
				}
			}
		}
	}
	
	void OnGUI() {
		
		if(ClientSide.instance.locked
			|| !Input.GetKey(KeyCode.Mouse0)
			|| (selectedViewPortRect.width * selectedViewPortRect.height < minimalArea)) { return; }
		
		displayedViewPortRect.x = selectedViewPortRect.x;
		displayedViewPortRect.width = selectedViewPortRect.width;
		displayedViewPortRect.y = Screen.height - selectedViewPortRect.y - displayedViewPortRect.height;
		displayedViewPortRect.height = selectedViewPortRect.height;
		
		GUI.color = color;
		GUI.Box(displayedViewPortRect, string.Empty);
	}
}
