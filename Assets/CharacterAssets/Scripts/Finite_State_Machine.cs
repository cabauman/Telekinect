using UnityEngine;
using System.Collections;

public abstract class Finite_State_Machine : MonoBehaviour 
{
    public State current_state;

	// Use this for initialization
    public abstract void Start();
	
	// Update is called once per frame
    public abstract void Update();

    public void Change_State(State new_state)
    {
        current_state.OnExit(this);
        current_state = new_state;
        current_state.OnEnter(this);
    }
}

/*------------------- State Interface -------------------*/
//Template for new states. All states must be singleton classes.
//public class New_State : State
//{
//    static private New_State instance;
//    private New_State() { }
//    public static New_State Instance()
//    {
//        if (instance == null)
//            instance = new New_State();
//        return instance;
//    }
//    public override void on_enter(Agent_FSM agent)
//    {
//    }
//    public override void on_execute(Agent_FSM agent)
//    {
//    }
//    public override void on_exit(Agent_FSM agent)
//    {
//    }
//}

public abstract class State
{
    public abstract void OnEnter(Finite_State_Machine FSM);
    public abstract void Update(Finite_State_Machine FSM);
    public abstract void OnExit(Finite_State_Machine FSM);
}