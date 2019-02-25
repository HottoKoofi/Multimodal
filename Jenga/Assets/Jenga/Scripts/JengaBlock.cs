using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
[System.Serializable]
public class JengaBlock : MonoBehaviour {

	public int floor;

	public int color;

	public int id;
	public Vector3 p {
		get {
			return transform.localPosition;
		}
		set {
			transform.localPosition = value;
		}
	}
	public Quaternion r {
		get {
			return transform.localRotation;
		}
		set {
			transform.localRotation = value;
		}
	}
	private bool _e;
	public bool e {
		get {
			return _e;
		}
		set {
			_e = value;
			gameObject.SetActive(value);
		}
	}

	public void Start() {
		e = true;
	}

	// void Update() {
	// 	if (color == 0)
	// 		gameObject.name = "Token Red";
	// 	if (color == 1)
	// 		gameObject.name = "Token Green";
	// 	if (color == 2)
	// 		gameObject.name = "Token Blue";
	// 	if (color == 3)
	// 		gameObject.name = "Token White";
	// }
}
