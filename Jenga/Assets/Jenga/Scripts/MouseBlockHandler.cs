using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseBlockHandler : IBlockHandler {

	public JengaStateMachine stateMachine;
	public Camera cam;
	public float forceMultiplier = 1.0f;

	public Vector3 faceDirection;
	public Vector3 initialMousePosition;
	public Vector3 hitPosition;
	public Rigidbody hitBody;
	public SpringJoint pullSpring;
	public Rigidbody pullObject;

	public GameObject buttonReset;

	// Use this for initialization
	void Start () {
		if (cam == null)
			cam = Camera.main;
	}

	override public void startTurn() {
		Debug.Log("Start turn");
	}

	override public void endTurn() {
		Debug.Log("End turn");
	}
	
	// Update is called once per frame
	void Update () {
		pullForce();

		// Sample of push force without a spring joint
		// pushForce();
	}

	public void setForceMultiplier(float value) {
		forceMultiplier = value;
	}

	// Example of function that push the token when the user clic on them
	void pushForce(){
		if (Input.GetMouseButton(0)) {
			RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
				Rigidbody r = hit.rigidbody;
				if (r == null)
					return;
				hitPosition = hit.point;
				r.AddForceAtPosition(ray.direction * forceMultiplier, hitPosition);
			}
        }
	}

	// Function that tries to grab a block when the user clics
	void grabBlock() {
		// Raycast to get the block behind the mouse position
		RaycastHit hit;
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit)) {

			// If there is a hit check that has a rigidbody
			Rigidbody r = hit.rigidbody;
			hitBody = r;
			if (r == null)
				return;

			// Configure the spring joint to grab the token
			hitPosition = hit.point;
			pullSpring.connectedBody = r;
			pullSpring.connectedAnchor = r.transform.InverseTransformPoint(hitPosition);
			pullSpring.spring *= forceMultiplier;
			faceDirection = hit.normal;
			initialMousePosition = Input.mousePosition;
			
			// Notify the state machine that a block has been grabbed
			if (stateMachine != null)
				stateMachine.setBlockGrabbed(r);
		}
	}

	// Function to release the grabbed block when the user release the mouse button
	void releaseBlock() {
		// Check if there is a block grabbed
		if (pullSpring.connectedBody != null) {

			// Disconect the token from the spring joint
			pullSpring.connectedBody = null;

			// Notify the state machine that the block has been released
			if (stateMachine != null)
				stateMachine.releaseBlock();
		}
	}

	void updateForce() {
		// Pull direction is computed using the orientation of the camera
		Vector3 pullDirection = cam.transform.right *(Input.mousePosition.x-initialMousePosition.x) +
								cam.transform.up * (Input.mousePosition.y-initialMousePosition.y);

		// Pull direction is corrected using the normal of the hitted face
		// with this correction it's easier to play 
		// because the system helps the user to pull in the correct direction
		float dot = Vector3.Dot(pullDirection, faceDirection);
		if (dot < 0)
			faceDirection *= -1;
		pullDirection = Vector3.Lerp(pullDirection,faceDirection*pullDirection.magnitude,0.3f);
		pullDirection /= Screen.width;
		pullDirection *= (hitBody.position - cam.transform.position).magnitude;

		// Finally the object thas pulls from the joint if moved in the computed direction
		pullObject.transform.position = hitPosition + pullDirection;
	}

	void pullForce() {
		if (Input.GetMouseButtonDown(0)) {
			grabBlock();
		} else if (!Input.GetMouseButton(0)) {
			releaseBlock();
		}

		// Update force
		if (pullSpring.connectedBody != null) {
			updateForce();
		}
	}

}
