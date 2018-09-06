using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using tcpNet;

public class PlayerGroundCheckSc : MonoBehaviour
{
    public Player myPlayer;
    private void OnTriggerEnter(Collider other)
    {
        if (!myPlayer.clientCheck)
            return;

        if(other.transform.tag == "Ground")
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(myPlayer.lookTarget), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Grounded(myPlayer.clientNum, true), NetworkManager.Protocol.TCP);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!myPlayer.clientCheck)
            return;

        if (other.transform.tag == "Ground")
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(myPlayer.lookTarget), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Grounded(myPlayer.clientNum, false), NetworkManager.Protocol.TCP);
        }
    }
}
