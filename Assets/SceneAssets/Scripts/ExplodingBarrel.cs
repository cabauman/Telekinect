using UnityEngine;
using System.Collections;

public class ExplodingBarrel : MonoBehaviour 
{
    public GameObject explosion;
    public int explosionDamage = 150;
	public float explosionForceMultiplier = 5.0f;
    public float explosionRadius = 5.0f;
	Vector3 explodeOffset = new Vector3(0.0f, 0.25f, 0.0f);
	public float explodeTime = 0.1f;
	bool isExploding = false;
	bool isExploded = false;
    
	void Update()
	{
		if(isExploding)
		{
			explodeTime -= Time.deltaTime;
			if(explodeTime <= 0.0f)
			{
				isExploded = true;
				Explode();
			}
		}
	}
	
    void OnCollisionEnter(Collision collision)
    {
		if(Network.isClient)
			return;
		
		if(collision.impactForceSum.magnitude > 20.0f)
			Explode();
		else
		{
	        Element_Base element = collision.collider.gameObject.GetComponent<Element_Base>();
			if(element != null && element.ID == -1)
	            Explode();
		}
    }

    void Explode()
    {
		if(!isExploded)
		{
			isExploding = true;
			return;
		}
		if(!this.enabled)
			return;
		else
			this.enabled = false;
		
        Collider[] caughtInTheExplosion = Physics.OverlapSphere(this.gameObject.transform.position, explosionRadius);
        foreach (Collider damaged in caughtInTheExplosion)
        {
			if(damaged == this.gameObject.GetComponent<Collider>())
				continue;
			
            //anything caught in the explosion gets pushed
            if (damaged.GetComponent<Rigidbody>() != null)
                damaged.GetComponent<Rigidbody>().AddExplosionForce(explosionDamage * explosionForceMultiplier, this.gameObject.transform.position, explosionRadius);

            //Grab a reference to the player
            KinectCharacterController player = damaged.gameObject.GetComponent<KinectCharacterController>();
            //Grab a reference to the agent
            Agent_FSM agent = damaged.gameObject.GetComponent<Agent_FSM>();
			if(player != null)
			{
				//find the distance from the center of the explosion to the player
				float playerDistance = (damaged.gameObject.transform.position - this.gameObject.transform.position).magnitude;
				//damage the player based on how far away from the explosion they are
				float damage = (1 - (playerDistance / explosionRadius)) * explosionDamage;

				if (player.GetComponent<NetworkView>().isMine)
					player.health -= damage;
				else
				{
					player.GetComponent<NetworkView>().RPC("DamageClientPlayer", player.GetComponent<NetworkView>().owner, damage);

					Vector3 direction = player.gameObject.transform.position - this.gameObject.transform.position;
					float distance = direction.magnitude;
					direction = direction / distance;

					player.GetComponent<NetworkView>().RPC("PushClientPlayer", player.GetComponent<NetworkView>().owner, explosionDamage * explosionForceMultiplier, this.gameObject.transform.position, explosionDamage);
				}
			}
            else if (agent != null && agent.ID != -1)
            {
                //find the distance from the center of the explosion to the player
                float agentDistance = (damaged.gameObject.transform.position - this.gameObject.transform.position).magnitude;
                //damage the player based on how far away from the explosion they are
                agent.health -= (1 - (agentDistance / explosionRadius)) * explosionDamage;
            }
			else
			{
				if(Network.peerType != NetworkPeerType.Disconnected && damaged.gameObject.GetComponent<NetworkView>() != null)
				{
					this.gameObject.GetComponent<NetworkView>().RPC("RemoteExplodeMessage", RPCMode.Others, damaged.gameObject.GetComponent<NetworkView>().viewID);
				}	
				damaged.gameObject.SendMessage("Explode", SendMessageOptions.DontRequireReceiver);
			}
        }
		if(Network.peerType != NetworkPeerType.Disconnected)
		{
			this.gameObject.GetComponent<NetworkView>().RPC("RemoteExplode", RPCMode.Others);
			Destroy(this.gameObject);
		}
		else
        	Destroy(this.gameObject);
		
        explosion = Instantiate(explosion, this.gameObject.transform.position + explodeOffset, Quaternion.identity) as GameObject;
    }
	
	[RPC]
	void RemoteExplode()
	{
		explosion = Instantiate(explosion, this.gameObject.transform.position + explodeOffset, Quaternion.identity) as GameObject;
		Destroy(this.gameObject);
	}
	
	[RPC]
	void RemoteExplodeMessage( NetworkViewID viewID )
	{
		NetworkView.Find(viewID).gameObject.SendMessage("Explode", SendMessageOptions.DontRequireReceiver);
	}
}
