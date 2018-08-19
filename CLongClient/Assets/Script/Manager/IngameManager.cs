using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;

public class IngameManager : Singleton<IngameManager>
{
    protected override void Init()
    {
        NetworkManager.Instance.RecvHandler += ProcessPacket;
    }
	
	// Update is called once per frame
	private void Update ()
    {
		
	}

    private void ProcessPacket(IPacket p, NetworkManager.Protocol pt)
    {
        if (pt == NetworkManager.Protocol.TCP)
        {
            if (p is Player_Init)
            {
                var s = (Player_Init)p;
            }
            if (p is Player_Reset)
            {
                var s = (Player_Reset)p;
            }
            if (p is Round_Start)
            {
                var s = (Round_Start)p;
            }
            if (p is Round_End)
            {
                var s = (Round_End)p;
            }
            if (p is Match_End)
            {
                var s = (Match_End)p;
                NetworkManager.Instance.RecvHandler -= ProcessPacket;
            }
            if (p is Round_Timer)
            {
                var s = (Round_Timer)p;
            }
        }
        else
        {

        }
    }
}
