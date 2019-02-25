using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBlockHandler : MonoBehaviour {
	// Function called by the state machine to notify the begining of the turn
	public virtual void startTurn() {}
	// Function called by the state machine to notify the end of the turn
	public virtual void endTurn() {}
}
