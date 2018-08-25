/** Summary **
 * 
 * Unit.cs - is a class that represents unit and it's behaviour in a world. Class provides interface for accessing
 * instantiated units instances as a List and by position value.
 * 
 * Created by meprohor (meprohor@gmail.com) for Nival as part of the "Client - Server" Pre-Employment Task
 * This script is licensed under wtfpl v.2
 */

#region // include
/* MonoBehaviour */
using UnityEngine;
/* IEnumerator */
using System.Collections;
/* List */
using System.Collections.Generic;
#endregion // include

public class Unit : MonoBehaviour {
	
	#region // Variables
	private enum UnitState { Standing, Walking }
	
	#region // private UnitState unitState
	private UnitState _unitState = UnitState.Standing;
	private UnitState unitState {
		
		get {
			
			return _unitState;
		}
		set {
			
			if(value == _unitState)
				return;
			
			_unitState = value;
			
			animator.CrossFade((unitState == UnitState.Standing)?(standAnimHash):(walkAnimHash), animTransitionDuration);
		}
	}
	#endregion // private UnitState unitState
	
	#region // animations related
	
	#region // private Animator animator
	private Animator _animator = null;
	private Animator animator {
		
		get {
			
			if(null == _animator) { _animator = GetComponent<Animator>(); }
			return _animator;
		}
	}
	#endregion // private UnitState unitState
	
	#region // private int standAnimHash
	private static int _standAnimHash = -1;
	private int standAnimHash {
		
		get {
			
			if(0 > _standAnimHash) { _standAnimHash = Animator.StringToHash(standAnimClipName); }
			return _standAnimHash;
		}
	}
	#endregion // private int standAnimHash
	
	#region // private int walkAnimHash
	private static int _walkAnimHash = -1;
	private int walkAnimHash {
		
		get {
			
			if(0 > _walkAnimHash) { _walkAnimHash = Animator.StringToHash(walkAnimClipName); }
			return _walkAnimHash;
		}
	}
	#endregion // private int walkAnimHash
	
	#endregion // animations related
	
	[Header("References")]
	public Transform[] selectionIndicators;
	
	[Header("Preferences")]
	public float selectionRotationPerSecond = 90.0f;
	public float maxSpeedMultiplier = 2.0f;
	public AnimationCurve moveCurve;
	public float walkTime = .25f;
	
	[Space]
	[Range(.0f, 1.0f)]
	public float animTransitionDuration = .25f;
	public string standAnimClipName = "Idle";
	public string walkAnimClipName = "Walk";
	
	/* Set by ClientSide */
	[HideInInspector]
	public int position = -1;
	
	private Vector3 _selectionRotationPerSecond;
	private static float[] speedModifiers;
	private static int indicatorsCount = -1;
	
	private Renderer[] indicatorRenderers;
	
	#region // private List<Unit> selected
	private List<Unit> _selected;
	private List<Unit> selected {
		
		get {
			
			if(null == _selected)
				_selected = ClientSide.instance.selectedUnits;
			
			return _selected;
		}
	}
	#endregion // private List<Unit> selected
	
	private const string invalidCurveEntry = "Curve Must end with 1.0f value";
	
	[System.NonSerialized]
	public static List<Unit> instances;
	
	#endregion // Variables
	
	public static Unit GetInstance(int providedPosition) {
		
		foreach(Unit instance in instances) {
			
			if(providedPosition == instance.position)
				return instance;
		}
		return null;
	}
	
	void Awake() {
		
		if(null == instances) {
			
			instances = new List<Unit>();
		}
		
		instances.Add(this);
		
		if(!Mathf.Approximately(1.0f, moveCurve.Evaluate(1.0f))) {
			
			InfoManager.Log(invalidCurveEntry);
		}
		
		_selectionRotationPerSecond = new Vector3(.0f, .0f, selectionRotationPerSecond);
		
		if(0 > indicatorsCount) {
			
			indicatorsCount = selectionIndicators.Length;
			
			speedModifiers = new float[indicatorsCount];
			
			for(int i = 0; i < indicatorsCount; i ++) {
				
				speedModifiers[i] = Random.Range(1.0f, maxSpeedMultiplier);
			}
		}
	}
	
	void Start() {
		
		indicatorRenderers = new Renderer[indicatorsCount];
		
		for(int i = 0; i < indicatorsCount; i ++) {
			
			indicatorRenderers[i] = selectionIndicators[i].GetComponent<Renderer>();
		}
		
		OnDeselected();
	}
	
	void OnMouseUpAsButton() {
		
		if(ClientSide.instance.locked) { return; }
		
		if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl)) {
			
			if(selected.Contains(this)) {
				
				selected.Remove(this);
				OnDeselected();
			}
			else {
				
				selected.Add(this);
				OnSelected();
			}
		}
		else {
			
			foreach(Unit listedUnit in selected) {
				
				listedUnit.OnDeselected();
			}
			
			selected.Clear();
			selected.Add(this);
			
			OnSelected();
		}
	}
	
	void Update() {
		
		for(int i = 0; i < indicatorsCount; i ++) {
			
			selectionIndicators[i].Rotate(_selectionRotationPerSecond * Time.deltaTime * speedModifiers[i]);
		}
	}
	
	public void OnSelected() {
		
		foreach(Renderer renderer in indicatorRenderers) {
			
			renderer.enabled = true;
		}
	}
	
	public void OnDeselected() {
		
		foreach(Renderer renderer in indicatorRenderers) {
			
			renderer.enabled = false;
		}
	}
	
	public void Receive(int[] command) {
		
		StartCoroutine(DoReceive(command));
	}
	
	private IEnumerator DoReceive(int[] command) {
		
		int count = command.Length;
		float lerpT;
		Vector3 positionA;
		Vector3 positionB;
		
		if(count <= 1)
			yield break;
		
		unitState = UnitState.Walking;
		
		for(int i = 1; i < count; i ++) {
			
			lerpT = .0f;
			
			positionA = transform.position;
			positionB = ClientSide.instance.GetPositionFromIndex(command[i]);
			
			while(1.0f > lerpT) {
				
				yield return null;
				
				lerpT += Time.deltaTime * (1.0f / walkTime);
				lerpT = Mathf.Clamp01(lerpT);
				
				transform.position = Vector3.Lerp(positionA, positionB, moveCurve.Evaluate(lerpT));
			}
		}
		
		unitState = UnitState.Standing;
		
		position = command[count - 1];
		
		foreach(Unit unit in instances) {
			
			if(UnitState.Walking == unit.unitState)
				yield break;
		}
		
		ClientSide.instance.selectedDestination.Deselect();
		ClientSide.instance.locked = false;
	}
}
