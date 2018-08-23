using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamTest : MonoBehaviour
{
   

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
           
        }
	}
}
