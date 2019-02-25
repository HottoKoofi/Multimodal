using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]

public class JengaStateMachine : MonoBehaviour {

	// Last block grabbed in the turn
	Rigidbody lastBlockGrabbed = null;
	// Block handler interface
	public IBlockHandler jengaHandler;
	// Socket to control the match
	JengaMatch jengaMatch;
	// Animator with the state machine
	Animator animator;
	// Block grabbed flag
	bool blockGrabbed;

	public GameObject buttonWin;
	public GameObject buttonLose;
	public GameObject buttonLost;
	public Text yourTurn;

	public int tColor;
	public Color turnColor;

	public string sceneBack;

	public float lastData = 0;

	// Use this for initialization
	void Start () {

		// Init random color seed
		Random.InitState(System.DateTime.Now.Millisecond);

		// Initialize variables
		lastBlockGrabbed = null;
		blockGrabbed = false;
		animator = GetComponent<Animator>();
		jengaMatch = GetComponent<JengaMatch>();

		// Check play alone
		if (animator != null)
			animator.SetBool("Play Alone", PlayerPrefs.GetInt("Play Alone", 1) == 1);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
			reloadScene();

		buttonLost.SetActive((Time.time - jengaMatch.lastData) > 5);
	}

	public void receivedStartTurn() {
		if (animator != null)
			animator.SetTrigger("Start Turn");
	}

	public void startTurn() {
		// Generate Random color
		tColor = Random.Range(0,3);
		turnColor = new Color(tColor == 0?1:0, tColor == 1?1:0, tColor == 2?1:0, 1);

		// Reset turn variables
		blockGrabbed = false;
		lastBlockGrabbed = null;
		jengaMatch.isMoving = true;
		if (yourTurn != null) {
			string[] colores = {"red", "green", "blue"};
			yourTurn.text = "Your Turn, color " + colores[tColor];
			yourTurn.color = turnColor;
			yourTurn.gameObject.SetActive(true);
		}

		// Enable and notify handler
		jengaHandler.enabled = true;
		jengaHandler.startTurn();
	}

	public void disableHandler() {
		if (jengaHandler.enabled) {
			jengaHandler.endTurn();
			jengaHandler.enabled = false;
		}
		if (yourTurn != null)
			yourTurn.gameObject.SetActive(false);
	}

	public void blockFalls() {
		disableHandler();
	}

	public void endTurn() {
		disableHandler();
		if (lastBlockGrabbed != null) {
			lastBlockGrabbed.gameObject.SetActive(false);
			lastBlockGrabbed.gameObject.GetComponent<JengaBlock>().e = false;
		}
		lastBlockGrabbed = null;
		jengaMatch.sendEndTurn();
		jengaMatch.isMoving = false;
		Invoke("sentEndTurn", 1.0f);
		// if (animator != null)
		// 	animator.SetTrigger("Start View");
	}

	public void sentEndTurn() {
		if (animator != null)
			animator.SetTrigger("Start View");
	}

	public void setBlockGrabbed(Rigidbody b) {
		JengaBlock	jb = b.gameObject.GetComponent<JengaBlock>();
		int bColor = 5;
		if (jb != null)
			bColor = jb.color;

		if ((bColor != tColor) || ((lastBlockGrabbed != null) && (lastBlockGrabbed != b))) {
			Debug.Log("Wrong block");
			if (animator != null)
				animator.SetTrigger("Wrong Block Grabbed");
		} else {
			lastBlockGrabbed = b;
		}
		
		blockGrabbed = true;
	}

	public void releaseBlock() {
		blockGrabbed = false;
	}

	public void blockTouchesGround(Rigidbody b) {
		
		if (lastBlockGrabbed == null)
			return;

		JengaBlock jb = b.GetComponent<JengaBlock>();
		if (jb == null)
			return;

		if (jb.floor == 0)
			return;

		if (b != lastBlockGrabbed) {
			Debug.Log(gameObject.name + ": Wrong block falls");
			if (animator != null)
				animator.SetTrigger("Wrong Block Falls");
		} else {
			blockFalls();
			if (animator != null)
				animator.SetTrigger("Block Falls");
		}
	}

	public void lose() {
		jengaMatch.sendLost();
		if (buttonLose != null)
			buttonLose.SetActive(true);
	}

	public void win() {
		if (buttonWin != null)
			buttonWin.SetActive(true);
	}

	public void reloadScene() {
		Scene scene = SceneManager.GetActiveScene(); 
		SceneManager.LoadScene(sceneBack);
	}
}
