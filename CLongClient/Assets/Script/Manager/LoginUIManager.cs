using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginUIManager : Singleton<LoginUIManager>
{
    public Text id;
    public Text pw;

    public void OnClickLogin()
    {
        var strId = id.text;
        var strPw = pw.text;
        //send
    }

    protected override void Init()
    {
        
    }
}
