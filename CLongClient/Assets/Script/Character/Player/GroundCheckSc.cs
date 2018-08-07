using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using tcpNet;

public class GroundCheckSc : MonoBehaviour
{
    public Player myPlayer;
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "Ground")
        {
            NetworkManagerTCP.SendTCP(new ClientMoveSync(myPlayer.clientNum, IngameProcess.ToNumericVectorChange(myPlayer.transform.position)));
            NetworkManagerTCP.SendTCP(new IsGrounded(myPlayer.clientNum, true));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Ground")
        {
            NetworkManagerTCP.SendTCP(new ClientMoveSync(myPlayer.clientNum, IngameProcess.ToNumericVectorChange(myPlayer.transform.position)));
            NetworkManagerTCP.SendTCP(new IsGrounded(myPlayer.clientNum, false));
        }
    }
}
