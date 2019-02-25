using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JengaUser
{
    public string name;
    public string ip;
    public int port;
    public string type;

    [System.NonSerialized]
    public float lastUpdate;

    public JengaUser() {
        name = ip = type = "";
        port = 0;
        lastUpdate = 0;
    }
}
