using UnityEngine;
using System.Collections;

public class Puzzle_MovingPlate: MonoBehaviour 
{
    public GameObject StartPoint, EndPoint;
	public float timer = 1.0f;

    void Start()
    {
		if(Network.peerType != NetworkPeerType.Disconnected && Network.isClient)
			this.enabled = false;
        //this.rigidbody.angularVelocity = new Vector3(0.0f, 0.0f, 1.0f);        
    }

	// Update is called once per frame
    void Update()
    {
		Vector3 toEndPoint = EndPoint.transform.position - this.gameObject.transform.position;
		float dist = toEndPoint.magnitude;
		
		if(dist <= 0.1f)
		{
			this.GetComponent<Rigidbody>().velocity = Vector3.zero;
			if(timer <= 0.0f)
			{
	            GameObject temp = StartPoint;
	            StartPoint = EndPoint;
	            EndPoint = temp;
				timer = 1.0f;
			}
			else
				timer -= Time.deltaTime;
		}
		else
			this.GetComponent<Rigidbody>().velocity = (toEndPoint / dist) * 1.5f;
    }
}
