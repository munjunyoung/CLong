using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour {
    public enum Behavior
    {
        Walk, Run, Death, Shoot, Jump
    }

    /// <summary>
    /// Set Animation by behaviorParam;
    /// </summary>
    /// <param name="be"></param>
    void SetAnimation(Behavior be) 
    {
        switch(be)
        {
            //..
        }
    }
}
