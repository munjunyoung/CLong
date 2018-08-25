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
        if(other.transform.tag == "Ground")
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Grounded(myPlayer.clientNum, true), NetworkManager.Protocol.TCP);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Ground")
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Grounded(myPlayer.clientNum, false), NetworkManager.Protocol.TCP);
        }
    }
}
