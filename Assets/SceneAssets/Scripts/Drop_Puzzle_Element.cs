using UnityEngine;
using System.Collections;

public class Drop_Puzzle_Element : MonoBehaviour
{
	public Transform spawnPoint;
	public float lifeTimer = 0.0f;
	
	public void Respawn()
	{
		if(Network.peerType != NetworkPeerType.Disconnected && Network.isServer || ! Network.isClient)
		{
			if(spawnPoint != null)
			{
				this.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero ;
				this.gameObject.transform.position = spawnPoint.position ;
				lifeTimer = 0.0f;
			}
			else
			{
				Debug.Log("You have not set the spawn point for this puzzle element. Do so.") ;
			}
		}
	}
	
	public void OnCollisionEnter( Collision collision )
	{
		if(Network.isClient)
			return;
		
		if(collision.relativeVelocity.sqrMagnitude >= 100.0f)
		{
			lifeTimer = 2.0f;
		}
	}
	
	void Update()
	{
		if(Network.isClient)
			return;
		
		if(lifeTimer > 0.0f)
		{
			lifeTimer -= Time.deltaTime;
		}
		else if(lifeTimer < 0.0f)
		{
			Respawn();
		}
	}
}

