using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour {

	public JengaStateMachine stateMachine;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider c) {
		if (c == null)
			return;
		if (c.GetComponent<Rigidbody>() == null)
			return;
		if (stateMachine == null)
			return; 

		stateMachine.blockTouchesGround(c.GetComponent<Rigidbody>());
	}
}
