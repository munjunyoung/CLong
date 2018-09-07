using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;

public class IngameUIManager : Singleton<IngameUIManager>
{

    protected override void Init()
    {
        NetworkManager.Instance.RecvHandler += ProcessPacket;
    }

    private void ProcessPacket(IPacket p, NetworkManager.Protocol pt)
    {

    }
}
