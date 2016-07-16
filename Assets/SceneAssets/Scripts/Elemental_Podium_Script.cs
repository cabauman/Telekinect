using UnityEngine;
using System.Collections;

public class Elemental_Podium_Script : MonoBehaviour
{
	public GameObject Podium_Base;
	public GameObject triggeredObject;
	Elemental_Podium_Challenge_Controller_Script controller;
	public bool completed = false;
	public int elementID = 0;
	
	public bool singularPodium = true ;
	
	float epsilon = 0.8660f ;// 0.8660f is about 30 degrees.
	//epsilon for the direction the podium is facing - cos(30) = .0.8660

// Use this for initializationepsilon
	void Start ()
	{
		if (singularPodium	!= true)
			controller = this.gameObject.transform.parent.gameObject.GetComponent<Elemental_Podium_Challenge_Controller_Script>() ;
	}

// Update is called once per frame
	void Update ()
	{
		if(Network.peerType != NetworkPeerType.Disconnected && Network.isClient)
			return;
		
		//if(this.rigidbody.angularVelocity.y > 0.0f)
		{
			//check the dot product between the Right vector of the Podium and the base, once they are less than the epsilon, completed = true;
		
			if (Vector3.Dot (this.transform.right, Podium_Base.transform.right) >= epsilon )
			{
				if(completed == false )
				{
					completed = true;
					if(Network.peerType != NetworkPeerType.Disconnected && Network.isServer)
					{
						this.gameObject.GetComponent<NetworkView>().RPC("RemoteTriggered", RPCMode.Others);
						this.gameObject.GetComponent<NetworkView>().RPC("AddRemoteElement", RPCMode.Others, elementID);
					}
					if(triggeredObject != null)
						triggeredObject.SendMessage("Triggered", SendMessageOptions.DontRequireReceiver) ;
					
					if(elementID != 0)
					{
						Element_Base element = null;
						switch (elementID)
						{
						case -1:
							element = this.gameObject.AddComponent<Element_Fire>();
							break;
						case 1:
							element = this.gameObject.AddComponent<Element_Water>();
							break;
						case 2:
							element = this.gameObject.AddComponent<Element_Crystal>();
							element.useMesh = true ;
							break;
						case -2:
							element = this.gameObject.AddComponent<Element_Air>();
							break;
						}
						element.isPool = true;
						element.destroyObject = false;
						element.useParticle = true;
						element.canTransfer = true;
					}
					UpdateController() ;
				}
			}
			else 
				if(completed == true)
				{
					completed = false ;
					if(Network.peerType != NetworkPeerType.Disconnected && Network.isServer)
						this.gameObject.GetComponent<NetworkView>().RPC("RemoteUnTriggered", RPCMode.Others);
					if(triggeredObject != null)
						triggeredObject.SendMessage("UnTriggered", SendMessageOptions.DontRequireReceiver) ;
					if(elementID != 0)
					{
						this.gameObject.GetComponent<Element_Base>().DestroyElement();
					}
					UpdateController() ;
				}
				else
					completed = false ;
		}
	}
	
	void UpdateController()
	{
		if(singularPodium != true)
		{
			if (completed==true)
			{
				controller.Podiums_Activated++ ;
				controller.TriggerObject() ;
			}
			else //completed == false
			{
				controller.Podiums_Activated-- ;
				controller.TriggerObject() ;
			}
		}
	}
	
	[RPC]
	public void AddRemoteElement(int elementID)
	{
		Element_Base element = null;
		switch (elementID)
		{
		case -1:
			element = this.gameObject.AddComponent<Element_Fire>();
			break;
		case 1:
			element = this.gameObject.AddComponent<Element_Water>();
			break;
		case 2:
			element = this.gameObject.AddComponent<Element_Crystal>();
			break;
		case -2:
			element = this.gameObject.AddComponent<Element_Air>();
			break;
		}
		element.useParticle = true;	
	}
	
	[RPC]
	public void RemoteTriggered()
	{
		if(triggeredObject != null)
			triggeredObject.SendMessage("Triggered", SendMessageOptions.DontRequireReceiver) ;
	}
	
	[RPC]
	public void RemoteUnTriggered()
	{
		if(triggeredObject != null)
			triggeredObject.SendMessage("UnTriggered", SendMessageOptions.DontRequireReceiver) ;
	}
}

