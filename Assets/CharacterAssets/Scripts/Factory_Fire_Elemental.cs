using UnityEngine;
using System.Collections;

public class Factory_Fire_Elemental : Agent_Factory 
{
    static int factory_ID = -1;

    public override void Create_Agent()
    {
		GameObject fireElemental = null;

		if( Network.isServer )
			fireElemental = (GameObject)Network.Instantiate(Resources.Load("Enemy_Fire_Elemental"), spawn_point.position , Quaternion.identity, 0);
		else
            fireElemental = (GameObject)Instantiate(Resources.Load("Enemy_Fire_Elemental"), spawn_point.position , Quaternion.identity);

        Agent_FSM  fireElemental_FSM = fireElemental.gameObject.GetComponent<Agent_FSM>();

        fireElemental_FSM.birth_place = this;
        fireElemental_FSM.ID          = factory_ID;
    }	
}