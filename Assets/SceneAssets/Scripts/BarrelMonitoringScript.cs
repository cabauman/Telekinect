using UnityEngine;
using System.Collections;

public class BarrelMonitoringScript : MonoBehaviour 
{
    public GameObject explodingBarrel;
    public float timer = 0.0f;
    	
	// Update is called once per frame
	void Update ()
    {
        if (explodingBarrel == null)
        {
            timer += Time.deltaTime;

            if (timer >= 5.0f)
            {
                timer = 0.0f;
                explodingBarrel = Instantiate(Resources.Load("Exploding_Barrel"), this.gameObject.transform.position, Quaternion.identity) as GameObject;
            }
        }
	}
}
