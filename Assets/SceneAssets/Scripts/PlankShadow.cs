using UnityEngine;
using System.Collections;

public class PlankShadow : MonoBehaviour {

    public GameObject blobShadow;

	// Use this for initialization
	void Start () 
    {
        blobShadow = Instantiate(Resources.Load("Blob Shadow Projector")) as GameObject;
	}
	
	// Update is called once per frame
	void Update () {
        //float lockPos = 0.0f;
        //gameObject.GetComponent<Projector>().transform.rotation = Quaternion.Euler(lockPos, lockPos, lockPos);
        blobShadow.transform.position = gameObject.transform.position + (Vector3.up * 15.0f);

        Vector3 newRot = this.gameObject.transform.rotation.eulerAngles;
        blobShadow.transform.rotation = Quaternion.Euler(90.0f, newRot.y , 0.0f);

	}
}
