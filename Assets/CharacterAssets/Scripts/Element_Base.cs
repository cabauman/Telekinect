using UnityEngine;
using System.Collections;

abstract public class Element_Base : MonoBehaviour
{
	//initialize data
	public int ID = 0;
	public float Elemental_Power = 100.0f;
	public bool isPool = false;
	public bool useParticle = false;
	public bool useMesh = false ;
	public bool canTransfer = false;
	public bool destroyObject = true;
	public GameObject particleSystemObject;
	public GameObject elementMesh;
	
	public float timeToDestroy = -1.0f;
	
	public virtual void Update()
	{
		if(Network.isClient)
			return;
		
		if(timeToDestroy > -1.0f)
		{
			timeToDestroy -= Time.deltaTime;
			if(timeToDestroy <= 0.0f)
				DestroyElement();
		}
	}
	
	public void DestroyElement()
	{
		 //If we aren't attached to an object that wishes to live, destroy this game object.
		if(destroyObject)
		{
			if( Network.isServer )
			{
				Network.RemoveRPCs(this.gameObject.GetComponent<NetworkView>().viewID);
				Network.Destroy(this.gameObject.GetComponent<NetworkView>().viewID);
			}
			else if( ! Network.isClient )
				Destroy(this.gameObject);

		}
		else
		{
			if( Network.isServer )
				this.GetComponent<NetworkView>().RPC("RemoteDestroy", RPCMode.OthersBuffered);

			Destroy(this.particleSystemObject);
			Destroy(this.elementMesh) ;
			Destroy(this);
		}		
	}
	
	public virtual void AttachElementParticles()
	{
		//Implemented per element
		Debug.Log("Base Element AttachParticles Called") ;
	}

	[RPC]
	public void RemoteDestroy()
	{
		Destroy(this.particleSystemObject);
		Destroy(this.elementMesh) ;
		Destroy(this);
	}
}
