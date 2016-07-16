using UnityEngine;
using System.Collections;

public class RootScript : MonoBehaviour 
{

    int flamingCellCount = 0;           //number of cells on fire
    bool burnComplete = false;
    float fadeTime = 3.0f;
    public FireCell[] cells;

	// Use this for initialization
	void Start ()
    {

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
                Destroy(this.gameObject);
            }
        }
	}

    public void CheckCells()            //gets called by a cell when it flames
    {
        flamingCellCount++;
        if(flamingCellCount >= cells.Length)
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
}
