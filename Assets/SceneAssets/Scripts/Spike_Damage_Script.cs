using UnityEngine;
using System.Collections;

public class Spike_Damage_Script: MonoBehaviour 
{

	void OnCollisionEnter(Collision collision)
	{
		
		KinectCharacterController player = collision.gameObject.GetComponent<KinectCharacterController>() ;
		
		if ( player != null )
		{
			player.gameObject.GetComponent<Rigidbody>().AddForce( this.gameObject.transform.up * 10, ForceMode.Impulse) ;
			player.health -= 55 ;
		}
		
	}
}
