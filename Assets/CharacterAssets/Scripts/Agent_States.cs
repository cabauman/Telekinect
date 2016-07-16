using UnityEngine;
using System.Collections;


#pragma warning disable 0618 //warning about using Quaternion.AxisAngle() (depreciated)

//Agent States
public class Agent_Spawn : State
{
    static private Agent_Spawn instance;

    private Agent_Spawn() { }

    public static Agent_Spawn Instance()
    {
        if (instance == null)
            instance = new Agent_Spawn();
        return instance;
    }

    public override void OnEnter(Finite_State_Machine FSM)
    {
        //Cast the Finite_State_Machine FSM input as an Agent_FSM
        Agent_FSM agent = (Agent_FSM)FSM;
        agent.currentInterval = 0.0f;

        //startup with no particles
        ParticleEmitter[] emitters = agent.GetComponentsInChildren<ParticleEmitter>();
        foreach(ParticleEmitter emitter in emitters)
            emitter.emit = false;

       	if(agent.birth_place != null)
			agent.target1 = agent.birth_place.spawn_point ;
		
        agent.GetComponent<Collider>().isTrigger = true;
        agent.gameObject.GetComponent<Rigidbody>().useGravity = false;
        agent.GetComponent<Animation>().CrossFade(agent.idle_animation, 0.3f);
    }

    public override void Update(Finite_State_Machine FSM)
    {
        //Cast the Finite_State_Machine FSM input as an Agent_FSM
        Agent_FSM agent = (Agent_FSM)FSM;

        agent.currentInterval += Time.deltaTime;

        //dissolve in
        float currentDissolve =  Mathf.Max(0.01f, 1 - (agent.currentInterval / agent.dissolveTime));
        foreach(Renderer agentRenderer in agent.agentRenderers)
            agentRenderer.material.SetFloat("_Cutoff", currentDissolve);
		
		if(Network.isServer)
			agent.GetComponent<NetworkView>().RPC("AgentNetworkDissolve", RPCMode.Others, currentDissolve);

        if(agent.currentInterval >= agent.dissolveTime)
          agent.Change_State(Agent_Calm.Instance());
    }

    public override void OnExit(Finite_State_Machine FSM)
    {
        //Cast the Finite_State_Machine FSM input as an Agent_FSM
        Agent_FSM agent = (Agent_FSM)FSM;
        //turn on the particles
        ParticleEmitter[] emitters = agent.GetComponentsInChildren<ParticleEmitter>();
        foreach(ParticleEmitter emitter in emitters)
            emitter.emit = true;

        agent.GetComponent<Collider>().isTrigger = false;
        agent.gameObject.GetComponent<Rigidbody>().useGravity = true;
    }
}

public class Agent_Calm : State
{
    static private Agent_Calm instance;

    private Agent_Calm() { }

    public static Agent_Calm Instance()
    {
        if (instance == null)
            instance = new Agent_Calm();
        return instance;
    }

    public override void OnEnter(Finite_State_Machine FSM)
    {
       	//Cast the Finite_State_Machine FSM input as an Agent_FSM
        Agent_FSM agent = (Agent_FSM)FSM;
		
		if( agent.birth_place != null )
		{
			if(agent.isWandering == false)//follow the path
				agent.steeringPipeline.AddBehavior( FollowPath.Instance() );
			else//isWandering the paths
				agent.steeringPipeline.AddBehavior( WanderPath.Instance() ); 
			
			//agent.steeringPipeline.AddBehavior( ObstacleAvoidance.Instance() );
			agent.steeringPipeline.AddBehavior( Seek.Instance() ) ;		
		}
		
		agent.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero ;
    }

    public override void Update(Finite_State_Machine FSM)
    {
        //Cast the Finite_State_Machine FSM input as an Agent_FSM
        Agent_FSM agent = (Agent_FSM)FSM;
		
		//Check current health
        if (agent.health <= 0.0f)
            agent.Change_State(Agent_Die.Instance());
		
		if( ! Physics.Raycast(agent.gameObject.transform.position, Vector3.down, (1.1f * agent.transform.root.lossyScale.y) ) )
			return ;
		
        //If you can find the player, change to attack state.
        if (agent.Player_Found())
            agent.Change_State(Agent_Attack.Instance());

		agent.gameObject.GetComponent<Rigidbody>().velocity = agent.steeringPipeline.Update(agent) ;
    }

    public override void OnExit(Finite_State_Machine FSM)
    {
		//Cast the Finite_State_Machine FSM input as an Agent_FSM
        Agent_FSM agent = (Agent_FSM)FSM;
		
		agent.steeringPipeline.ClearBehaviors() ;
    }
}

public class Agent_Attack : State
{
    static private Agent_Attack instance;

    private Agent_Attack() { }

    public static Agent_Attack Instance()
    {
        if (instance == null)
            instance = new Agent_Attack();
        return instance;
    }

    public override void OnEnter(Finite_State_Machine FSM)
    {
		//Cast the Finite_State_Machine FSM input as an Agent_FSM
        Agent_FSM agent = (Agent_FSM)FSM;
				
		agent.rotationDampening *= 2;
		
		//agent.steeringPipeline.AddBehavior( ObstacleAvoidance.Instance() );
		agent.steeringPipeline.AddBehavior( Seek.Instance() ) ;
		//agent.steeringPipeline.AddBehavior( Flee.Instance() ) ;

		agent.current_attack_state = Agent_FSM.Agent_Attack_State.IDLE;
		agent.currentInterval = agent.shotInterval;
        agent.GetComponent<Animation>().CrossFade(agent.idle_animation, 0.3f);
    }

    public override void Update(Finite_State_Machine FSM)
    {
		//Cast the Finite_State_Machine FSM input as an Agent_FSM
        Agent_FSM agent = (Agent_FSM)FSM;

        //check if the target (player) has gone out of range (square of the distance from us to the target is greater than square of our range)
        //faster than doing the overlap sphere every frame (10/8/2011 - Dan Dunham)
        if ((agent.target1.position - agent.transform.position).sqrMagnitude > (agent.range * agent.range) || (agent.target1 == null && agent.current_attack_state == Agent_FSM.Agent_Attack_State.IDLE))
		{
            agent.Change_State(Agent_Calm.Instance());
			return ;
		}
		
		if( ! Physics.Raycast(agent.gameObject.transform.position, Vector3.down, ( 1.1f * agent.transform.root.lossyScale.y) ))//if not on the ground, return
			return;
		
		agent.faceTarget = true ;
				
		agent.gameObject.GetComponent<Rigidbody>().velocity = agent.steeringPipeline.Update(agent) ;
				
		if( (agent.gameObject.transform.position - agent.target1.position).sqrMagnitude <= (agent.minFiringDistance * agent.minFiringDistance) )
			agent.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero ;
				
		agent.currentInterval -= Time.deltaTime;

		switch(agent.current_attack_state)
		{
			case Agent_FSM.Agent_Attack_State.IDLE:
			
			if( (agent.gameObject.transform.position - agent.target1.position).sqrMagnitude <= (agent.firingRange * agent.firingRange) )//if within firing range
				
				if(agent.currentInterval <= 0.0f)
				{
					agent.current_attack_state = Agent_FSM.Agent_Attack_State.ANIMATING;
					agent.currentInterval = agent.animationInterval;
					agent.GetComponent<Animation>().CrossFade(agent.attack_animation, 0.3f);
				}
	
				break;

			case Agent_FSM.Agent_Attack_State.ANIMATING:
				
				agent.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero ;

				if(agent.currentInterval <= 0.0f )
				{
				    agent.currentInterval = agent.shotInterval;
				    agent.current_attack_state = Agent_FSM.Agent_Attack_State.IDLE;
				    agent.GetComponent<Animation>().CrossFade(agent.idle_animation, 0.3f);
				}

				break;

		}

        //Check current health
        if (agent.health <= 0.0f)
            agent.Change_State(Agent_Die.Instance());
    }

    public override void OnExit(Finite_State_Machine FSM)
    {
        //Cast the Finite_State_Machine FSM input as an Agent_FSM
        Agent_FSM agent = (Agent_FSM)FSM;
		
		agent.steeringPipeline.ClearBehaviors() ;
		agent.faceTarget = false ;
        agent.target1 = null;
    }
}

public class Agent_Die : State
{
    static private Agent_Die instance;

    private Agent_Die() { }

    public static Agent_Die Instance()
    {
        if (instance == null)
            instance = new Agent_Die();
        return instance;
    }

    public override void OnEnter(Finite_State_Machine FSM)
    {
        //Cast the Finite_State_Machine FSM input as an Agent_FSM
        Agent_FSM agent = (Agent_FSM)FSM;

        agent.currentInterval = 0.0f;

        //turn off the particles
        ParticleEmitter[] emitters = agent.GetComponentsInChildren<ParticleEmitter>();
        foreach(ParticleEmitter emitter in emitters)
            emitter.emit = false;
		
		agent.gameObject.GetComponent<Animation>().Stop();
		
		Collider[] colliders = agent.GetComponentsInChildren<Collider>() ;
		foreach(Collider collider in colliders)
		{
			collider.isTrigger = false ;
			collider.gameObject.GetComponent<Rigidbody>().isKinematic = false ;
		}
		agent.GetComponent<Collider>().isTrigger = true ;

		if(Network.isServer)
		{
			agent.GetComponent<NetworkView>().RPC("AgentDisableParticles", RPCMode.Others);
			//agent.networkView.RPC("AgentNetworkDisassemble", RPCMode.Others) ;
		}
		
        //agent.gameObject.rigidbody.isKinematic = true;
		agent.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        agent.gameObject.GetComponent<Collider>().isTrigger = true; //dont' recieve any more collisions
        agent.gameObject.GetComponent<Rigidbody>().useGravity = false;

		//Drop a goodie
		if(agent.drop != null)
		{
			if( Network.peerType == NetworkPeerType.Disconnected )
				Object.Instantiate(agent.drop, agent.transform.position, Quaternion.identity);
			else
				Network.Instantiate(agent.drop, agent.transform.position, Quaternion.identity, 0);
		}
    }

    public override void Update(Finite_State_Machine FSM)
    {
        //Cast the Finite_State_Machine FSM input as an Agent_FSM
        Agent_FSM agent = (Agent_FSM)FSM;

        agent.currentInterval += Time.deltaTime;

        //dissolve out
        float currentDissolve = agent.currentInterval / agent.dissolveTime;

        foreach(Renderer agentRenderer in agent.agentRenderers)
            agentRenderer.material.SetFloat("_Cutoff", currentDissolve);

		if(Network.isServer)
			agent.GetComponent<NetworkView>().RPC("AgentNetworkDissolve", RPCMode.Others, currentDissolve);

        if(agent.currentInterval >= agent.dissolveTime)
        {
            if (agent.birth_place != null)
                agent.birth_place.Agent_Destroyed();
            
			if(Network.isServer)
			{
				Network.RemoveRPCs(agent.GetComponent<NetworkView>().viewID);
				Network.Destroy(agent.GetComponent<NetworkView>().viewID);
			}
			else
				Object.Destroy(agent.gameObject);
        }
    }

    public override void OnExit(Finite_State_Machine FSM)
    {

    }
}