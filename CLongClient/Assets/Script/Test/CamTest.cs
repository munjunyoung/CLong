using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamTest : MonoBehaviour
{
    public IKExample ik;

    bool follow = true;
	// Update is called once per frame
	void Update ()
    {
        if(Input.GetKeyDown(KeyCode.F12))
        {
            follow = !follow;
        }

        if (follow)
        {
            transform.Rotate(-Input.GetAxis("Mouse Y"), 0, 0);
            transform.Rotate(0, Input.GetAxis("Mouse X"), 0, Space.World);
            //ik.lookTarget = transform.forward * 100;
        }
	}
}
