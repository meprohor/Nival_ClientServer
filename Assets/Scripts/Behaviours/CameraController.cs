/** Summary **
 * 
 * CameraController.cs - is a class that allows for camera rotation around global center by dragging mouseScrollDelta
 * with mouse wheel pressed and zooming in by scrolling mouse wheel.
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

#region // include
/* MonoBehaviour */
using UnityEngine;
#endregion // include

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
	
	#region // Variables
	public float mouseSensitivity = 1.0f;
	public float zoomSensitivity = 1.0f;
	
	public Vector2 zoomRestrictions = new Vector2(2.5f, 10.0f);
	#endregion // Variables
	
	void LateUpdate() {
		
		transform.position += transform.position.normalized * Input.mouseScrollDelta.y * - zoomSensitivity;
		transform.position = transform.position.normalized * V2Clamp(transform.position.magnitude, zoomRestrictions);
		
		if(Input.GetKey(KeyCode.Mouse2)) {
			
			transform.RotateAround(Vector3.zero, Vector3.up, Input.GetAxis("Mouse X") * mouseSensitivity);
		}
	}
	
	private float V2Clamp(float val, Vector2 restr) {
		
		return Mathf.Clamp(val, restr.x, restr.y);
	}
}
