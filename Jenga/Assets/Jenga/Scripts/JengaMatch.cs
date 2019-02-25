using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

public class JengaMatch : MonoBehaviour {

	public JengaStateMachine stateMachine;
	public int portNumber;
	public int remotePortNumber;
	public string remoteIP = "127.0.0.1";
	UdpClient udp;
	IPEndPoint ep;

	public bool isMoving = false;
	public GameObject tower;
	
	public Dictionary<int,JengaBlock> dblocks;

	public float lastData = 0;

	[System.Serializable]
	public class JengaData {
		public float timestamp;
		public JengaBlockData[] blocks;
		public bool endTurn;
		public bool lost;
		public int blockMoving;

		public JengaData() {
			timestamp = Time.time;
			endTurn = false;
			lost = false;
			blocks = null;
			blockMoving = -1;
		}
	}

	[System.Serializable]
	public class JengaBlockData {
		public int id;
		public float[] p;
		public float[] r;
		public bool e;

		public JengaBlockData(JengaBlock b) {
			id = b.id;
			p = new float[]{b.p.x, b.p.y, b.p.z};
			r = new float[]{b.r.x, b.r.y, b.r.z, b.r.w};
			e = b.e;
		}
	}

	// Use this for initialization
	void Start () {

		remoteIP = PlayerPrefs.GetString("remoteIP", remoteIP);
		remotePortNumber = PlayerPrefs.GetInt("remotePort", remotePortNumber);
		portNumber = PlayerPrefs.GetInt("localPort",10101);

		JengaBlock[] blocks = tower.GetComponentsInChildren<JengaBlock>();
		dblocks = new Dictionary<int, JengaBlock>();
		//int cont = 0;
		foreach(JengaBlock b in blocks) {
			//b.id = cont++;
			dblocks[b.id] = b;
		}

		initSocket();
		
		if (portNumber > remotePortNumber)  
			stateMachine.receivedStartTurn();
		else if (portNumber == remotePortNumber) {
			if (PlayerPrefs.GetInt("Invited",0) == 0) 
				stateMachine.endTurn();
			else
				stateMachine.receivedStartTurn();
		} else {
			stateMachine.endTurn();
		}
			

	}
	
	// Update is called once per frame
	void Update () {
		enablePhysx(isMoving);
		if (isMoving) {
			lastData = Time.time;
			if (!IsInvoking("sendBlocks"))
				InvokeRepeating("sendBlocks", 0.5f, 0.1f);
		}
		else if (!isMoving) {
			CancelInvoke("sendBlocks");
		}
		receiveBlocks();
	}

	void initSocket() {
		ep = new IPEndPoint(IPAddress.Parse(remoteIP), remotePortNumber);
		udp = new UdpClient(portNumber);
	}

	void enablePhysx(bool value) {
		Rigidbody[] bs = tower.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody b in bs) {
			b.isKinematic = !value;
			//b.enabled = value;
		}
	}

	void receiveBlocks() {
		if (udp == null)
			return;

		udp.Client.ReceiveTimeout =5;
		try {

			for (int i= 0; i<20; i++) {
				
				IPEndPoint ep2 = new IPEndPoint(IPAddress.Any, 0);
				byte[] bytes = udp.Receive(ref ep2);

				if (!isMoving)
				{
					JengaData data;
					BinaryFormatter bf = new BinaryFormatter();
					using (MemoryStream ms = new MemoryStream(bytes)) {
						data = (JengaData)bf.Deserialize(ms);

						lastData = Time.time;

						// PARSE BLOCK INFORMATION
						if (data.blocks != null)
						foreach (JengaBlockData b in data.blocks) {
							//if (dblocks[b.id] != null) {
								dblocks[b.id].p = new Vector3(b.p[0], b.p[1], b.p[2]);
								dblocks[b.id].r = new Quaternion(b.r[0], b.r[1], b.r[2], b.r[3]);
								dblocks[b.id].e = b.e;
							//}
						}

						if (data.endTurn == true) {
							stateMachine.receivedStartTurn();
						}

						if (data.lost == true) {
							stateMachine.win();
						}
					}
				}
			}

		} catch (SocketException e) {
            
		} catch(System.Exception e) {

		}

	}

	void fillBlocks(ref JengaData data) {
		List<JengaBlockData> lblocks = new List<JengaBlockData>();
		foreach (KeyValuePair<int,JengaBlock> b in dblocks) {
			if (b.Value != null)
				lblocks.Add(new JengaBlockData(b.Value));
		}
		data.blocks = lblocks.ToArray();
	}

	void sendBlocks() {
		JengaData data = new JengaData();
		fillBlocks(ref data);
		sendData(data);
	}

	public void sendEndTurn() {
		lastData = Time.time;
		JengaData data = new JengaData();
		fillBlocks(ref data);
		data.endTurn = true;
		sendData(data);
	}

	public void sendLost() {
		JengaData data = new JengaData();
		fillBlocks(ref data);
		data.lost = true;
		sendData(data);
	}

	bool sendData(JengaData data) {
		if (udp == null)
            return false;

		try {
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream()) {
				bf.Serialize(ms, data);
				byte[] bytes = ms.ToArray();
				int res = udp.Send(bytes,bytes.Length,ep);
				if (res != bytes.Length)
					return false;

				return true;
			}
		} catch (System.Exception e) {
			Debug.LogError(e.Message);
			return false;
		}
	}
}
