using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellScript : MonoBehaviour {

    public float shellSpeed;
    public byte clientNum;
    public int damage;
    public Transform parentTransform;
	// Use this for initialization
	private void Start () {
        Destroy(this.gameObject, 3f);
	}
	
	// Update is called once per frame
	private void FixedUpdate () {
        parentTransform.Translate(Vector3.forward * shellSpeed * Time.deltaTime);
	}

    private void OnTriggerEnter(Collider other)
    {
    }
}
