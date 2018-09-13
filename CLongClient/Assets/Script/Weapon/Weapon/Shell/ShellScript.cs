using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellScript : MonoBehaviour {

    public float shellSpeed;
    public byte clientNum;
    public int damage;
    public Transform parentTransform;
    public bool coroutineCheck = true;

	// Use this for initialization
	private void Start () {
        Destroy(this.parentTransform.gameObject, 3f);
        parentTransform.GetComponent<Rigidbody>().AddForce(-transform.up * shellSpeed, ForceMode.Impulse);
        // StartCoroutine(ShellMove());
    }
	
	// Update is called once per frame
	private void FixedUpdate () {
        //parentTransform.Translate(Vector3.forward * shellSpeed * Time.deltaTime);
        //parentTransform.GetComponent<Rigidbody>().AddForce(-transform.up * shellSpeed, ForceMode.Acceleration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            coroutineCheck = false;
      
        }
        
    }

    IEnumerator ShellMove()
    {
        while (coroutineCheck)
        {
           
            yield return null;
        }
    }
}
