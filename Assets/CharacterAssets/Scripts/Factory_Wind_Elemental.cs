using UnityEngine;
using System.Collections;

public class Factory_Wind_Elemental : Agent_Factory 
{
    static int factory_ID = -2;

    public override void Create_Agent()
    {
        GameObject Windy = null;

		if( Network.isServer )
			Windy = (GameObject)Network.Instantiate(Resources.Load("Enemy_Wind_Elemental"), spawn_point.position , Quaternion.identity, 0);
		else
            Windy = (GameObject)Instantiate(Resources.Load("Enemy_Wind_Elemental"));

        Agent_FSM Windy_FSM = Windy.gameObject.GetComponent<Agent_FSM>();
        
        Windy_FSM.birth_place = this;
        Windy_FSM.ID = factory_ID;
    }
}