using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JengaMatchMakerMessage 
{
    public string cmd;
    public JengaUser[] users;
    public JengaUser user;

    public JengaMatchMakerMessage() {
        cmd = "";
        users = new JengaUser[0];
        user = null;
    }
}
