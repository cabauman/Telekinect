using UnityEngine;
using System.Collections;

public class Factory_Water_Elemental : Agent_Factory 
{
    static int factory_ID = 1;

    public override void Create_Agent()
    {
		GameObject Drippy = null;

		if( Network.isServer )
			Drippy = (GameObject)Network.Instantiate(Resources.Load("Enemy_Water_Elemental"), spawn_point.position , spawn_point.rotation, 0);
		else
            Drippy = (GameObject)Instantiate(Resources.Load("Enemy_Water_Elemental"), spawn_point.position, spawn_point.rotation);

        Agent_FSM Drippy_FSM = Drippy.gameObject.GetComponent<Agent_FSM>();


        Drippy_FSM.birth_place = this;
        Drippy_FSM.ID = factory_ID;
    }
}