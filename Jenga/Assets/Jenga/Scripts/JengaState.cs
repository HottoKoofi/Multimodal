using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JengaState : StateMachineBehaviour {

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		
		//Debug.Log("Enter");
		JengaStateMachine sm = animator.GetComponent<JengaStateMachine>();
		if (sm == null)
			return;

		if (stateInfo.IsName("Playing")) {
			//Debug.Log("Enter: Start Turn");
			sm.startTurn();			
		}

		if (stateInfo.IsName("End Turn")) {
			//Debug.Log("Enter: End Turn");
			sm.endTurn();
		}

		if (stateInfo.IsName("Lose")) {
			//Debug.Log("Enter: End Turn");
			sm.lose();
		}

		if (stateInfo.IsName("Win")) {
			//Debug.Log("Enter: End Turn");
			sm.win();
		}

		if (stateInfo.IsName("Viewing")) {
			//Debug.Log("Enter: Start Turn");
			sm.disableHandler();			
		}

		if (stateInfo.IsName("Block Fallen")) {
			animator.ResetTrigger("Block Falls");
		}

	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		
		JengaStateMachine sm = animator.GetComponent<JengaStateMachine>();
		if (sm == null)
			return;

		if (stateInfo.IsName("Playing")) {
			//Debug.Log("Enter: Start Turn");
			sm.disableHandler();			
		}

	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
