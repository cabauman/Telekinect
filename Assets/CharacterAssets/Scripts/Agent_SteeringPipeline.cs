using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentSteeringPipeline
{
	List<SteeringBehavior> steeringBehaviorList = new List<SteeringBehavior>();
	
	public Vector3 Update( Agent_FSM agent)
	{	
		//Debug.DrawLine(agent.gameObject.transform.position, agent.transform.root.position + Vector3.down * agent.transform.root.localScale.y, Color.cyan );
		
		Vector3 velocity = Vector3.zero;

		foreach(SteeringBehavior steer in steeringBehaviorList)
		{
			Vector3 currentVelocity = steer.Update(agent);
			if((velocity + currentVelocity).sqrMagnitude >= agent.maxPipelineVelocity * agent.maxPipelineVelocity)
			{
				float leftOver = agent.maxPipelineVelocity - velocity.magnitude;
				currentVelocity = currentVelocity.normalized * leftOver;
				velocity = velocity + currentVelocity;
				break;	
			}
			
			velocity = velocity + currentVelocity;
		}
	
	
		velocity = velocity.normalized * agent.max_speed;
		velocity.y = agent.GetComponent<Rigidbody>().velocity.y;
		return velocity;
		
	}
	
	public void AddBehavior(SteeringBehavior behavior)
	{
		foreach(SteeringBehavior steer in steeringBehaviorList)
			if (steer == behavior)
				return;
		
		steeringBehaviorList.Add(behavior);	
	}
	
	public void RemoveBehavior(SteeringBehavior behavior)
	{
		steeringBehaviorList.Remove(behavior);
	}
	
	public void ClearBehaviors()
	{
		steeringBehaviorList.Clear();	
	}
}

//Steering Behaviors must be implemented as singletons
public abstract class SteeringBehavior
{
	public abstract Vector3 Update( Agent_FSM agent);
}

//Steering behavior for seeking towards target defined in the Agent_FSM::target1
public class Seek : SteeringBehavior
{
	static private Seek instance;

    private Seek() { }

    public static Seek Instance()
    {
        if (instance == null)
            instance = new Seek();
        return instance;
    }
	
	public override Vector3 Update( Agent_FSM agent)
	{
		if(agent.target1 == null)
			return Vector3.zero;
		
		Vector3 toTarget = agent.target1.position - agent.gameObject.transform.position;
		toTarget.y = 0 ;
		return (toTarget.normalized * agent.maxPipelineVelocity) ;
	}
}

public class Flee : SteeringBehavior
{
	static private Flee instance;
	private Flee() {}
	
	public static Flee Instance()
	{
		if (instance == null)
			instance = new Flee();
		return instance ;
	}
	
	public override Vector3 Update( Agent_FSM agent)
	{
		Vector3 fromTarget = agent.gameObject.transform.position - agent.target1.position;
		return (fromTarget.normalized * agent.maxPipelineVelocity) ;
	}
}

public class ObstacleAvoidance : SteeringBehavior
{
	static private ObstacleAvoidance instance ;
	private ObstacleAvoidance() {}
	
	public static ObstacleAvoidance Instance() 
	{
		if (instance == null)
			instance = new ObstacleAvoidance() ;
		return instance ;
	}
	
	public override Vector3 Update ( Agent_FSM agent)
	{
		//ray cast to get obstacle
		RaycastHit hitInfo ;
		
		if ( Physics.Raycast(agent.gameObject.transform.position, agent.gameObject.GetComponent<Rigidbody>().velocity , out hitInfo, 2.0f ) )
		{
			Vector3 DesiredVelocity = agent.gameObject.GetComponent<Rigidbody>().velocity ;
			DesiredVelocity = DesiredVelocity - (Vector3.Dot(DesiredVelocity, hitInfo.normal) * hitInfo.normal) ;
			DesiredVelocity = DesiredVelocity.normalized * agent.maxPipelineVelocity ;
			
			Debug.DrawRay(agent.gameObject.transform.position, agent.gameObject.GetComponent<Rigidbody>().velocity, Color.blue);
			Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red);
			Debug.DrawRay(agent.gameObject.transform.position, DesiredVelocity, Color.green);
			
			return DesiredVelocity ;
		}
		
		return Vector3.zero ;
	}
}

public class FollowPath : SteeringBehavior
{
	static private FollowPath instance ;
		
	private FollowPath() {}

	public static FollowPath Instance()
	{
		if(instance == null)
			instance = new FollowPath() ;
		
		return instance ;
	}
		
	public void SetNewWaypoint(Agent_FSM agent)
	{
		if ( agent.birth_place.wayPoints.Count <= 1 )
		{
			agent.target1 = agent.birth_place.wayPoints[0] ;
			return ;
		}
		
		if ( ++agent.currentWaypoint >= agent.birth_place.wayPoints.Count )
		{
			agent.currentWaypoint = 0 ;
		}
		
		agent.target1 = agent.birth_place.wayPoints[agent.currentWaypoint] ;
	}
	
	
	public override Vector3 Update ( Agent_FSM agent )
	{
		if(agent.birth_place == null)//if no birth place, do not follow 
			return Vector3.zero ;
		
		if(agent.target1 == null)
		{
			agent.target1 = agent.birth_place.wayPoints[agent.currentWaypoint] ;
		}
		
		//check to see if near "current" waypoint then seek toward it
		float distSqr = ((agent.gameObject.transform.position + (Vector3.down * agent.transform.root.lossyScale.y))- agent.birth_place.wayPoints[agent.currentWaypoint].position).sqrMagnitude ;
		if( (distSqr <= (agent.waypointEpsilon * agent.waypointEpsilon)) )
			SetNewWaypoint(agent);
		
		return Vector3.zero; 
	}
}

public class WanderPath : SteeringBehavior //Put this steering behavior first in the pipeline as it returns zero
{
	static private WanderPath instance ;
		
	private WanderPath() {}
	
	public static WanderPath Instance()
	{
		if(instance == null)
			instance = new WanderPath() ;
		return instance ;
	}
			
	public void SetNewWaypoint(Agent_FSM agent)
	{
		if (agent.birth_place.wayPoints.Count <= 1)
		{
			agent.target1 = agent.birth_place.wayPoints[0] ;
			return ;
		}
		
		int randomWaypoint ;
		
		do
		{
			randomWaypoint = (int)RNG.Instance().fUni(0.0f, agent.birth_place.wayPoints.Count) ;

		}while (agent.birth_place.wayPoints[randomWaypoint] == agent.target1 ) ;
		
		agent.currentWaypoint = randomWaypoint ;
		agent.target1 = agent.birth_place.wayPoints[randomWaypoint] ;
	}
	
	public override Vector3 Update ( Agent_FSM agent )
	{
		if(agent.birth_place == null) //if no birth place, do not wander
			return Vector3.zero ;
		
		if(agent.target1 == null)
		{
			agent.target1 = agent.birth_place.wayPoints[agent.currentWaypoint] ;
		}
		
		//check to see if near "current" waypoint then seek toward it
		float distSqr = (agent.gameObject.transform.position + (Vector3.down * agent.transform.root.lossyScale.y) - agent.birth_place.wayPoints[agent.currentWaypoint].position).sqrMagnitude ;
		
		if(distSqr <= (agent.waypointEpsilon * agent.waypointEpsilon))
			SetNewWaypoint(agent);
		
		return Vector3.zero ;
	}
}
