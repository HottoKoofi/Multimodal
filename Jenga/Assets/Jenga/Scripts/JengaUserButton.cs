using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JengaUserButton : MonoBehaviour
{
    public JengaUser user;
    public Text labelName;
    public Text labelType;

    public void setUser(JengaUser u) {
        user = u;
        user.lastUpdate = Time.time;
        labelName.text = u.name;
        labelType.text = u.type;
    }
}
