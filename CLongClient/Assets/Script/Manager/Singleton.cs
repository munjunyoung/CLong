using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour
{
    private static T _instance;

    public static T Instance { get { return _instance; } }

    protected virtual void Awake()
    {
        //if (_instance == null)
        {
            _instance = GetComponent<T>();
            Init();
        }
        //}
        //else
        //{
        //    Debug.LogError("Scene has two <color=red>" + _instance.GetType() + "</color> components. Destroy new one.", gameObject);
        //    Destroy(gameObject);
        //}
    }

    protected virtual void Init()
    {
        Debug.LogError("Please override Init method of <color=red>" + _instance.GetType() + "</color>.");
    }
}
