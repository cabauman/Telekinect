//DLoment-10/05/11
//Upon collision with an any object, the projectile will find out if the collision was with the player or not. 
//If yes, then projectileDamage will decrement the health of the character and destroy itself.
//If no, then it will just destroy itself.

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))]
public class Projectile_Script : MonoBehaviour {

    public float                projectileDamage = 20.0f;
    public float                lifeTime = 1.0f;
	//public float				explosionTime = 1.0f;
    public float                explosionRadius;
	public GameObject			explodeEffect;
    public float                turnSpeed = 0.0f;

    public Transform			target;
    public Element_Base     	element;
	public GameObject			creator;
	
	void Start()
	{
		if( Network.isClient )
			this.enabled = false;
	}

	void Update()
	{
		//if we have a target, and the ability to seek, adjust our heading.
		if(target != null && turnSpeed != 0.0f)
		{
			//Grab the current speed of the projectile
			float speed = this.gameObject.GetComponent<Rigidbody>().velocity.magnitude;

			//interpolate the velocity based on how fast we can change
            this.gameObject.GetComponent<Rigidbody>().velocity = Vector3.Lerp(this.gameObject.GetComponent<Rigidbody>().velocity,
															 (target.transform.position - this.gameObject.transform.position).normalized * speed,
															 turnSpeed);
		}

		lifeTime -= Time.deltaTime;
		if(lifeTime <= 0.0f)
		{
			Die();
		}
	}


    void OnCollisionEnter(Collision collisionObject)
    {
		if( ! this.enabled )
			return;

		//projectile created by the player - damage enemies
		if(creator != null && creator.layer == 9)
		{
			Agent_FSM tempAgent = collisionObject.collider.gameObject.GetComponent<Agent_FSM>();
			if(tempAgent != null)
			{
				tempAgent.health -= projectileDamage;
				Die();
			}
		}
		else //projectile created by something else - damage the player
		{
			KinectCharacterController tempPlayer = collisionObject.collider.gameObject.GetComponent<KinectCharacterController>();
 			Element_Base tempElement = collisionObject.collider.gameObject.GetComponent<Element_Base>() ;
			
			if (tempPlayer != null)
			{
				if(Network.peerType == NetworkPeerType.Disconnected || tempPlayer.GetComponent<NetworkView>().isMine)
					tempPlayer.health -= projectileDamage; 
				else
					tempPlayer.GetComponent<NetworkView>().RPC("DamageClientPlayer", tempPlayer.GetComponent<NetworkView>().owner, projectileDamage);

				Die(); //kill the projectile
			}
			else
			{
				if( tempElement != null && tempElement.ID == this.element.ID )
				{
					return ; //hit the same type of element, don't do anything
				}
//				else 
//					this.gameObject.rigidbody.useGravity = true ;//hit anything other than player/same element, use grav
				
			}
			
		}
    }

	public void Die()
	{
		//if the creator is the player, cause damage to objects in the selectable layer( layer 10 ) (agents are in this layer), otherwise, damage the player( layer 9 )
		int layerToDamage;
		if (creator == null)
			layerToDamage = 9;
		else
			layerToDamage = creator.layer == 9 ? 10 : 9;

		//explode if we can
		if (explodeEffect != null)
		{
			//spawn the effect
			if (Network.isServer)
				Network.Instantiate(explodeEffect, this.gameObject.transform.position, this.gameObject.transform.rotation, 0);
			else
				Instantiate(explodeEffect, this.gameObject.transform.position, this.gameObject.transform.rotation);

			if (explosionRadius > 0.0f)
			{
				Collider[] caughtInTheExplosion =
					Physics.OverlapSphere(this.gameObject.transform.position, explosionRadius);

				foreach (Collider damaged in caughtInTheExplosion)
				{
					if (damaged.gameObject != creator) //exclude the creator of the projectile from the explosion
					{

						//anything caught in the explosion gets pushed
						if (damaged.GetComponent<Rigidbody>() != null && (layerToDamage == 10 ? damaged.gameObject.layer != 9 : true))
							damaged.GetComponent<Rigidbody>().AddExplosionForce(projectileDamage, this.gameObject.transform.position, explosionRadius, 1.5f, ForceMode.Impulse);

						//check if the object is in the layer we want to damage
						if (damaged.gameObject.layer == layerToDamage)
						{
							//damage the player
							if (layerToDamage == 9)
							{
								//Grab a reference to the player
								KinectCharacterController player = damaged.gameObject.GetComponent<KinectCharacterController>();
								//find the distance from the center of the explosion to the player
								float playerDistance = (damaged.gameObject.transform.position - this.gameObject.transform.position).magnitude;
								//damage the player based on how far away from the explosion they are
								float damage = (1 - (playerDistance / explosionRadius)) * projectileDamage;

								if ( player.GetComponent<NetworkView>().isMine || Network.peerType == NetworkPeerType.Disconnected )
									player.health -= damage;
								else
								{
									player.GetComponent<NetworkView>().RPC("DamageClientPlayer", player.GetComponent<NetworkView>().owner, damage);

									Vector3 direction = player.gameObject.transform.position - this.gameObject.transform.position;
									float distance = direction.magnitude;
									direction = direction / distance;

									player.GetComponent<NetworkView>().RPC("PushClientPlayer", player.GetComponent<NetworkView>().owner, projectileDamage, this.gameObject.transform.position, explosionRadius);
								}
							}
							//damage agents
							else
							{
								//Grab a reference to the agent
								Agent_FSM agent = damaged.gameObject.GetComponent<Agent_FSM>();
								if (agent != null)
								{
									//find the distance from the center of the explosion to the player
									float agentDistance = (damaged.gameObject.transform.position - this.gameObject.transform.position).magnitude;
									//damage the player based on how far away from the explosion they are
									agent.health -= (1 - (agentDistance / explosionRadius)) * projectileDamage;
								}
							}
						} //damage object
					}//if(damaged.gameObject != creator)
				}//foreach collider
			}//if (explosionRadius > 0.0f)
		}//if(explodeEffect != null)

		if (Network.isServer)
		{
			Network.RemoveRPCs(this.GetComponent<NetworkView>().viewID);
			Network.Destroy(this.GetComponent<NetworkView>().viewID);
		}
		else
			Destroy(this.gameObject);

	}

//	void OnSerializeNetworkView ( BitStream stream, NetworkMessageInfo info )
//	{
//		Vector3 pos = Vector3.zero;
//		Vector3 velocity = Vector3.zero;
//		//Quaternion rot = Quaternion.identity;
//		//Vector3 angularVelocity = Vector3.zero;
//		
//		// Send data 
//		if ( stream.isWriting )
//		{
//			//Debug.Log("Writing to Stream");
//
//			pos = rigidbody.position;
//			//rot = rigidbody.rotation;
//			velocity = rigidbody.velocity;
//			//angularVelocity = rigidbody.angularVelocity;
//
//			stream.Serialize(ref pos);
//			stream.Serialize(ref velocity);
//			//stream.Serialize(ref rot);
//			//stream.Serialize(ref angularVelocity);
//		}
//		// Read data
//		else
//		{
//			stream.Serialize(ref pos);
//			stream.Serialize(ref velocity);
//			//stream.Serialize(ref rot);
//			//stream.Serialize(ref angularVelocity);
//
//			rigidbody.position = pos;
//			//rigidbody.rotation = rot;
//			rigidbody.velocity = velocity;
//			//rigidbody.angularVelocity = angularVelocity;
//		}
//	}
}