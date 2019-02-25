using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class JengaMatchMaker : MonoBehaviour
{
    public int serverPort=10100;
	public string serverIP = "127.0.0.1";
    public GameObject userButton;
    public GameObject usersContainer;
    public InputField inputName;
    public string InteractionType = "";
    public GameObject panelInvitation;
    public Text labelInvitation;
    public GameObject panelWaiting;
    public Text labelWaiting;
    public JengaUser userSelected = null;
    public string jengaGameScene;

    private JengaUser localUser = null;

	UdpClient udp = null;
	IPEndPoint ep = null;

    private bool loading = false;

    // Start is called before the first frame update
    void Start()
    {
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        loading = false;
        string userName = PlayerPrefs.GetString("userName","");
        if (userName != "") {
            inputName.text = userName;
            Login();
        }

        initSocket();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if loading new scene
        if (loading) {
            
            if (!IsInvoking("loadJengaGame")) {
                Invoke("loadJengaGame",1.0f);
            }
            return;
        }

        // Get the current users list
        JengaUserButton[] jubs = usersContainer.gameObject.GetComponentsInChildren<JengaUserButton>();

        // if udp is not working properly, then not continue
        if ((udp == null) || (ep == null) || (udp.Client == null) )
            return;

        // set a very low timeout to avoid waiting time in update
        udp.Client.ReceiveTimeout = 5;
		try {

            // try to load 20 packets to avoid acumulation of the information
            for (int i= 0; i<20; i++) {

                if (udp.Client.Available <= 0)
                    break;

                IPEndPoint ep2 = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = udp.Receive(ref ep2);

                string msg = System.Text.Encoding.ASCII.GetString(bytes);
                JengaMatchMakerMessage jmmm = JsonUtility.FromJson<JengaMatchMakerMessage>(msg);
               
                if (jmmm.cmd == "userList") {
                    ToggleGroup tg = usersContainer.GetComponent<ToggleGroup>();
                    foreach(JengaUser u in jmmm.users) {
                        
                        // Search the user
                        JengaUserButton ju = null;
                        foreach (JengaUserButton j in jubs) {
                            if ((j.user.ip == u.ip) && (j.user.port == u.port)) {
                                ju = j;
                                break;
                            }
                        }

                        // Instantiate new user button
                        if (ju == null) {
                            
                            GameObject button = Instantiate(userButton, usersContainer.transform);
                            ju = button.GetComponent<JengaUserButton>();
                            Toggle t = button.GetComponent<Toggle>();
                            t.group = tg;
                        }
  
                        // Set user data to the button
                        ju.setUser(u);
                    }
                }

                // Invitation to play received
                else if (jmmm.cmd == "playInvitation") {
                    Debug.Log("Play invitation");
                    panelInvitation.SetActive(true);
                    userSelected = jmmm.user;
                    userSelected.ip = ep2.Address.ToString();
                    userSelected.port = ep2.Port;
                    labelInvitation.text = "Do you want to play with " + userSelected.name + "?\n" + userSelected.type;
                    PlayerPrefs.SetInt("Invited",1);
                }

                // Invitation to play accepted
                else if (jmmm.cmd == "accept") {
                    acceptMatch();
                    acceptedMatch();
                }

                // Invitation rejected
                else if (jmmm.cmd == "reject") {
                    panelWaiting.SetActive(false);
                }

                // Invitation canceled
                else if (jmmm.cmd == "cancel") {
                    panelInvitation.SetActive(false);
                }

            }
        } catch (SocketException e) {
            // if (e.SocketErrorCode != SocketError.TimedOut)
            //     udp = null;
        }

        // Clean old users
        foreach (JengaUserButton j in jubs) {
            if ((Time.time - j.user.lastUpdate) > 15.0f) {
                Destroy(j.gameObject, 0.3f);
            }
        }
    } 

    void initSocket() {
        Debug.Log("Init socket");
        ep = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
		udp = new UdpClient();
    }

    void loadJengaGame() {
        CancelInvoke("sendKeepAlive");

        // Close the socket, the same portnumber will be used later in the game
        try {
        if (udp != null)
            udp.Close();
        } catch (System.Exception e) {

        }
        udp = null;

        // Load the game scene
        SceneManager.LoadScene(jengaGameScene);
    }

    void sendKeepAlive() {
        if (loading)
            return;

        JengaMatchMakerMessage jmmm = new JengaMatchMakerMessage();
        jmmm.cmd = "keepAlive";
        sendData(jmmm);
    }

    public void playAlone() {
        
        PlayerPrefs.SetInt("Play Alone", 1);
        
        loading = true;
    }

    public void acceptMatch() {
        if (!loading) {

            JengaMatchMakerMessage jmmm = new JengaMatchMakerMessage();
            jmmm.cmd = "accept";
            jmmm.user = localUser;
            sendData(jmmm,userSelected);
        }
    }

    public void acceptedMatch() {
        Leave();

        loading = true;

        PlayerPrefs.SetInt("localPort", ((IPEndPoint)udp.Client.LocalEndPoint).Port);
        PlayerPrefs.SetInt("remotePort", userSelected.port);
        PlayerPrefs.SetString("remoteIP", userSelected.ip);
        PlayerPrefs.SetInt("Play Alone", 0);
    }

    public void rejectMatch() {
        JengaMatchMakerMessage jmmm = new JengaMatchMakerMessage();
        jmmm.cmd = "reject";
        sendData(jmmm, userSelected);
        panelInvitation.SetActive(false);
    }

    public void startMatch() {
        JengaUserButton[] jubs = usersContainer.gameObject.GetComponentsInChildren<JengaUserButton>();
        foreach (JengaUserButton j in jubs) {
            Toggle t = j.gameObject.GetComponent<Toggle>();
            if (t.isOn) {
                userSelected = j.user;
                panelWaiting.SetActive(true);
                labelWaiting.text = "Waiting the answer from " + userSelected.name + "\n" + userSelected.type;

                JengaMatchMakerMessage jmmm = new JengaMatchMakerMessage();
                jmmm.cmd = "playInvitation";
                jmmm.user  = new JengaUser();
                jmmm.user.name = inputName.text;
                jmmm.user.type = InteractionType;
                sendData(jmmm, userSelected);
                PlayerPrefs.SetInt("Invited",0);
                break;
            }
        }
    }

    public void cancelMatch() {
        panelWaiting.SetActive(false);
        JengaMatchMakerMessage jmmm = new JengaMatchMakerMessage();
        jmmm.cmd = "cancel";
        sendData(jmmm, userSelected);
    }

    public void Login() {
        PlayerPrefs.SetString("userName",inputName.text);

        JengaMatchMakerMessage jmmm = new JengaMatchMakerMessage();
        jmmm.cmd = "join";
        localUser = new JengaUser();
        localUser = new JengaUser();
        localUser.name = inputName.text;
        localUser.type = InteractionType;
        jmmm.user = localUser;
        sendData(jmmm);
        InvokeRepeating("sendKeepAlive",1,10);
    }

    public void Leave() {
        JengaMatchMakerMessage jmmm = new JengaMatchMakerMessage();
        jmmm.cmd = "leave";
        jmmm.user  = new JengaUser();
        jmmm.user.name = inputName.text;
        jmmm.user.type = InteractionType;
        sendData(jmmm);
    }

    bool sendData(JengaMatchMakerMessage data, JengaUser u=null) {
        if (udp == null)
            return false;

		try {
            string json = JsonUtility.ToJson(data);
            byte[] send_buffer = System.Text.Encoding.ASCII.GetBytes(json);
			
            IPEndPoint e = ep;
            if (u != null) {
                e = new IPEndPoint(IPAddress.Parse(u.ip), u.port);
            }

            int res = udp.Send(send_buffer,send_buffer.Length,e);
            if (res != send_buffer.Length)
                return false;

			return true;
			
		} catch (System.Exception e) {
			Debug.LogError(e.Message);
			return false;
		}
	}

}
