using UnityEngine;
using System.Collections;

public class RootScriptWater : MonoBehaviour 
{

    int flamingCellCount = 0;           //number of cells on fire
    bool burnComplete = false;
    float fadeTime = 3.0f;
    public FireCell[] cells;

	// Use this for initialization
	void Start ()
    {
		if(Network.peerType != NetworkPeerType.Disconnected && Network.isClient)
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
	    if(burnComplete)
        {
            fadeTime -= Time.deltaTime;
            this.GetComponent<Renderer>().material.SetFloat("_Cutoff", 1 - (fadeTime / 3.0f));

            if(fadeTime <= 0.0f)
            {
				if(Network.peerType != NetworkPeerType.Disconnected)
                	Network.Destroy(this.gameObject);
				else
					Destroy(this.gameObject);
            }
        }
	}

    public void CheckCells()            //gets called by a cell when it flames
    {
        flamingCellCount++;
    }
	
	void OnCollisionEnter(Collision collision)
	{		
		if(this.enabled == false)
			return;
		
		Element_Water water = collision.collider.gameObject.GetComponent<Element_Water>();
		if(water != null && flamingCellCount >= cells.Length)
		{
            //burning complete, gracefully kill ourselves - in a fire.
            BurnComplete();
			if(Network.peerType != NetworkPeerType.Disconnected)
				this.GetComponent<NetworkView>().RPC("BurnComplete", RPCMode.OthersBuffered);
            
            foreach(FireCell cell in cells)
            {
                ParticleEmitter[] emitters;
                emitters = cell.gameObject.GetComponentsInChildren<ParticleEmitter>();
                foreach(ParticleEmitter emitter in emitters)
                {
                    emitter.emit = false;
                }
            }				
		}
	}

	[RPC]
	public void BurnComplete()
	{
		burnComplete = true;
	}
	
	[RPC]
	public void SetID(NetworkViewID newID)
	{
		GetComponent<NetworkView>().viewID = newID;
	}
}
