/*
using UnityEngine;
using System.Collections;

abstract public class Steer
{
	// Update is called once per frame
    public abstract Vector3 Update( ref Agent_FSM agent );
}

//This behavior only effects the rotation of the agent.  Not the Veclocity.
//The rotation is set so the agent is facing its target, then a zero vector
//is returned so the velocity will be untouched.
public class Face_Target : Steer
{
    static private Face_Target instance;

    private Face_Target() { }

    public static Face_Target Instance()
    {
        if (instance == null)
            instance = new Face_Target();
        return instance;
    }

    public override Vector3 Update(ref Agent_FSM agent)
    {
        if( agent.target1 != Vector3.zero )
            agent.transform.rotation = Quaternion.LookRotation(agent.target1 - agent.transform.position);

        return Vector3.zero;
    }
}

public class Seek : Steer
{
    static private Seek instance;

    private Seek() {}

    public static Seek Instance() 
    {
        if (instance == null)
            instance = new Seek();
        return instance;
    }

    public override Vector3 Update(ref Agent_FSM agent)
    {
        Vector3 velocity = Vector3.zero;

        if (agent.seek_target != null)
        {
            velocity = (agent.seek_target.transform.position - agent.transform.position);
            Debug.DrawLine(agent.transform.position, agent.seek_target.position, Color.blue);
        }

        if (velocity.magnitude > agent.max_speed)
        {
            velocity.Normalize();
            velocity *= agent.max_speed;
        }

        velocity.y = 0.0f;
        return velocity;
    }
}

public class Flee : Steer 
{
    static private Flee instance;

    private Flee() {}

    public static Flee Instance() 
    {
        if (instance == null)
            instance = new Flee();
        return instance;
    }

    public override Vector3 Update( ref Agent_FSM agent )
    {
        Vector3 velocity = Vector3.zero;
        

        if (agent.flee_target != null)
        {
            float dist = (agent.transform.position - agent.flee_target.transform.position).magnitude;
            velocity = (agent.transform.position - agent.flee_target.transform.position)  / dist;
            //Debug.DrawLine(agent.transform.position, agent.flee_target.transform.position, Color.yellow);
        }

        velocity.y = 0.0f;
        return velocity;
    }
}

public class Wander : Steer
{
    public float wander_offset = 3.0f;
    public float wander_radius = 3.0f;

    static private Wander instance;

    private Wander() {}

    public static Wander Instance() 
    {
        if (instance == null)
            instance = new Wander();
        return instance;
    }

    public override Vector3 Update( ref Agent_FSM agent )
    {
        Vector3 target = agent.transform.position + (agent.transform.forward.normalized * wander_offset);
        float wander_orientation = RNG.Instance().fUni(-wander_radius, wander_radius);

        target += agent.transform.right * wander_orientation;

        Debug.DrawLine(agent.transform.position, target, Color.black);

        return target;
    }
}

public class ObstacleAvoidance : Steer
{
    static private ObstacleAvoidance instance;

    public float feeler_length = 1.0f;

    private ObstacleAvoidance() {}

    public static ObstacleAvoidance Instance() 
    {
        if (instance == null)
            instance = new ObstacleAvoidance();
        return instance;
    }

    public override Vector3 Update( ref Agent_FSM agent )
    {
        RaycastHit hit_info;
        Vector3 velocity = Vector3.zero;
        Vector3 ray_start = agent.transform.position;
        ray_start.y -= 0.25f;

        //Debug.DrawLine(ray_start, ray_start + ( agent.transform.forward * feeler_length ), Color.green); //Ray

        if (Physics.Raycast(ray_start, agent.transform.forward, out hit_info, feeler_length))
        {
            float depth = feeler_length - hit_info.distance;

            velocity = hit_info.normal * depth * 2.0f;

            Debug.DrawLine(hit_info.point, hit_info.point + hit_info.normal, Color.red); //Collision normal
        }
        else
            velocity = agent.transform.forward * agent.max_speed;

        velocity.y = 0.0f;

        return velocity;
    }
}

public class Separation : Steer
{
    static private Separation instance;

    private Separation() { }

    public static Separation Instance()
    {
        if (instance == null)
            instance = new Separation();
        return instance;
    }

    public override Vector3 Update(ref Agent_FSM agent)
    {
        //look for other agents in the neighborhood

        Vector3 velocity = Vector3.zero;
        Collider[] neighbors = Physics.OverlapSphere(agent.transform.position, 0.25f, 0x1000);
        foreach (Collider neighbor in neighbors)
        {
            if (neighbor.gameObject == agent.gameObject)
                continue;

            velocity += (agent.transform.position - neighbor.transform.position) / (agent.transform.position - neighbor.transform.position).magnitude;
        }

        return velocity;
    }
}
*/