using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*------------------- Agent Finite State Machine --------------------*/
[RequireComponent(typeof(NetworkView))]
public class Agent_FSM : Finite_State_Machine 
{

	public enum Agent_Attack_State {IDLE, ANIMATING, FIRING};

    public Agent_Factory    	birth_place; 
    public int                  ID;
    public float                health;
    public float                range;
    public float                max_speed;
	public float				damageMultiplier ;
	
	
	[HideInInspector]
	public AgentSteeringPipeline steeringPipeline = new AgentSteeringPipeline() ;
	
	public float				rotationDampening = 2.0f ;
	
	//[HideInInspector]
	public bool					isWandering = false ;//use to deferentiate for path wondering or following
	
	public Transform			target1; //typically used for seeking type behaviors
	public Transform			target2; //typically used for fleeing type behaviors
	public float 				maxPipelineVelocity;//maximum velocity allotted to the pipeline
	public int 					currentWaypoint ;//integer of the array of waypoints
	public float 				waypointEpsilon = 1.0f ;//how far from the current target must the agent be to be "at" it.
	public float				firingRange = 40.0f ;//how far from the player can Agent start firing
	public float 				minFiringDistance = 20.0f ;//minimum distance agent can be from player and still fire
	
//	public float				distanceToCauseFlee = 2.0f ;//how close the player must get to cause the agent to flee away.
//	public float				distancetoEndFlee = 5.0f ; //distance the agent must be to end flee and return to other tasks.
	
	public bool					faceTarget = false ;
	
    public GameObject			projectile;	//projectile for the Agent to fire (may cause other projectiles to fire as well)
	public GameObject			drop; //drop of the element of the agent, if used.
	
	public bool					projectileUsesGeneratorOrExplosion = false ;//if the projectile uses a generator or explosion, the Fire function behaves differently.
    public float				projectileSpeed; //speed of the projectile from the agent. May need to look at the projectile fired to speed, depending on agent
	public Transform			projectilsSpawnLocation ;
    public float 				shotInterval = 3.0f; //interval between the firing of projectiles
	public float				animationInterval = 1.0f; //interval for animation plays
    public float				currentInterval; //current point of animation invterval
	public string				idle_animation;
	public string				attack_animation;
	
    public float				dissolveTime = 2.0f; //time for desloving the agent in and out
    public Agent_Attack_State	current_attack_state;
	
	public Renderer[] 			agentRenderers;//renderes of the agent to dissolve
	
	[HideInInspector]
	GameObject 					projectileGo ; //object reference to the projectile currently being fired
	
    // Use this for initialization
    public override void Start()
    {
		agentRenderers = GetComponentsInChildren<Renderer>();

		if(Network.isClient)
		{
			this.enabled = false;
			return;
		}
		else if(Network.isServer && ! GetComponent<NetworkView>().isMine)
		{
			//Make sure Server owns this object's NetworkViewID
			NetworkViewID newID = Network.AllocateViewID();
			GetComponent<NetworkView>().RPC("SetID", RPCMode.OthersBuffered, newID);
			GetComponent<NetworkView>().viewID = newID;
		}

        health = RNG.Instance().fTri(90.0f, 200.0f, 100.0f);
		
		if(health > 100)
		{
			float temp = health * 0.01f ;
			this.transform.root.localScale = new Vector3 (temp, temp, temp) ;
		}
		
		this.transform.root.position = birth_place.spawn_point.position + (Vector3.up * this.transform.localScale.y );
		
		if(birth_place != null )
		{
			isWandering = birth_place.shouldAgentsWander ;
			waypointEpsilon = birth_place.agentWaypointEpsilon ;
			damageMultiplier = birth_place.incomingDamageMultiplier ;
		}
		
        current_state = Agent_Spawn.Instance();
        current_state.OnEnter(this);
    }

    // Update is called once per frame
    public override void Update()
    {		
        current_state.Update(this);
		
		if((target1 != null))
		{
			if( faceTarget )
			{
				Vector3 toTarget = (this.target1.position - this.gameObject.transform.position) ; toTarget.y = 0.0f;
				
				this.gameObject.transform.rotation = Quaternion.Slerp( this.gameObject.transform.rotation, Quaternion.LookRotation(toTarget), Time.deltaTime * rotationDampening) ;
			}
			else//face the current velocity
			{	
				if(this.gameObject.GetComponent<Rigidbody>().velocity == Vector3.zero)
				{
					return ;
					//this.gameObject.transform.rotation = Quaternion.Slerp(this.gameObject.transform.rotation, Quaternion.identity, Time.deltaTime) ;
				}
				Vector3 velocity = new Vector3 (this.gameObject.GetComponent<Rigidbody>().velocity.x, 0.0f, this.gameObject.GetComponent<Rigidbody>().velocity.z);
				this.gameObject.transform.rotation = Quaternion.Slerp( this.gameObject.transform.rotation, Quaternion.LookRotation(velocity), Time.deltaTime * rotationDampening) ;
			}
		}
    }

    public bool Player_Found()
    {
        //Layer Masks
        int searchMask = ~(1 << 9 | 1 << 2) ;

        //Check if the player is within the agents range
        Collider[] players = Physics.OverlapSphere(transform.position, range, 1 << 9 );
        if (players.Length > 0)
        {
            //direction and distance 
			float closestDistance = Mathf.Infinity;
			Collider closestTarget = null;
			Vector3 direction = new Vector3();
			float distance = closestDistance;
			foreach (Collider player in players)
			{
	            direction = (player.gameObject.transform.position - this.gameObject.transform.position);
	            distance  = direction.magnitude;
				if(distance < closestDistance)
				{
					closestDistance = distance;
					closestTarget = player;
				}
			}

            //Cast a ray to make sure there is nothing between the agent and the player (check against all colliders except the player)
			direction = closestTarget.gameObject.transform.position - this.gameObject.transform.position ;
			
            if (Physics.Raycast(this.gameObject.transform.position, direction, distance, searchMask))
			{   
				//TODO ObstacleAvoidance
				//this.steeringPipeline.AddBehavior(ObstacleAvoidance.Instance() ) ;
				return false;
			}	
            else
            {
                //The player is now the agents target.
				//TODO replace this with Target1 of the pipeline and have it seek/obstacle avoid toward it.
				this.target1 = closestTarget.gameObject.transform;
                return true;
            }
        }
        else
            return false;
    }
	
	public void Create_Projectile( )
	{
		//if we are multiplayer, instantiate across the network
		if(Network.isServer)
		{
			projectileGo = (GameObject)Network.Instantiate(this.projectile, projectilsSpawnLocation.position, this.gameObject.transform.rotation, 0);
		}
		//otherwise just instantiate locally
		else
			projectileGo = (GameObject)GameObject.Instantiate(this.projectile, projectilsSpawnLocation.position, this.gameObject.transform.rotation);
		
		Projectile_Script projectileObj = this.projectileGo.GetComponent<Projectile_Script>();
		projectileObj.creator = this.gameObject;
		
		if( projectileUsesGeneratorOrExplosion == false)
		{
			projectileGo.GetComponent<Rigidbody>().useGravity = false ;
		}		
		
		projectileGo.transform.parent = this.gameObject.transform ;		

	}
	
	public void Fire_Projectile( ) //called from animation system during animation
	{
		
		if(target1 == null) //target 1 has left the agent's range of attack.
		{
			Destroy(projectileGo) ;
			projectileGo = null ;
		}
				
		if(projectileGo == null) //return if the projectile game obj got destroyed in some fashion
			return;
				
		if( projectileUsesGeneratorOrExplosion == false)
		{
			projectileGo.transform.parent = null ;
			projectileGo.GetComponent<Rigidbody>().useGravity = true ;
		    
			Vector3 shotDirection = (target1.position - this.gameObject.GetComponent<Rigidbody>().position);
		
		    //ballistic calculation if the projectile is using gravity
		    if (projectileGo.GetComponent<Rigidbody>().useGravity)
		    {
		        float upAngle = Mathf.Asin((9.8f * shotDirection.magnitude) / (projectileSpeed * projectileSpeed)) / 2.0f;
		
		        //Unity complains about an obsolete function, but we want to use it because it uses radians (the new one is in degrees) - keeps from having to do a conversion -Dan
		        shotDirection = Quaternion.AxisAngle(-this.gameObject.transform.right, upAngle) * shotDirection;
		    }
			
		    shotDirection.Normalize();
		    projectileGo.GetComponent<Rigidbody>().velocity = (shotDirection) * this.projectileSpeed;
		}
		
		//projectileGo.GetComponent<Element_Base>().AttachElementParticles() ;		
		
		projectileGo = null ;
	}
	
    void OnCollisionEnter(Collision collision)
    {
		if(Network.isClient || ! this.enabled || collision.rigidbody == null || collision.gameObject.layer != 13) //13 is a movable object - meaning that only things that are movable will damage enemies.
			return;
		
		float hitPower = Vector3.Dot(collision.contacts[0].normal, collision.relativeVelocity) * collision.gameObject.GetComponent<Rigidbody>().mass * damageMultiplier;
		
		if(hitPower > 10.0f)
		{
			Element_Base tempElement = collision.transform.root.GetComponent<Element_Base>() ;
			if(tempElement != null)
			{
				if(tempElement.ID + this.ID == 0)
				{
					//extra damage | modify hitpower +
				}
				else if( tempElement.ID == this.ID)
				{
					//gain health | Invert Hitpower (-hitpower)
				}
//				else
//				{
//					//normal damage	
//				}
			}
			
			this.health -= hitPower;	
			//scale by the health * .01, minimum is 1.0
		}	
		
		
		//scale according to health - plus or minus depending on element collision
    }

	void OnSerializeNetworkView ( BitStream stream, NetworkMessageInfo info )
	{
		Vector3 pos = Vector3.zero;
		Vector3 velocity = Vector3.zero;
		Quaternion rot = Quaternion.identity;
		Vector3 angularVelocity = Vector3.zero;
		float healthScale = 0.0f; 
		
		// Send data 
		if ( stream.isWriting )
		{
			//Debug.Log("Writing to Stream");

			pos = GetComponent<Rigidbody>().position;
			rot = GetComponent<Rigidbody>().rotation;
			velocity = GetComponent<Rigidbody>().velocity;
			angularVelocity = GetComponent<Rigidbody>().angularVelocity;
			//healthScale = gameObject.transform.lossyScale.x ;
			
			stream.Serialize(ref pos);
			stream.Serialize(ref velocity);
			stream.Serialize(ref rot);
			stream.Serialize(ref angularVelocity);
			//stream.Serialize(ref healthScale) ;
		}
		// Read data
		else
		{
			stream.Serialize(ref pos);
			stream.Serialize(ref velocity);
			stream.Serialize(ref rot);
			stream.Serialize(ref angularVelocity);
			//stream.Serialize(ref healthScale) ;

			GetComponent<Rigidbody>().position = pos;
			GetComponent<Rigidbody>().rotation = rot;
			GetComponent<Rigidbody>().velocity = velocity;
			GetComponent<Rigidbody>().angularVelocity = angularVelocity;
			//gameObject.transform.localScale = new Vector3( healthScale, healthScale, healthScale);
		}
	}

	[RPC]
	void SetID( NetworkViewID newID )
	{
		Debug.Log("Agent network ID set!");
		this.GetComponent<NetworkView>().viewID = newID;
	}
	
	[RPC]
	void AgentNetworkDissolve( float currentDissolve, NetworkMessageInfo info )
	{
		foreach(Renderer agentRenderer in agentRenderers)
            agentRenderer.material.SetFloat("_Cutoff", currentDissolve);
	}
	
	[RPC]
	void AgentNetworkDisassemble(NetworkMessageInfo info)
	{
		Collider[] colliders = this.GetComponentsInChildren<Collider>() ;
		foreach(Collider collider in colliders)
		{
			collider.isTrigger = false ;
			collider.gameObject.GetComponent<Rigidbody>().isKinematic = false ;
		}
		this.GetComponent<Collider>().isTrigger = true ;
	}

	[RPC]
	void AgentDisableParticles( NetworkMessageInfo info )
	{
        ParticleEmitter[] emitters = GetComponentsInChildren<ParticleEmitter>();
        foreach(ParticleEmitter emitter in emitters)
            emitter.emit = false;
	}

	[RPC]
	void AgentNetworkScale( Vector3 scale, NetworkMessageInfo info )
	{
		this.transform.root.localScale = scale;
	}
	
	[RPC]
	void SetPlayerNetViewID( NetworkViewID playerViewID )
	{	
	}
}