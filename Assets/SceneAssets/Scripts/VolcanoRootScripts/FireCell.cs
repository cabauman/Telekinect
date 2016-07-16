using UnityEngine;
using System.Collections;

public class FireCell : MonoBehaviour 
{

    public float timeToFlame = 5.0f;
    public Element_Fire myFire;
    public RootScriptWater myRoot;


	// Use this for initialization
	void Start () 
    {
		if( Network.isClient )
			this.enabled = false;
		else if(Network.isServer && ! GetComponent<NetworkView>().isMine)
		{
			//Make sure Server owns this object's NetworkViewID
			NetworkViewID newID = Network.AllocateViewID();
			GetComponent<NetworkView>().RPC("SetID", RPCMode.OthersBuffered, newID);
			GetComponent<NetworkView>().viewID = newID;
		}
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnTriggerEnter(Collider other)
    {
		if ( ! this.enabled )
			return;

        if(myFire == null && other.gameObject.GetComponent<Element_Fire>() != null)
        {
		
//			Element_Base[] oldElements = other.gameObject.GetComponents<Element_Base>();
//
//			foreach(Element_Base element in oldElements)
//				element.DestroyElement();

            //catch on fire
			CatchFire();
			if ( Network.peerType != NetworkPeerType.Disconnected )
				GetComponent<NetworkView>().RPC("CatchFire", RPCMode.OthersBuffered);

        }
    }

    void OnTriggerStay(Collider other)  //this gets called every frame for every object that is touching, so the more objects that are touching and on fire, the faster this one will catch
    {

		if ( ! this.enabled )
			return;

        if( myFire == null )
        {
            Element_Fire otherFire = other.gameObject.GetComponent<Element_Fire>();
            if( otherFire != null)
            {
                timeToFlame -= Time.deltaTime;
            
                if(timeToFlame <= 0.0f)
                {
                    //catch on fire
					CatchFire();
					if ( Network.peerType != NetworkPeerType.Disconnected )
						GetComponent<NetworkView>().RPC("CatchFire", RPCMode.OthersBuffered);
                }
            }
        }
    }

	[RPC]
	public void CatchFire()
	{
        myFire =  this.gameObject.AddComponent<Element_Fire>();
        myFire.canTransfer = true;
        myFire.isPool = true;
        myFire.useParticle = true;
		if(myRoot != null)
        	myRoot.CheckCells();
	}
	
	[RPC]
	public void SetID(NetworkViewID newID)
	{
		GetComponent<NetworkView>().viewID = newID;
	}
}
