using UnityEngine;
using System.Collections;

public class CheckPoint : MonoBehaviour 
{
	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.layer == 9 && HotValues.Instance().spawnPoint != this)
		{
			if((Network.peerType != NetworkPeerType.Disconnected && other.gameObject.GetComponent<NetworkView>().isMine) ||
			    Network.peerType == NetworkPeerType.Disconnected )
			HotValues.Instance().spawnPoint = this;
		}
	}

	public Vector3 GenerateSpawnPoint()
	{
		Vector3 spawnPoint = Vector3.zero;
		float radius = this.GetComponent<Collider>().bounds.extents.x;

		//get a random direction
		spawnPoint.x = RNG.Instance().fUni(-1.0f, 1.0f);
		spawnPoint.z = RNG.Instance().fUni(-1.0f, 1.0f);
		spawnPoint.Normalize();

		//get a random magnitude within the bounds of the checkpoint radius
		spawnPoint *= (int)(RNG.Instance().fUni(0.0f, radius) + 0.5f);

		//reference spawnPoint from the checkpoint center
		spawnPoint += this.transform.position;

		Debug.Log("Spawn Point Generated: " + spawnPoint.ToString());

		return spawnPoint;
	}
}