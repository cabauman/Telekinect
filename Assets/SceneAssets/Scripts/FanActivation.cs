using UnityEngine;
using System.Collections;

public class FanActivation : MonoBehaviour 
{

	void OnTriggerStay(Collider collider)
	{		
		if( collider.gameObject.GetComponent<Rigidbody>() != null && collider.gameObject.layer != 14)
		{
			Debug.Log("Entered fan area");
			//collider.gameObject.rigidbody.useGravity = false;
			Vector3 forceVector = Vector3.Lerp(this.gameObject.transform.up, this.gameObject.transform.forward, 0.5f).normalized;
			collider.gameObject.GetComponent<Rigidbody>().AddForce(forceVector * 5.0f, ForceMode.Impulse);
			//this.gameObject.rigidbody.velocity = new Vector3(0, 0, -500);
		}
	}
	
	[RPC]
	public void Deactivate()
	{
		this.gameObject.active = false ;
	}
	
	[RPC]
	public void Activate()
	{
		this.gameObject.active = true ;
	}
	
//	void OnTriggerExit(Collider other)
//	{
//		if(other.gameObject.layer == 9)
//		{	
//			Debug.Log("Exit fan area");
//		}
//	}
}
