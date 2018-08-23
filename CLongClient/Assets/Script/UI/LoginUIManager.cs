using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CLongLib;

public class LoginUIManager : Singleton<LoginUIManager>
{
    public Text id;
    public Text pw;

    public void OnClickLogin()
    {
        var strId = id.text;
        var strPw = pw.text;
        NetworkManager.Instance.SendPacket(new Login_Req(strId, strPw), NetworkManager.Protocol.TCP);
    }

    public void OnClickSetting()
    {

    }

    protected override void Init()
    {
        
    }
}
