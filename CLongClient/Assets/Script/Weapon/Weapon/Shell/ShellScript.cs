using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellScript : MonoBehaviour {

    public float shellSpeed;
    public byte clientNum;
    public int damage;
    public Transform parentTransform;
    public bool coroutineCheck = true;

    public GameObject SparkEffect;

    public float timetest;
    public int frame;
	// Use this for initialization
	private void Start () {
        Destroy(this.parentTransform.gameObject, 3f);
        parentTransform.GetComponent<Rigidbody>().AddForce(parentTransform.forward * shellSpeed, ForceMode.Impulse);
        // StartCoroutine(ShellMove());
    }
	
	// Update is called once per frame
	private void FixedUpdate () {
      
        //parentTransform.Translate(Vector3.forward * shellSpeed * Time.deltaTime);
        //parentTransform.GetComponent<Rigidbody>().AddForce(-transform.up * shellSpeed);
        Debug.Log("확인 : " + this.gameObject.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ground")
        {
            var ef = Instantiate(SparkEffect);
            ef.transform.position = this.transform.position;
            var dir = other.transform.position - this.transform.position;
            dir = dir.normalized;
            ef.transform.rotation =  Quaternion.LookRotation(dir);
            
            Destroy(this.gameObject);
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
