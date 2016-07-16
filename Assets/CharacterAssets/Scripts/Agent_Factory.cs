using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkView))]
abstract public class Agent_Factory : MonoBehaviour 
{
    //These Variables can be tweaked in the Unity Inspector.
    public Transform    spawn_point;            //Trasnform of location to generate Agent
    public float        spawn_rate;             //Seconds between spawns. Low numbers = fast spawns. High numbers = slow spawns.

    public int          total_agents;           //Total number of agents the factory can produce.
    public int          max_active_agents;      //Max number of active agents at a given time.
	
	public List<Transform> wayPoints = new List<Transform>() ; 
	
	public Trigger		trigger ;
	
    private float       time_since_last_spawn;  //Keeps track of the time between spawns.
    private int         active_agents;          //Number of active agents.
	
	public bool			isActive = false;
	public bool			shouldAgentsWander = false ;
	public float		incomingDamageMultiplier = 1.0f ;
	public float		agentWaypointEpsilon = 1.0f ;
	

	// Use this for initialization of static variables
	void Start () 
    {
		if( Network.peerType != NetworkPeerType.Disconnected && Network.isClient )
			this.enabled = false;
		
		else if(Network.isServer && ! GetComponent<NetworkView>().isMine)
		{
			//Make sure Server owns this object's NetworkViewID
			NetworkViewID newID = Network.AllocateViewID();
			GetComponent<NetworkView>().RPC("SetID", RPCMode.OthersBuffered, newID);
			GetComponent<NetworkView>().viewID = newID;
		}

		time_since_last_spawn = spawn_rate; //Match the variables so that when the factory goes active an agent is spawned immediately.
	}

    // Update is called once per frame
    void Update()
    {
		if(!isActive)
			return;
		
        Update_Agents();
        time_since_last_spawn += Time.deltaTime;
    }	

    public void Update_Agents()
    {
        if (total_agents == 0 && active_agents <= 0)
            Self_Destruct();
        
        if (active_agents < max_active_agents)
        {
            if( time_since_last_spawn >= spawn_rate )
            {
                time_since_last_spawn = 0.0f;

                Create_Agent();
                active_agents++;
                total_agents--;
            }
        }
    }

    public abstract void Create_Agent();


    public void Agent_Destroyed()
    { 
        Debug.Log("Agent Destroyed");
        active_agents --; 
    }

    public void Self_Destruct()
    {
        Debug.Log("Factory Dispatched");
		if(Network.isServer)
			this.GetComponent<NetworkView>().RPC("DissolveFactory", RPCMode.All);
		else
			DissolveFactory();
		
		trigger.Activate() ;
    }
	
	public void Triggered()
	{
		isActive = true;	
	}
	
	[RPC]
	public void DissolveFactory()
	{
		//do dissolving here
		
		Destroy(this.gameObject);
	}
		
	[RPC]
	public void SetID(NetworkViewID newID)
	{
		GetComponent<NetworkView>().viewID = newID;
	}
}